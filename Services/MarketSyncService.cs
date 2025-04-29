using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using automation.mbtdistr.ru.Data;
using automation.mbtdistr.ru.Models;
using automation.mbtdistr.ru.Services.Ozon;
using automation.mbtdistr.ru.Services.Wildberries;
using Telegram.Bot;
using static automation.mbtdistr.ru.Models.Internal;
using static System.Formats.Asn1.AsnWriter;
using System.Text.Json;
using automation.mbtdistr.ru.Services.Ozon.Models;
using System.Text;
using Telegram.Bot.Types.Enums;
using System.Text.RegularExpressions;
using Telegram.Bot.Types;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using automation.mbtdistr.ru.Services.Wildberries.Models;
using System.Collections.Generic;
using automation.mbtdistr.ru.Services.YandexMarket;
using static automation.mbtdistr.ru.Services.YandexMarket.Models.DTOs;

namespace automation.mbtdistr.ru.Services
{
  /// <summary>
  /// Фоновая служба для периодической синхронизации параметров площадок (возвратов, остатков и т.д.)
  /// </summary>
  public class MarketSyncService : BackgroundService
  {
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<MarketSyncService> _logger;
    private readonly TimeSpan _interval;
    private readonly ITelegramBotClient _botClient;
    private const int _telegramMaxMessageLength = 4096;
    private readonly ApplicationDbContext _db;

    private readonly YMApiService _yMApiService;

    private bool debug = false;
    private bool log = false;

    public MarketSyncService(

        IServiceScopeFactory scopeFactory,
        ILogger<MarketSyncService> logger,
        ITelegramBotClient botClient,
        IConfiguration config)
    {

      _db = scopeFactory.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();
      _botClient = botClient;
      _scopeFactory = scopeFactory;
      _yMApiService = scopeFactory.CreateScope().ServiceProvider.GetRequiredService<YMApiService>();
      _logger = logger;
      var minutes = config.GetValue<int>("MarketSync:IntervalMinutes", 25);
      _interval = TimeSpan.FromMinutes(minutes);

      var admin = _db.Workers
        .Include(w => w.NotificationOptions)
        .FirstOrDefault(w => w.TelegramId == 1406950293.ToString());

      if (admin?.NotificationOptions?.IsReceiveNotification == true && admin.NotificationOptions.NotificationLevels.Contains(NotificationLevel.Debug))
        debug = true;

      if (admin?.NotificationOptions?.IsReceiveNotification == true && admin.NotificationOptions.NotificationLevels.Contains(NotificationLevel.Log))
        log = true;

      //  SyncAllAsync(CancellationToken.None);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
      if (log) Extensions.SendDebugMessage($"Синхронизация площадок запущена\n{DateTime.Now}")?.ConfigureAwait(false);

      //await SyncAllAsync(stoppingToken);

      using var timer = new PeriodicTimer(_interval);
      while (await timer.WaitForNextTickAsync(stoppingToken))
      {
        try
        {
          await SyncAllAsync(stoppingToken);
        }
        catch (Exception ex)
        {
          if (debug)
            await _botClient.SendMessage(
               chatId: 1406950293, // ID чата для отправки сообщений
               text: $"Ошибка при синхронизации площадок:\n{ex.Message}",
               cancellationToken: stoppingToken);
        }
      }

      if (log) Extensions.SendDebugMessage($"Синхронизация площадок завершена\n{DateTime.Now}")?.ConfigureAwait(false);
    }

    /// <summary>
    /// Делит текст на «умные» фрагменты и отправляет их по очереди.
    /// </summary>
    private async Task SendLongMessageAsync(long chatId, string text, CancellationToken cancellationToken)
    {
      if (string.IsNullOrEmpty(text))
        return;

      foreach (var chunk in SplitMessage(text))
      {
        await _botClient.SendMessage(
            chatId: chatId,
            text: chunk,
            cancellationToken: cancellationToken);
      }
    }

