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
      _logger = logger;
      var minutes = config.GetValue<int>("MarketSync:IntervalMinutes", 25);
      _interval = TimeSpan.FromMinutes(minutes);

      var admin = _db.Workers
        .Include(w => w.NotificationOptions)
        .FirstOrDefault(w => w.TelegramId == 1406950293.ToString());

      if (admin?.NotificationOptions?.IsReceiveNotification == true && admin.NotificationOptions.NotificationLevels.Contains(NotificationLevel.DeepDegugNotification))
        debug = true;

      if (admin?.NotificationOptions?.IsReceiveNotification == true && admin.NotificationOptions.NotificationLevels.Contains(NotificationLevel.LogNotification))
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

      // подтягиваем все кабинеты вместе с настройками
      var cabinets = await _db.Cabinets
                             .Include(c => c.Settings)
                             .ThenInclude(s => s.ConnectionParameters)
                             .ToListAsync(ct);

      foreach (var cab in cabinets)
      {
        if (log) Extensions.SendDebugMessage($"Синхронизация кабинета {cab.Marketplace} / {cab.Name}")?.ConfigureAwait(false);
        try
        {
          if (cab.Marketplace.Equals("OZON", StringComparison.OrdinalIgnoreCase) || cab.Marketplace.Equals("OZ", StringComparison.OrdinalIgnoreCase) || cab.Marketplace.Equals("ОЗОН", StringComparison.OrdinalIgnoreCase) || cab.Marketplace.Equals("ОЗ", StringComparison.OrdinalIgnoreCase))
          {
            var filter = new Services.Ozon.Models.Filter();

            // Пример фильтрации по дате возврата
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


            await ProcessOzonReturnsAsync(returns, cab, _db, ct);
          }

          else if (cab.Marketplace.Equals("WILDBERRIES", StringComparison.OrdinalIgnoreCase) || cab.Marketplace.Equals("WB", StringComparison.OrdinalIgnoreCase))
          {
            var response = await wbSvc.GetReturnsListAsync(cab);
            Extensions.SendDebugObject<Wildberries.Models.ReturnsListResponse>(response)?.ConfigureAwait(false);
            await ProcessWbReturnsAsync(response.Claims, cab, _db, ct);
          }


          else if (cab.Marketplace.Equals("МЕГАМАРКЕТ", StringComparison.OrdinalIgnoreCase) || cab.Marketplace.Equals("ММ", StringComparison.OrdinalIgnoreCase) || cab.Marketplace.Equals("MEGAMARKET", StringComparison.OrdinalIgnoreCase) || cab.Marketplace.Equals("MM", StringComparison.OrdinalIgnoreCase) || cab.Marketplace.Equals("МЕГА", StringComparison.OrdinalIgnoreCase) || cab.Marketplace.Equals("МЕГ", StringComparison.OrdinalIgnoreCase) || cab.Marketplace.Equals("СБЕР", StringComparison.OrdinalIgnoreCase) || cab.Marketplace.Equals("СБЕР", StringComparison.OrdinalIgnoreCase) || cab.Marketplace.Equals("SBER", StringComparison.OrdinalIgnoreCase))
          {

          }


          else if (cab.Marketplace.Equals("YANDEXMARKET", StringComparison.OrdinalIgnoreCase) || cab.Marketplace.Equals("YANDEX MARKET", StringComparison.OrdinalIgnoreCase) || cab.Marketplace.Equals("YANDEX", StringComparison.OrdinalIgnoreCase) || cab.Marketplace.Equals("ЯНДЕКС", StringComparison.OrdinalIgnoreCase) || cab.Marketplace.Equals("ЯМ", StringComparison.OrdinalIgnoreCase) || cab.Marketplace.Equals("YM", StringComparison.OrdinalIgnoreCase))
          {

          }


          else
          {
            throw new NotSupportedException($"Неизвестная площадка: {cab.Marketplace}");
          }
        }
        catch (Exception ex)
        {
          if (log) Extensions.SendDebugMessage($"Ошибка при синхронизации кабинета {cab.Marketplace} / {cab.Name}\n{ex.Message}")?.ConfigureAwait(false);
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
          Extensions.SendDebugMessage(text)?.ConfigureAwait(false);
        }
      }
      catch (Exception ex)
      {
        if (log) Extensions.SendDebugMessage($"Ошибка при сохранении данных в БД\n{ex.Message}")?.ConfigureAwait(false);
      }
    }

    private async Task ProcessOzonReturnsAsync(
        List<Services.Ozon.Models.ReturnInfo> returns,
        Cabinet cab,
        ApplicationDbContext db,
        CancellationToken ct)
    {
      // TODO: конкретно мапить response.Items → модели Return и upsert в db.Returns
      string message = string.Empty;
      var admin = await db.Workers.FirstOrDefaultAsync(w => w.TelegramId == 1406950293.ToString(), ct);
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

            // если админ подписан на уведомления, отправляем ему
            if (admin != null && admin.NotificationOptions != null && admin.NotificationOptions.IsReceiveNotification && admin.NotificationOptions.NotificationLevels.Contains(NotificationLevel.ReturnNotification))
            {
              await Extensions.SendDebugObject<ReturnInfo>(x, $"Изменения в личном кабинете {cab.Marketplace.ToUpper()} / {cab.Name}").ConfigureAwait(false);
            }
          }

          if (existingReturn.ChangedAt != newChangedAt)
          {
            message = $"<b>Изменения в личном кабинете {cab.Marketplace.ToUpper()} / {cab.Name}</b>\n\nОбновление возврата {x.Id}\nЗаказ №{x.OrderNumber} ({x.OrderId})\n\n{newChangedAt:dd.MM.yyyy HH:mm:ss}";
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

          var newIsOpened = x.AdditionalInfo?.IsOpened ?? false;
          existingReturn.IsOpened = newIsOpened;
          existingReturn.IsSuperEconom = x.AdditionalInfo?.IsSuperEconom ?? false;
          existingReturn.Info.ReturnStatus = Enum.TryParse(typeof(ReturnStatus), x.Visual?.Status?.SysName, out status) ? (ReturnStatus)status : ReturnStatus.Unknown;
          existingReturn.Info.ReturnInfoId = x.Id;
          existingReturn.Info.ReturnReasonName = x.ReturnReasonName;
          existingReturn.ChangedAt = x.Visual?.ChangeMoment;
          existingReturn.Info.OrderId = x.OrderId;
          db.Returns.Update(existingReturn);
        }
        else
        {
          try
          {
            //проверяем подписан ли админ на уведомления о глубокой отладке
            if (admin != null && admin.NotificationOptions != null && admin.NotificationOptions.IsReceiveNotification && admin.NotificationOptions.NotificationLevels.Contains(NotificationLevel.DeepDegugNotification))
            {
              await Extensions.SendDebugObject<ReturnInfo>(x, $"Изменения в личном кабинете {cab.Marketplace.ToUpper()} / {cab.Name}");
            }


            Return @return = new Return();
            @return.CabinetId = cab.Id;
            @return.ChangedAt = x.Visual?.ChangeMoment;
            @return.Info.ReturnInfoId = x.Id;
            @return.IsOpened = x.AdditionalInfo?.IsOpened ?? false;
            @return.IsSuperEconom = x.AdditionalInfo?.IsSuperEconom ?? false;
            @return.Info.ReturnStatus = Enum.TryParse(typeof(ReturnStatus), x.Visual?.Status?.SysName, out var status) ? (ReturnStatus)status : ReturnStatus.Unknown;
            @return.Info.ReturnReasonName = x.ReturnReasonName;
            @return.Info.OrderId = x.OrderId;
            db.Returns.Add(@return);

            //message = $"Изменения в личном кабинете {cab.Marketplace.ToUpper()} / {cab.Name}\nНовый возврат:\n#{x.Id}\nЗаказ №{x.OrderId}\nСостояние: {(@return.IsOpened ? "открыт" : "закрыт")}\nСтатус: {GetEnumDisplayName(@return.Info.ReturnStatus)}\nПричина возврата: {x.ReturnReasonName}\nДата изменения: {x.Visual?.ChangeMoment}";

            message = FormatReturnHtml(x, cab, true);

            await _botClient.SendMessage(
                chatId: 1406950293, // ID чата для отправки сообщений
                text: message,
                parseMode: ParseMode.Html,
                cancellationToken: ct);
          }
          catch (Exception ex)
          {
            if (log) Extensions.SendDebugMessage($"Ошибка при обработке возврата {x.Id} для кабинета {cab.Marketplace.ToUpper()} / {cab.Name}\n{ex.Message}")?.ConfigureAwait(false);
            _logger.LogError(ex, "Ошибка при обработке возврата {ReturnId} для кабинета {CabinetId}", x.Id, cab.Id);
          }
        }
      }
    }



    string FormatReturnHtml(ReturnInfo x, Cabinet cab, bool isNew, ReturnStatus? oldStatus = null)
    {
      var sb = new StringBuilder();
      sb.AppendLine(isNew
          ? $"<b>Новый возврат в {cab.Marketplace.ToUpper()} / {cab.Name}</b>"
          : $"<b>Обновление возврата в {cab.Marketplace.ToUpper()} / {cab.Name}</b>");
      sb.AppendLine("");
      sb.AppendLine($"<b>Возврат</b>{x.Id}");
      sb.AppendLine($"<b>Заказ</b>{x.OrderId}");
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

    private Task ProcessWbReturnsAsync(
        List<Claim> claims,
        Cabinet cab,
        ApplicationDbContext db,
        CancellationToken ct)
    {

      try
      {
        foreach (var claim in claims)
        {
          // Проверяем, существует ли возврат с таким ID в базе данных
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
          }
          else
          {
            // Создаем новый возврат
            Return @return = new Return();
            @return.CabinetId = cab.Id;
            // @return.ChangedAt = claim.ChangedAt;
            @return.Info.ClaimId = claim.Id;
            @return.ChangedAt = claim.DtUpdate;
            @return.CreatedAt = claim.Dt;
            @return.OrderedAt = claim.OrderDt;
            db.Returns.Add(@return);
          }
        }
        var changes = db.SaveChanges();
        var admin = db.Workers.Include(w => w.NotificationOptions).FirstOrDefault(w => w.TelegramId == 1406950293.ToString());

        if (admin.NotificationOptions.NotificationLevels.Contains(NotificationLevel.LogNotification))
        {
          string message = string.Empty;
          if (changes > 0)
          {
            message = $"Синхронизировано {changes} возвратов для кабинета {cab.Marketplace.ToUpper()} / {cab.Name}";
          }
          else
          {
            message = $"Нет изменений для сохранения в БД для кабинета {cab.Marketplace.ToUpper()} / {cab.Name}";
          }
          _botClient.SendMessage(
              chatId: admin.TelegramId,
              text: message,
              parseMode: ParseMode.Html,
              cancellationToken: ct);
        }
        if (admin.NotificationOptions.NotificationLevels.Contains(NotificationLevel.DeepDegugNotification))
        {
          var obj = new
          {
            claims,
            cab,
            changes
          };
          Extensions.SendDebugObject<dynamic>(obj).ConfigureAwait(false);
        }
      }

      catch (Exception ex)
      {
        Extensions.SendDebugObject<Exception>(ex);
      }

      return Task.CompletedTask;
    }
  }


}
