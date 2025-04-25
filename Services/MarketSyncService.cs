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
      var minutes = config.GetValue<int>("MarketSync:IntervalMinutes", 15);
      _interval = TimeSpan.FromMinutes(minutes);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
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
          await _botClient.SendMessage(
               chatId: 1406950293, // ID чата для отправки сообщений
               text: $"Ошибка при синхронизации площадок:\n{ex.Message}",
               cancellationToken: stoppingToken);
        }
      }

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

          else if (cab.Marketplace.Equals("YANDEX MARKET", StringComparison.OrdinalIgnoreCase) || cab.Marketplace.Equals("YANDEX", StringComparison.OrdinalIgnoreCase) || cab.Marketplace.Equals("ЯНДЕКС", StringComparison.OrdinalIgnoreCase) || cab.Marketplace.Equals("ЯМ", StringComparison.OrdinalIgnoreCase) || cab.Marketplace.Equals("YM", StringComparison.OrdinalIgnoreCase))
          {

          }

          else
          {
            throw new NotSupportedException($"Неизвестная площадка: {cab.Marketplace}");
          }
        }
        catch (Exception ex)
        {
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
          await _botClient.SendMessage(
               chatId: 1406950293, // ID чата для отправки сообщений
               text: $"Синхронизировано {changes} записей",
               cancellationToken: ct);
        }
        else
        {
          await _botClient.SendMessage(
               chatId: 1406950293, // ID чата для отправки сообщений
               text: "Нет изменений для сохранения в БД",
               cancellationToken: ct);
        }
      }
      catch (Exception ex)
      {
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
      foreach (var x in returns)
      {
        // Проверяем, существует ли возврат с таким ID в базе данных
        var existingReturn = await db.Returns
          .Include(r => r.Info)
          .Include(r => r.Compensation)
          .Include(r => r.Cabinet)
          .FirstOrDefaultAsync(r => r.Info.ReturnInfoId == x.Id && r.CabinetId == cab.Id, ct);
        if (existingReturn != null)
        {
          if (existingReturn.Info.Id == 0)
            existingReturn.Info = new ReturnMainInfo { ReturnId = existingReturn.Id };

          var newStatus = Enum.TryParse(typeof(ReturnStatus), x.Visual?.Status?.SysName, out var status) ? (ReturnStatus)status : ReturnStatus.Unknown;
          var newStatusStr = GetEnumDisplayName(newStatus);
          var currentStatus = existingReturn.Info.ReturnStatus;
          var currentStatusStr = GetEnumDisplayName(currentStatus);
          if (currentStatus != newStatus)
          {
            message = $"Изменения в личном кабинете {cab.Marketplace.ToUpper()} / {cab.Name}\nОбновлена информация по возврату:\n#{x.Id}\nЗаказ №{x.OrderId}\n\nПрошлый статус: {currentStatus}\nНовый статус: {newStatusStr}";

            _logger.LogInformation(message);
            await _botClient.SendMessage(
                chatId: 1406950293, // ID чата для отправки сообщений
                text: message,
                cancellationToken: ct);
          }

          var newChangedAt = x.Visual?.ChangeMoment;
          if (existingReturn.ChangedAt != newChangedAt)
          {
            message = $"Изменения в личном кабинете {cab.Marketplace.ToUpper()} / {cab.Name}\nОбновлена информация по возврату:\n#{x.Id}\nЗаказ №{x.OrderId}\n{newChangedAt}";
            await _botClient.SendMessage(
                chatId: 1406950293, // ID чата для отправки сообщений
                text: message,
                cancellationToken: ct);
          }

          var newIsOpened = x.AdditionalInfo?.IsOpened ?? false;
          if (existingReturn.IsOpened != newIsOpened)
          {
            var currentIsOpened = existingReturn.IsOpened ? "открыт" : "закрыт";
            var newIsOpenedStr = newIsOpened ? "открыт" : "закрыт";
            message = $"Изменения в личном кабинете {cab.Marketplace.ToUpper()} / {cab.Name}\nОбновлена информация по возврату:\n#{x.Id}\nЗаказ №{x.OrderId}\nСтатус возврата: {newIsOpenedStr}";

            await _botClient.SendMessage(
                chatId: 1406950293, // ID чата для отправки сообщений
                text: message,
                cancellationToken: ct);
          }

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

            message = $"Изменения в личном кабинете {cab.Marketplace.ToUpper()} / {cab.Name}\nСоздан новый возврат:\n#{x.Id}\nЗаказ №{x.OrderId}\nСтатус возврата: {GetEnumDisplayName(@return.Info.ReturnStatus)}\nПричина возврата: {x.ReturnReasonName}";
            await _botClient.SendMessage(
                chatId: 1406950293, // ID чата для отправки сообщений
                text: message,
                cancellationToken: ct);
          }
          catch (Exception ex)
          {
            await _botClient.SendMessage(
                  chatId: 1406950293, // ID чата для отправки сообщений
                  text: $"Ошибка при обработке возврата {x.Id} для кабинета {cab.Id}:\n{ex.Message}",
                  cancellationToken: ct);
            _logger.LogError(ex, "Ошибка при обработке возврата {ReturnId} для кабинета {CabinetId}", x.Id, cab.Id);
          }
        }
      }
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