    /// <summary>
    /// Делит текст на фрагменты <= TelegramMaxMessageLength,
    /// стараясь рвать по двойным/одинарным переносам или границам предложений.
    /// </summary>
    private IEnumerable<string> SplitMessage(string text)
    {
      int pos = 0, length = text.Length;
      while (pos < length)
      {
        int maxLen = Math.Min(_telegramMaxMessageLength, length - pos);
        string window = text.Substring(pos, maxLen);

        int split;
        // 1) двойной перенос строки
        split = window.LastIndexOf("\n\n", StringComparison.Ordinal);
        if (split >= 0)
        {
          split += 2;
        }
        else
        {
          // 2) одинарный перенос
          split = window.LastIndexOf('\n');
          if (split >= 0)
            split += 1;
          else
          {
            // 3) граница предложения: .!? плюс пробел
            var matches = Regex.Matches(window, @"[\.\!\?]\s+");
            if (matches.Count > 0)
            {
              var m = matches[matches.Count - 1];
              split = m.Index + m.Length;
            }
            else
            {
              // 4) жёсткий лимит
              split = maxLen;
            }
          }
        }

        var part = text.Substring(pos, split).TrimEnd();
        yield return part;
        pos += split;
      }
    }


    private async Task SyncAllAsync(CancellationToken ct)
    {
      using var scope = _scopeFactory.CreateScope();

      var wbSvc = scope.ServiceProvider.GetRequiredService<WildberriesApiService>();
      var ozSvc = scope.ServiceProvider.GetRequiredService<OzonApiService>();
      var ymSvc = scope.ServiceProvider.GetRequiredService<YMApiService>();

      // подтягиваем все кабинеты вместе с настройками
      var cabinets = await _db.Cabinets
                             .Include(c => c.Settings)
                             .ThenInclude(s => s.ConnectionParameters)
                             .ToListAsync(ct);

      List<Models.Return> allReturns = new List<Models.Return>();

      foreach (var cab in cabinets)
      {
        try
        {
          if (cab.Marketplace.Equals("OZON", StringComparison.OrdinalIgnoreCase) || cab.Marketplace.Equals("OZ", StringComparison.OrdinalIgnoreCase) || cab.Marketplace.Equals("ОЗОН", StringComparison.OrdinalIgnoreCase) || cab.Marketplace.Equals("ОЗ", StringComparison.OrdinalIgnoreCase))
          {
            var filter = new Services.Ozon.Models.Filter();
            filter.LogisticReturnDate = new Services.Ozon.Models.DateRange
            {
              From = DateTime.UtcNow.AddDays(-40),
              To = DateTime.UtcNow
            };

            List<Ozon.Models.ReturnInfo> returns = new List<Ozon.Models.ReturnInfo>();
            long lastId = 0;
            do
            {
              var response = await ozSvc.GetReturnsListAsync(cab, filter, lastId: lastId);



              if (response == null)
              {
                _logger.LogWarning("Не удалось получить данные возвратов для кабинета {CabinetId}", cab.Id);
                break;
              }
              if (response.Returns != null && response.Returns.Count > 0)
              {
                returns.AddRange(response.Returns);
              }
              if (!response.HasNext)
              {
                break; // Если нет следующей страницы, выходим из цикла
              }
              lastId = response.Returns[^1].Id; // Получаем ID последнего возврата для следующего запроса
            } while (true);

            //string debugMessage = "API Ozon - возвраты";
            //await Extensions.SendDebugObject<List<ReturnInfo>>(returns, debugMessage);

            allReturns.AddRange(await ProcessOzonReturnsAsync(returns, cab, _db, ct));
          }

          else if (cab.Marketplace.Equals("WILDBERRIES", StringComparison.OrdinalIgnoreCase) || cab.Marketplace.Equals("WB", StringComparison.OrdinalIgnoreCase))
          {
            var response = await wbSvc.GetReturnsListAsync(cab) as Wildberries.Models.ReturnsListResponse;

            await Extensions.SendDebugObject<Wildberries.Models.ReturnsListResponse>(response);

            allReturns.AddRange(await ProcessWbReturnsAsync(response.Claims, cab, _db, ct));
          }


          else if (cab.Marketplace.Equals("МЕГАМАРКЕТ", StringComparison.OrdinalIgnoreCase) || cab.Marketplace.Equals("ММ", StringComparison.OrdinalIgnoreCase) || cab.Marketplace.Equals("MEGAMARKET", StringComparison.OrdinalIgnoreCase) || cab.Marketplace.Equals("MM", StringComparison.OrdinalIgnoreCase) || cab.Marketplace.Equals("МЕГА", StringComparison.OrdinalIgnoreCase) || cab.Marketplace.Equals("МЕГ", StringComparison.OrdinalIgnoreCase) || cab.Marketplace.Equals("СБЕР", StringComparison.OrdinalIgnoreCase) || cab.Marketplace.Equals("СБЕР", StringComparison.OrdinalIgnoreCase) || cab.Marketplace.Equals("SBER", StringComparison.OrdinalIgnoreCase))
          {

          }


          else if (cab.Marketplace.Equals("YANDEXMARKET", StringComparison.OrdinalIgnoreCase) || cab.Marketplace.Equals("YANDEX MARKET", StringComparison.OrdinalIgnoreCase) || cab.Marketplace.Equals("YANDEX", StringComparison.OrdinalIgnoreCase) || cab.Marketplace.Equals("ЯНДЕКС", StringComparison.OrdinalIgnoreCase) || cab.Marketplace.Equals("ЯМ", StringComparison.OrdinalIgnoreCase) || cab.Marketplace.Equals("YM", StringComparison.OrdinalIgnoreCase))
          {


            List<Services.YandexMarket.Models.DTOs.ReturnsListResponse> _ymReturns = new List<Services.YandexMarket.Models.DTOs.ReturnsListResponse>();

            //получаем обьект кабинета на вб
            cabinets = await _db.Cabinets
                 .Include(c => c.Settings)
                     .ThenInclude(s => s.ConnectionParameters).Where(c => c.Marketplace.ToUpper() == "YANDEXMARKET").ToListAsync();

            List<CampaignsResponse> campaigns = new List<CampaignsResponse>();

            foreach (var c in cabinets)
            {
              var _campaigns = await _yMApiService.GetCampaignsAsync(c);
              campaigns.Add(_campaigns);
              foreach (var camp in _campaigns.Campaigns)
              {
                var result = await _yMApiService.GetReturnsListAsync(c, camp);
                _ymReturns.Add(result);
              }
            }
          }

          else
          {
            throw new NotSupportedException($"Неизвестная площадка: {cab.Marketplace}");
          }
        }
        catch (Exception ex)
        {
          await Extensions.SendDebugObject<Exception>(ex, $"Ошибка при синхронизации кабинета\n{cab.Marketplace} / {cab.Name}");
        }



      }
      try
      {
        var changes = await _db.SaveChangesAsync(ct);
        if (log)
        {
          string text = string.Empty;
          if (changes > 0)
            text = $"Синхронизировано {changes} записей";
          else
            text = "Нет изменений для сохранения в БД";
        }
      }
      catch (Exception)
      {
        throw;
      }
    }

