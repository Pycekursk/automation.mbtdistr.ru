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
using Return = automation.mbtdistr.ru.Models.Return;
using ZXing;
using automation.mbtdistr.ru.Services.YandexMarket.Models;

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

    //Добавляем делегат события изменения статуса возврата или создания нового возврата
    public delegate void ReturnStatusChangedEventHandler(ReturnStatusChangedEventArgs e);
    public static event ReturnStatusChangedEventHandler? ReturnStatusChanged;

    //класс для передачи данных события
    public class ReturnStatusChangedEventArgs : EventArgs
    {
      public Models.Return Return { get; set; }
      public string Message { get; set; }
      public int CabinetId { get; set; }
      public ReturnStatusChangedEventArgs(int cabinetId, Models.Return @return, string message)
      {
        CabinetId = cabinetId;
        Return = @return;
        Message = message;
      }
    }


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

      MarketSyncService.ReturnStatusChanged += OnReturnStatusChanged;
    }

    private async void OnReturnStatusChanged(ReturnStatusChangedEventArgs e)
    {
      // Null-check для cab и связанных работников
      var workers = _db.Cabinets.Include(c => c.AssignedWorkers).ThenInclude(w => w.NotificationOptions).FirstOrDefault(c => c.Id == e.CabinetId)?.AssignedWorkers;
      if (workers == null)
        return;
      // Отправляем сообщение всем рабочим
      foreach (var worker in workers)
      {
        if (worker.NotificationOptions.IsReceiveNotification)
          await _botClient.SendMessage(worker.TelegramId, e.Message, ParseMode.Html);
      }
      if (debug)
        await Extensions.SendDebugObject<Return>(e.Return, $"Обьект уведомления: \n{e.Message}");
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



    private async Task SyncAllAsync(CancellationToken ct)
    {
      if (Program.Environment.IsDevelopment()) return;

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
                //выбираем те возвраты у которых вижуал статус айди не равен 34
                returns.AddRange(response.Returns.Where(r => r.Visual?.Status.Id != 34));
                //returns.AddRange(response.Returns);
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
            if (response?.Claims.Count > 0)
            {
              await Extensions.SendDebugObject<Wildberries.Models.ReturnsListResponse>(response);
              var ret = await ProcessWbReturnsAsync(response.Claims, cab, _db, ct);
              allReturns.AddRange(ret);
            }
          }

          //else if (cab.Marketplace.Equals("МЕГАМАРКЕТ", StringComparison.OrdinalIgnoreCase) || cab.Marketplace.Equals("ММ", StringComparison.OrdinalIgnoreCase) || cab.Marketplace.Equals("MEGAMARKET", StringComparison.OrdinalIgnoreCase) || cab.Marketplace.Equals("MM", StringComparison.OrdinalIgnoreCase) || cab.Marketplace.Equals("МЕГА", StringComparison.OrdinalIgnoreCase) || cab.Marketplace.Equals("МЕГ", StringComparison.OrdinalIgnoreCase) || cab.Marketplace.Equals("СБЕР", StringComparison.OrdinalIgnoreCase) || cab.Marketplace.Equals("СБЕР", StringComparison.OrdinalIgnoreCase) || cab.Marketplace.Equals("SBER", StringComparison.OrdinalIgnoreCase))
          //{

          //}


          else if (cab.Marketplace.Equals("YANDEXMARKET", StringComparison.OrdinalIgnoreCase) || cab.Marketplace.Equals("YANDEX MARKET", StringComparison.OrdinalIgnoreCase) || cab.Marketplace.Equals("YANDEX", StringComparison.OrdinalIgnoreCase) || cab.Marketplace.Equals("ЯНДЕКС", StringComparison.OrdinalIgnoreCase) || cab.Marketplace.Equals("ЯМ", StringComparison.OrdinalIgnoreCase) || cab.Marketplace.Equals("YM", StringComparison.OrdinalIgnoreCase))
          {
            List<Services.YandexMarket.Models.ReturnsListResponse> _ymReturns = new List<Services.YandexMarket.Models.ReturnsListResponse>();
            var _campaigns = await _yMApiService.GetCampaignsAsync(cab);
            foreach (var camp in _campaigns.Campaigns)
            {



              var result = await _yMApiService.GetReturnsListAsync(cab, camp);



              if (result.Result.Returns.Count > 0)
              {
                _ymReturns.Add(result);
              }
            }
            if (_ymReturns.Count > 0)
            {
              await ProcessYMReturnsAsync(_ymReturns, cab, _db, ct);
            }
          }

          else
          {
            throw new NotSupportedException($"Неизвестная площадка: {cab.Marketplace}");
          }
        }
        catch (Exception ex)
        {
          await Extensions.SendDebugMessage($"Ошибка при синхронизации кабинета #{cab.Id}\n{cab.Name} ({cab.Marketplace})\n\n{ex.Message}\n{ex.StackTrace}\n\n{ex.InnerException?.Message}");
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
      catch (Exception ex)
      {
        await Extensions.SendDebugMessage($"Ошибка при сохранении изменений в БД\n{ex.Message}");
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
            var newStatusStr = newStatus.GetDisplayName();
            var currentStatus = existingReturn.Info.ReturnStatus;
            var currentStatusStr = currentStatus.GetDisplayName();
            var newChangedAt = x.Visual?.ChangeMoment;
            var oldChangedAt = existingReturn.ChangedAt;

            existingReturn.Info.ReturnStatus = Enum.TryParse(typeof(ReturnStatus), x.Visual?.Status?.SysName, out status) ? (ReturnStatus)status : ReturnStatus.Unknown;
            existingReturn.Info.ReturnInfoId = x.Id;
            existingReturn.Info.ReturnReasonName = x.ReturnReasonName;
            existingReturn.ChangedAt = x.Visual?.ChangeMoment;
            existingReturn.Info.OrderId = x.OrderId;
            db.Returns.Update(existingReturn);
            returnList.Add(existingReturn);

            await db.SaveChangesAsync();

            if (currentStatus != newStatus || oldChangedAt != newChangedAt)
            {
              message = FormatReturnHtmlMessage(existingReturn, cab, false, currentStatus);
              ReturnStatusChanged?.Invoke(new ReturnStatusChangedEventArgs(cab.Id, existingReturn, message));
            }
          }
          else
          {
            try
            {
              Models.Return @return = new Models.Return();
              @return.CabinetId = cab.Id;
              @return.ChangedAt = x.Visual?.ChangeMoment;
              @return.Info.ReturnInfoId = x.Id;
              //if (x.Logistic.ReturnDate.HasValue)
              //  @return.OrderedAt = x.Logistic.ReturnDate.Value;
              // Updated line to handle potential null reference
              @return.CreatedAt = x.Logistic?.ReturnDate.GetValueOrDefault() ?? DateTime.MinValue;




              @return.Info.ReturnStatus = Enum.TryParse(typeof(ReturnStatus), x.Visual?.Status?.SysName, out var status) ? (ReturnStatus)status : ReturnStatus.Unknown;
              @return.Info.ReturnReasonName = x.ReturnReasonName;
              @return.Info.OrderId = x.OrderId;
              db.Returns.Add(@return);
              returnList.Add(@return);

              await db.SaveChangesAsync();

              message = FormatReturnHtmlMessage(@return, cab, true);
              ReturnStatusChanged?.Invoke(new ReturnStatusChangedEventArgs(cab.Id, @return, message));
            }
            catch (Exception ex)
            {
              await Extensions.SendDebugMessage($"Ошибка при обработке возврата Ozon\n{ex.Message}\n{ex.InnerException?.Message}\n{ex.StackTrace}");
            }
          }
        }
        return returnList;
      }
      catch (Exception ex)
      {
        //ошибка при сохранении изменений в БД
        await Extensions.SendDebugMessage($"Ошибка при обработке возвратов Ozon\n{ex.Message}\n{ex.InnerException?.Message}\n{ex.StackTrace}");
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
       .FirstOrDefault(r => r.Info.ClaimId == claim.Id && r.CabinetId == cab.Id);

          if (existingReturn != null)
          {
            var oldChangedAt = existingReturn.ChangedAt;
            var newChangedAt = claim.DtUpdate;
            // Обновляем существующий возврат
            existingReturn.Info.ClaimId = claim.Id;
            existingReturn.ChangedAt = claim.DtUpdate;
            existingReturn.OrderedAt = claim.OrderDt;
            existingReturn.CreatedAt = claim.Dt;
            db.Returns.Update(existingReturn);
            returnsList.Add(existingReturn);

            await db.SaveChangesAsync();
            if (oldChangedAt != newChangedAt)
            {
              var message = $"<b>Изменения в личном кабинете {cab.Marketplace.ToUpper()} / {cab.Name}</b>\n\nОбновление возврата {claim.NmId}\n\n{newChangedAt:dd.MM.yyyy HH:mm:ss}";
              ReturnStatusChanged?.Invoke(new ReturnStatusChangedEventArgs(cab.Id, existingReturn, message));
            }
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

            await db.SaveChangesAsync();

            var message = $"<b>Новый возврат в личном кабинете {cab.Marketplace.ToUpper()} / {cab.Name}</b>\n\nВозврат {claim.NmId}\n\n{claim.DtUpdate:dd.MM.yyyy HH:mm:ss}";
            ReturnStatusChanged?.Invoke(new ReturnStatusChangedEventArgs(cab.Id, @return, message));
          }
        }
        return returnsList;
      }

      catch (Exception ex)
      {
        await Extensions.SendDebugMessage($"Ошибка при обработке возвратов Wildberries\n{ex.Message}\n{ex.InnerException?.Message}\n{ex.StackTrace}");
        throw;
      }
    }

    public async Task<List<Models.Return>> ProcessYMReturnsAsync(
        List<Services.YandexMarket.Models.ReturnsListResponse> returns,
        Cabinet cab,
        ApplicationDbContext db,
        CancellationToken ct
      )
    {
      var returnsList = new List<Models.Return>();

      try
      {

        foreach (var yaRetun in returns)
        {
          foreach (var r in yaRetun.Result.Returns)
          {
            var existingReturn = await db.Returns
              .Include(r => r.Info)
              .Include(r => r.Compensation)
              .Include(r => r.Cabinet)
              .ThenInclude(c => c.AssignedWorkers)
              .FirstOrDefaultAsync(_r => _r.CabinetId == cab.Id && _r.Info.ReturnInfoId == r.Id, ct);
            if (existingReturn != null)
            {
              var oldChangedAt = existingReturn.ChangedAt;
              var newChangedAt = r.UpdateDate;

              if (newChangedAt != oldChangedAt)
              {
                existingReturn.ChangedAt = newChangedAt;
                db.Returns.Update(existingReturn);
                returnsList.Add(existingReturn);
                var message = FormatReturnHtmlMessage(existingReturn, cab, false);
                ReturnStatusChanged?.Invoke(new ReturnStatusChangedEventArgs(cab.Id, existingReturn, message));
              }
            }
            else
            {
              Models.Return @return = new Models.Return();
              @return.CreatedAt = r.CreationDate;
              @return.ChangedAt = r.UpdateDate;
              @return.CabinetId = cab.Id;
              @return.Info.ReturnReasonName = $"{(string.IsNullOrEmpty(r.Comment) ? "" : $"{r.Comment}. ")}{r.DecisionReason.GetDisplayName()}. {r.DecisionSubreason.GetDisplayName()}";


              @return.Info.ProductsSku = r.Items.Select(i => i.MarketSku).ToList();
              @return.Info.ReturnInfoId = r.Id;
              @return.Info.OrderId = r.OrderId;

              //@return.Info.ReturnStatus = r.ShipmentStatus;

              db.Returns.Add(@return);
              returnsList.Add(@return);

              var message = FormatReturnHtmlMessage(@return, cab, true);
              ReturnStatusChanged?.Invoke(new ReturnStatusChangedEventArgs(cab.Id, @return, message));
            }
          }
        }

        await db.SaveChangesAsync();

      }
      catch (Exception ex)
      {
        await Extensions.SendDebugMessage($"Ошибка при обработке возвратов ЯндексМаркет\n{ex.Message}");
      }

      return returnsList;
    }

    public static string FormatReturnHtmlMessage(Return x, Cabinet cab, bool? isNew, ReturnStatus? oldStatus = null)
    {
      var sb = new StringBuilder();

      if (isNew.HasValue && isNew.Value)
      {
        sb.AppendLine($"<b>НОВЫЙ ВОЗВРАТ<b>");
        sb.AppendLine($"{cab.Name} ({cab.Marketplace.ToUpper()})");
      }
      else if (isNew.HasValue && !isNew.Value)
      {
        sb.AppendLine($"<b>Обновление возврата<b>");
        sb.AppendLine($"{cab.Name} ({cab.Marketplace.ToUpper()})");
      }

      sb.AppendLine($"<b>ID возврата:</b> {x.Info.ReturnInfoId}");
      sb.AppendLine($"<b>ID заказа:</b> {x.Info.OrderId}");
      sb.AppendLine("");
      sb.AppendLine($"<b>Причина возврата:</b> {x.Info.ReturnReasonName}");
      sb.AppendLine("");
      sb.AppendLine($"<b>Создан:</b> {x.CreatedAt:dd.MM.yyyy HH:mm:ss}");
      sb.AppendLine($"<b>Обновлен:</b> {x.ChangedAt:dd.MM.yyyy HH:mm:ss}");
      return sb.ToString();
    }

    public static string FormatReturnHtmlContent(Return x, Cabinet cab, bool? isNew, ReturnStatus? oldStatus = null)
    {
      var sb = new StringBuilder();

      if (isNew.HasValue && isNew.Value)
      {
        sb.AppendLine($"<b>Новый возврат в {cab.Marketplace.ToUpper()} / {cab.Name}</b>");
        sb.AppendLine("<br>");
      }
      else if (isNew.HasValue && !isNew.Value)
      {
        sb.AppendLine($"<b>Обновление возврата в {cab.Marketplace.ToUpper()} / {cab.Name}</b>");
        sb.AppendLine("<br>");
      }

      sb.AppendLine($"<b>ID возврата:</b> {x.Info.ReturnInfoId}");
      sb.AppendLine($"<b>ID заказа:</b> {x.Info.OrderId}");
      sb.AppendLine("<br>");
      sb.AppendLine("<br>");
      sb.AppendLine($"<b>Причина возврата:</b> {x.Info.ReturnReasonName}");
      sb.AppendLine("<br>");
      sb.AppendLine($"<b>Создан:</b> {x.CreatedAt:dd.MM.yyyy HH:mm:ss}");
      sb.AppendLine("<br>");
      sb.AppendLine($"<b>Обновлен:</b> {x.ChangedAt:dd.MM.yyyy HH:mm:ss}");
      return sb.ToString();
    }

    public static string FormatSupplyHtmlContent(
    YMSupplyRequest supply,
    Cabinet cab,
    bool? isNew,
    YMSupplyRequestStatusType? oldStatus = null)   // SupplyRequestStatus — ваш enum статусов заявки
    {
      var sb = new StringBuilder();

      if (isNew.HasValue && isNew.Value)
      {
        sb.AppendLine($"<b>Новый запрос на поставку в {cab.Marketplace.ToUpper()} / {cab.Name}</b>");
        sb.AppendLine("<br>");
      }
      else if (isNew.HasValue && !isNew.Value)
      {
        sb.AppendLine($"<b>Обновление запроса на поставку в {cab.Marketplace.ToUpper()} / {cab.Name}</b>");
        sb.AppendLine("<br>");
      }

      sb.AppendLine($"<b>ID Заявки:</b> {supply.ExternalId}");
      sb.AppendLine($"<b>Тип:</b> {supply.Type.GetDisplayName()}");
      sb.AppendLine($"<b>Подтип:</b> {supply.Subtype.GetDisplayName()}");
      sb.AppendLine($"<b>Статус:</b> {supply.Status.GetDisplayName()}");

      // При обновлении — показываем старый и новый статус
      if (oldStatus.HasValue && isNew.HasValue && !isNew.Value)
      {
        sb.AppendLine($"<b>Старый статус:</b> {oldStatus.Value.GetDisplayName()}");
        sb.AppendLine($"<b>Новый статус:</b> {supply.Status.GetDisplayName()}");
      }

      sb.AppendLine("<br>");
      sb.AppendLine($"<b>Локация:</b> {supply.TargetLocation?.Name}");
      sb.AppendLine("<br>");
      sb.AppendLine("<br>");
      sb.AppendLine($"<b>Обновлен:</b> {supply.UpdatedAt:dd.MM.yyyy HH:mm:ss}");

      return sb.ToString();
    }

  }
}
