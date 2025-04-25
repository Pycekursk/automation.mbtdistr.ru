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

    public MarketSyncService(
        IServiceScopeFactory scopeFactory,
        ILogger<MarketSyncService> logger,
        ITelegramBotClient botClient,
        IConfiguration config)
    {
      _botClient = botClient;
      _scopeFactory = scopeFactory;
      _logger = logger;
      var minutes = config.GetValue<int>("MarketSync:IntervalMinutes", 25);
      _interval = TimeSpan.FromMinutes(minutes);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
      var adminNotify = false;
      using var scope = _scopeFactory.CreateScope();
      var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
      var admin = await db.Workers.FirstOrDefaultAsync(w => w.TelegramId == 1406950293.ToString());
      if (admin != null && admin.NotificationOptions != null && admin.NotificationOptions.IsReceiveNotification)
        adminNotify = true;


      if (adminNotify)
        await _botClient.SendMessage(
             chatId: 1406950293, // ID чата для отправки сообщений
             text: $"Синхронизация площадок запущена, интервал = {_interval}",
             cancellationToken: stoppingToken);

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
          if (adminNotify)
            await _botClient.SendMessage(
               chatId: 1406950293, // ID чата для отправки сообщений
               text: $"Ошибка при синхронизации площадок:\n{ex.Message}",
               cancellationToken: stoppingToken);
        }
      }
      if (adminNotify)
        await _botClient.SendMessage(
           chatId: 1406950293, // ID чата для отправки сообщений
           text: "Синхронизация площадок остановлена",
           cancellationToken: stoppingToken);
    }

    private async Task SyncAllAsync(CancellationToken ct)
    {
      using var scope = _scopeFactory.CreateScope();

      var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
      var wbSvc = scope.ServiceProvider.GetRequiredService<WildberriesApiService>();
      var ozSvc = scope.ServiceProvider.GetRequiredService<OzonApiService>();


      var adminNotify = db.Workers
     .FirstOrDefault(w => w.TelegramId == "1406950293")?
     .NotificationOptions?.IsReceiveNotification ?? false;

      // подтягиваем все кабинеты вместе с настройками
      var cabinets = await db.Cabinets
                             .Include(c => c.Settings)
                             .ThenInclude(s => s.ConnectionParameters)
                             .ToListAsync(ct);

      foreach (var cab in cabinets)
      {
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
            await ProcessOzonReturnsAsync(returns, cab, db, ct);
          }

          else if (cab.Marketplace.Equals("WILDBERRIES", StringComparison.OrdinalIgnoreCase) || cab.Marketplace.Equals("WB", StringComparison.OrdinalIgnoreCase))
          {

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
          if (adminNotify)
            await _botClient.SendMessage(
                 chatId: 1406950293, // ID чата для отправки сообщений
                 text: $"Ошибка при синхронизации кабинета #{cab.Id}\n{cab.Marketplace} / {cab.Name}:\n{ex.Message}",
                 cancellationToken: ct);
        }
      }
      try
      {
        var changes = await db.SaveChangesAsync(ct);
        if (changes > 0)
        {
          if (adminNotify)
            await _botClient.SendMessage(
               chatId: 1406950293, // ID чата для отправки сообщений
               text: $"Синхронизировано {changes} записей",
               cancellationToken: ct);
        }
        else
        {
          if (adminNotify)
            await _botClient.SendMessage(
               chatId: 1406950293, // ID чата для отправки сообщений
               text: "Нет изменений для сохранения в БД",
               cancellationToken: ct);
        }
      }
      catch (Exception ex)
      {
        if (adminNotify)
          await _botClient.SendMessage(
             chatId: 1406950293, // ID чата для отправки сообщений
             text: $"Ошибка при сохранении данных в БД:\n{ex.Message}\n" + $"{ex.InnerException?.Message ?? string.Empty}",
             cancellationToken: ct);
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
            existingReturn.Info = new ReturnMainInfo { ReturnId = existingReturn.Id };

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
              if (worker.NotificationOptions != null && worker.NotificationOptions.IsReceiveNotification && worker.NotificationOptions.NotificationLevels.Contains(NotificationLevel.ReturnNotification))
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
              var inputJson = JsonSerializer.Serialize(new
              {
                existingReturn,
                x
              }, new JsonSerializerOptions
              {
                WriteIndented = true
              });
              message += $"\n\n<b>Дебаг:</b>\n<pre><code>{inputJson.EscapeHtml()}</code></pre>";
              await _botClient.SendMessage(
              chatId: admin.TelegramId, // ID чата для отправки сообщений
              parseMode: ParseMode.Html,
              text: message,
              cancellationToken: ct);
            }

            //message = $"Изменения в личном кабинете {cab.Marketplace.ToUpper()} / {cab.Name}\nОбновился статус возврата:\n#{x.Id}\nСостояние:{((x.AdditionalInfo?.IsOpened).GetValueOrDefault(false) ? "открыт" : "закрыт")}\nЗаказ №{x.OrderId}\n\nПрошлый статус: {currentStatusStr}\nНовый статус: {newStatusStr}\nИзменен: {newChangedAt}";
            // Отправляем сообщение пользователю, если он подписан на уведомления

          }

          if (existingReturn.ChangedAt != newChangedAt)
          {
            message = $"Изменения в личном кабинете {cab.Marketplace.ToUpper()} / {cab.Name}\nОбновлена информация по возврату:\n#{x.Id}\nЗаказ №{x.OrderId}\n{newChangedAt}";
            await _botClient.SendMessage(
                chatId: 1406950293, // ID чата для отправки сообщений
                text: message,
                cancellationToken: ct);
            if ((admin?.NotificationOptions?.IsReceiveNotification ?? false) && (admin?.NotificationOptions?.NotificationLevels.Any(l => l == NotificationLevel.LogNotification) ?? false))
            {
              message = $@"<b>Изменения в личном кабинете {cab.Marketplace.ToUpper()} / {cab.Name}</b>
                            <b>Обновлён возврат:</b> #{x.Id}
                            <b>Заказ №</b> {x.OrderId}
                            <b>Состояние:</b> {(x.AdditionalInfo?.IsOpened ?? false ? "открыт" : "закрыт")}";
              if (admin?.NotificationOptions?.NotificationLevels.Any(l => l == NotificationLevel.DeepDegugNotification) ?? false)
              {
                var inputJson = JsonSerializer.Serialize(new
                {
                  existingReturn,
                  x
                }, new JsonSerializerOptions
                {
                  WriteIndented = true
                });
                message += $"\n\n<b>Дебаг:</b>\n```json\n{inputJson}\n```";
              }
              await _botClient.SendMessage(
                  chatId: admin!.TelegramId, // ID чата для отправки сообщений
                  text: message,
                  parseMode: ParseMode.Html,
                  cancellationToken: ct);
            }
          }

          var newIsOpened = x.AdditionalInfo?.IsOpened ?? false;
          //if (existingReturn.IsOpened != newIsOpened)
          //{
          //  if ((admin?.NotificationOptions?.IsReceiveNotification ?? false) && (admin?.NotificationOptions?.NotificationLevels.Any(l => l == NotificationLevel.LogNotification) ?? false))
          //  {
          //    message = $@"<b>Изменения в личном кабинете {cab.Marketplace.ToUpper()} / {cab.Name}</b>
          //                  <b>Обновлён возврат:</b> #{x.Id}
          //                  <b>Заказ №</b> {x.OrderId}
          //                  <b>Состояние:</b> {(x.AdditionalInfo?.IsOpened ?? false ? "открыт" : "закрыт")}";
          //    if (admin?.NotificationOptions?.NotificationLevels.Any(l => l == NotificationLevel.DeepDegugNotification) ?? false)
          //    {
          //      var inputJson = JsonSerializer.Serialize(new
          //      {
          //        existingReturn,
          //        x
          //      }, new JsonSerializerOptions
          //      {
          //        WriteIndented = true
          //      });
          //      message += $"\n\n<b>Дебаг:</b>\n```json\n{inputJson}\n```";
          //    }
          //    await _botClient.SendMessage(
          //        chatId: admin!.TelegramId, // ID чата для отправки сообщений
          //        text: message,
          //        parseMode: ParseMode.Html,
          //        cancellationToken: ct);
          //  }
          //}

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
            if (admin != null && admin.NotificationOptions != null && admin.NotificationOptions.IsReceiveNotification)
              await _botClient.SendMessage(
                    chatId: 1406950293, // ID чата для отправки сообщений
                    text: $"Ошибка при обработке возврата {x.Id} для кабинета {cab.Id}:\n{ex.Message}",
                    cancellationToken: ct);
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
      sb.AppendLine($"<b>Возврат</b>#{x.Id}");
      sb.AppendLine($"<b>Заказ</b>{x.OrderId}");
      if (!isNew && oldStatus.HasValue)
      {
        sb.AppendLine($"<b>Старый статус:</b> {GetEnumDisplayName(oldStatus.Value)}");
        var newStatus = GetEnumDisplayName(Enum.TryParse(typeof(ReturnStatus), x.Visual?.Status?.SysName, out var status) ? (ReturnStatus)status : ReturnStatus.Unknown);
        sb.AppendLine($"<b>Новый статус:</b> {newStatus}");
      }
      //sb.AppendLine($"<b>Статус возврата:</b> {GetEnumDisplayName(x.Visual.Status.SysName)}");
      sb.AppendLine($"<b>Причина возврата:</b> {x.ReturnReasonName}");
      sb.AppendLine($"<b>Дата изменения:</b> {x?.Visual?.ChangeMoment:dd.MM.yyyy HH:mm:ss}");
      var db = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();
      var admin = db.Workers.FirstOrDefault(w => w.TelegramId == 1406950293.ToString());
      if ((admin?.NotificationOptions?.IsReceiveNotification ?? false) && (admin?.NotificationOptions?.NotificationLevels.Any(l => l == NotificationLevel.DeepDegugNotification) ?? false))
      {
        // Отправляем дебаг-вывод
        var inputJson = JsonSerializer.Serialize(new
        {
          x
        }, new JsonSerializerOptions
        {
          WriteIndented = true
        });
        sb.AppendLine($"<b>Дебаг:</b>\n```json\n{inputJson}\n```");
      }
      return sb.ToString();
    }

    private Task ProcessWbReturnsAsync(
        dynamic response,
        Cabinet cab,
        ApplicationDbContext db,
        CancellationToken ct)
    {
      // TODO: конкретная логика маппинга для WB
      _logger.LogInformation("Wildberries: получено {Count} возвратов", 0);
      return Task.CompletedTask;
    }
  }
}