    private async Task<List<Models.Return>> ProcessOzonReturnsAsync(
        List<Services.Ozon.Models.ReturnInfo> returns,
        Cabinet cab,
        ApplicationDbContext db,
        CancellationToken ct)
    {

      try
      {
        List<Models.Return> returnList = new List<Models.Return>();
        // TODO: конкретно мапить response.Items → модели Return и upsert в db.Returns
        string message = string.Empty;
        //var admin = await db.Workers.FirstOrDefaultAsync(w => w.TelegramId == 1406950293.ToString(), ct);
        foreach (var x in returns)
        {
          // Проверяем, существует ли возврат с таким ID в базе данных
          var existingReturn = await db.Returns
            .Include(r => r.Info)
            .Include(r => r.Compensation)
            .Include(r => r.Cabinet)
            .ThenInclude(c => c.AssignedWorkers)
            .FirstOrDefaultAsync(r => r.Info.ReturnInfoId == x.Id && r.CabinetId == cab.Id, ct);
          if (existingReturn != null)
          {
            if (existingReturn.Info.Id == 0)
              existingReturn.Info = new ReturnMainInfo { ClaimId = existingReturn.Id.ToString() };

            var newStatus = Enum.TryParse(typeof(ReturnStatus), x.Visual?.Status?.SysName, out var status) ? (ReturnStatus)status : ReturnStatus.Unknown;
            var newStatusStr = GetEnumDisplayName(newStatus);
            var currentStatus = existingReturn.Info.ReturnStatus;
            var currentStatusStr = GetEnumDisplayName(currentStatus);
            var newChangedAt = x.Visual?.ChangeMoment;
            if (currentStatus != newStatus)
            {
              message = FormatReturnHtml(x, cab, false, currentStatus);
              //отправляем всем участникам кабинета
              foreach (var worker in cab.AssignedWorkers)
              {
                if (worker.NotificationOptions.IsReceiveNotification && worker.NotificationOptions.NotificationLevels.Contains(NotificationLevel.ReturnNotification))
                {
                  await _botClient.SendMessage(
                      chatId: worker.TelegramId,
                      text: message,
                      parseMode: ParseMode.Html,
                      cancellationToken: ct);
                }
              }
            }

            if (existingReturn.ChangedAt != newChangedAt)
            {
              message = FormatReturnHtml(x, cab, false);
              foreach (var worker in cab.AssignedWorkers)
              {
                if (worker.NotificationOptions.IsReceiveNotification && (worker.NotificationOptions.NotificationLevels.Contains(NotificationLevel.ReturnNotification) || worker.NotificationOptions.NotificationLevels.Contains(NotificationLevel.AllCabinetNotifications)))
                {
                  await _botClient.SendMessage(
                      chatId: worker.TelegramId,
                      text: message,
                      parseMode: ParseMode.Html,
                      cancellationToken: ct);
                }
              }
            }

            var newIsOpened = x.AdditionalInfo?.IsOpened ?? false;
            existingReturn.IsOpened = newIsOpened;
            existingReturn.IsSuperEconom = x.AdditionalInfo?.IsSuperEconom ?? false;
            existingReturn.Info.ReturnStatus = Enum.TryParse(typeof(ReturnStatus), x.Visual?.Status?.SysName, out status) ? (ReturnStatus)status : ReturnStatus.Unknown;
            existingReturn.Info.ReturnInfoId = x.Id;
            existingReturn.Info.ReturnReasonName = x.ReturnReasonName;
            existingReturn.ChangedAt = x.Visual?.ChangeMoment;
            existingReturn.Info.OrderId = x.OrderId;
            db.Returns.Update(existingReturn);
            returnList.Add(existingReturn);
          }
          else
          {
            try
            {
              Models.Return @return = new Models.Return();
              @return.CabinetId = cab.Id;
              @return.ChangedAt = x.Visual?.ChangeMoment;
              @return.Info.ReturnInfoId = x.Id;
              @return.IsOpened = x.AdditionalInfo?.IsOpened ?? false;
              @return.IsSuperEconom = x.AdditionalInfo?.IsSuperEconom ?? false;
              @return.Info.ReturnStatus = Enum.TryParse(typeof(ReturnStatus), x.Visual?.Status?.SysName, out var status) ? (ReturnStatus)status : ReturnStatus.Unknown;
              @return.Info.ReturnReasonName = x.ReturnReasonName;
              @return.Info.OrderId = x.OrderId;
              db.Returns.Add(@return);
              message = FormatReturnHtml(x, cab, true);
              foreach (var worker in cab.AssignedWorkers)
              {
                if (worker.NotificationOptions.IsReceiveNotification && (worker.NotificationOptions.NotificationLevels.Contains(NotificationLevel.ReturnNotification) || worker.NotificationOptions.NotificationLevels.Contains(NotificationLevel.AllCabinetNotifications)))
                {
                  await _botClient.SendMessage(
                      chatId: worker.TelegramId,
                      text: message,
                      parseMode: ParseMode.Html,
                      cancellationToken: ct);
                }
              }
              returnList.Add(@return);
            }
            catch (Exception)
            {
              throw;
            }
          }

          await db.SaveChangesAsync();
        }

        return returnList;
      }
      catch (Exception)
      {

        throw;
      }

    }


    private async Task<List<Models.Return>> ProcessWbReturnsAsync(
        List<Claim> claims,
        Cabinet cab,
        ApplicationDbContext db,
        CancellationToken ct)
    {
      List<Models.Return> returnsList = new List<Models.Return>();
      try
      {
        foreach (var claim in claims)
        {
          var existingReturn = db.Returns
            .Include(r => r.Info)
            .Include(r => r.Compensation)
            .Include(r => r.Cabinet)
            .ThenInclude(c => c.AssignedWorkers)
            .FirstOrDefault(r => r.Info.ToString() == claim.Id && r.CabinetId == cab.Id);
          if (existingReturn != null)
          {
            var oldChangedAt = existingReturn.ChangedAt;
            var newChangedAt = claim.DtUpdate;

            if (oldChangedAt != newChangedAt)
            {
              var message = $"<b>Изменения в личном кабинете {cab.Marketplace.ToUpper()} / {cab.Name}</b>\n\nОбновление возврата {claim.NmId}\n\n{newChangedAt:dd.MM.yyyy HH:mm:ss}";
              //отправляем всем участникам кабинета
              foreach (var worker in cab.AssignedWorkers)
              {
                if (worker.NotificationOptions.IsReceiveNotification && worker.NotificationOptions.NotificationLevels.Contains(NotificationLevel.ReturnNotification))
                {
                  _botClient.SendMessage(
                      chatId: worker.TelegramId,
                      text: message,
                      parseMode: ParseMode.Html,
                      cancellationToken: ct);
                }
              }
            }


            // Обновляем существующий возврат
            existingReturn.Info.ClaimId = claim.Id;
            existingReturn.ChangedAt = claim.DtUpdate;
            existingReturn.OrderedAt = claim.OrderDt;
            existingReturn.CreatedAt = claim.Dt;
            db.Returns.Update(existingReturn);
            returnsList.Add(existingReturn);
          }
          else
          {
            Models.Return @return = new Models.Return();
            @return.CabinetId = cab.Id;
            @return.Info.ClaimId = claim.Id;
            @return.ChangedAt = claim.DtUpdate;
            @return.CreatedAt = claim.Dt;
            @return.OrderedAt = claim.OrderDt;
            db.Returns.Add(@return);
            returnsList.Add(@return);
          }

          var changes = await db.SaveChangesAsync();
        }
        return returnsList;
      }

      catch (Exception)
      {
        throw;
      }
    }

    private async Task<List<Models.Return>> ProcessYMReturnsAsync()
    {
      var returnsList = new List<Models.Return>();

      return returnsList;
    }

    string FormatReturnHtml(ReturnInfo x, Cabinet cab, bool isNew, ReturnStatus? oldStatus = null)
    {
      var sb = new StringBuilder();
      sb.AppendLine(isNew
          ? $"<b>Новый возврат в {cab.Marketplace.ToUpper()} / {cab.Name}</b>"
          : $"<b>Обновление возврата в {cab.Marketplace.ToUpper()} / {cab.Name}</b>");
      sb.AppendLine("");
      sb.AppendLine($"<b>Возврат</b> {x.Id}");
      sb.AppendLine($"<b>Заказ</b> {x.OrderId}");
      if (oldStatus.HasValue)
      {
        sb.AppendLine($"<b>Старый статус:</b> {GetEnumDisplayName(oldStatus)}");
        var newStatus = GetEnumDisplayName(Enum.TryParse(typeof(ReturnStatus), x.Visual?.Status?.SysName, out var status) ? (ReturnStatus)status : ReturnStatus.Unknown);
        sb.AppendLine($"<b>Новый статус:</b> {newStatus}");
      }
      //sb.AppendLine($"<b>Статус возврата:</b> {GetEnumDisplayName(x.Visual.Status.SysName)}");
      sb.AppendLine($"<b>Причина возврата:</b> {x.ReturnReasonName}");
      sb.AppendLine("");
      sb.AppendLine($"<b>Дата изменения:</b> {x?.Visual?.ChangeMoment:dd.MM.yyyy HH:mm:ss}");



      //if ((admin?.NotificationOptions?.IsReceiveNotification ?? false) && (admin?.NotificationOptions?.NotificationLevels.Any(l => l == NotificationLevel.DeepDegugNotification) ?? false))
      //{
      //  var obj = new
      //  {
      //    x,
      //    cab,
      //    isNew,
      //    oldStatus
      //  };
      //  string caption = $"FormatReturnHtml return\n\n{cab.Marketplace.ToUpper()} / {cab.Name}\nВозврат{x.Id}\nЗаказ №{x.OrderNumber} (#{x.OrderId})";
      //  Extensions.SendDebugObject<dynamic>(obj, caption).ConfigureAwait(false);
      //}
      return sb.ToString();
    }

  }
}
