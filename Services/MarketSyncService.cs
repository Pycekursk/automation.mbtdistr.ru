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
      var minutes = config.GetValue<int>("MarketSync:IntervalMinutes", 30);
      _interval = TimeSpan.FromMinutes(minutes);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
      _logger.LogInformation("MarketSyncService запущен, интервал = {Interval}", _interval);

      while (!stoppingToken.IsCancellationRequested)
      {
        try
        {
          await _botClient.SendMessage(
               chatId: 1406950293, // ID чата для отправки сообщений
               text: "Синхронизация площадок начата",
               cancellationToken: stoppingToken);

          await SyncAllAsync(stoppingToken);
        }
        catch (Exception ex)
        {
          await _botClient.SendMessage(
               chatId: 1406950293, // ID чата для отправки сообщений
               text: $"Ошибка при синхронизации площадок: {ex.Message}",
               cancellationToken: stoppingToken);
          _logger.LogError(ex, "Ошибка при синхронизации площадок");
        }

        await Task.Delay(_interval, stoppingToken);
      }

      _logger.LogInformation("MarketSyncService остановлен");
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
        _logger.LogInformation("Синхронизируем для кабинета {CabinetId} ({Marketplace})", cab.Id, cab.Marketplace);

        try
        {
          if (cab.Marketplace.ToLowerInvariant().Equals("ozon", StringComparison.OrdinalIgnoreCase) || cab.Marketplace.ToLowerInvariant().Equals("oz", StringComparison.OrdinalIgnoreCase))
          {
            // TODO: фильтры по типам данных (возвраты, остатки и т.д.)
            var filter = new Services.Ozon.Models.Filter();

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
                lastId = response.Returns[^1].Id; // Получаем ID последнего возврата для следующего запроса
              }
              if (!response.HasNext)
              {
                break; // Если нет следующей страницы, выходим из цикла
              }
            } while (true);
            await ProcessOzonReturnsAsync(returns, cab, db, ct);
          }
          else if (cab.Marketplace.ToLowerInvariant().Equals("wildberries", StringComparison.OrdinalIgnoreCase) || cab.Marketplace.ToLowerInvariant().Equals("wb", StringComparison.OrdinalIgnoreCase))
          {
            //await _botClient.SendMessage(
            //      chatId: 1406950293, // ID чата для отправки сообщений
            //      text: $"Синхронизация Wildberries для кабинета {cab.Name}",
            //      cancellationToken: ct);
            // var response = await wbSvc.GetReturnsListAsync(cab.Id);
            // await ProcessWbReturnsAsync(response, cab, db, ct);
          }
          else
          {
            //await _botClient.SendMessage(
            //     chatId: 1406950293, // ID чата для отправки сообщений
            //     text: $"Неизвестный Marketplace «{cab.Marketplace}» в кабинете {cab.Id}",
            //     cancellationToken: ct);
            //_logger.LogWarning("Неизвестный Marketplace «{Marketplace}» в кабинете {CabinetId}", cab.Marketplace, cab.Name);
          }
        }
        catch (Exception ex)
        {
          await _botClient.SendMessage(
               chatId: 1406950293, // ID чата для отправки сообщений
               text: $"Ошибка при синхронизации кабинета {cab.Id} ({cab.Marketplace}):\n{ex.Message}",
               cancellationToken: ct);

          _logger.LogError(ex, "Ошибка при синхронизации кабинета {CabinetId} ({Marketplace})", cab.Name, cab.Marketplace);
        }
      }
      try
      {
        var changes = await db.SaveChangesAsync(ct);
        if (changes > 0)
        {
          _logger.LogInformation("Изменения сохранены в БД: {Changes} записей", changes);
          await _botClient.SendMessage(
               chatId: 1406950293, // ID чата для отправки сообщений
               text: $"Изменения сохранены в БД: {changes} записей",
               cancellationToken: ct);
        }
        else
        {
          _logger.LogInformation("Нет изменений для сохранения в БД");
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
          if (existingReturn.Info.ReturnStatus != newStatus)
          {
            var currentStatus = existingReturn.Info.ReturnStatus.ToString();
            _logger.LogInformation($"Статус возврата {x.Id} изменился с {currentStatus} на {newStatus}");
            await _botClient.SendMessage(
                chatId: 1406950293, // ID чата для отправки сообщений
                text: $"Статус возврата {x.Id} изменился с {currentStatus} на {newStatus}",
                cancellationToken: ct);
          }
          var newChangedAt = x.Visual?.ChangeMoment;
          if (existingReturn.ChangedAt != newChangedAt)
          {
            var currentChangedAt = existingReturn.ChangedAt?.ToString("yyyy-MM-dd HH:mm:ss");
            _logger.LogInformation($"Дата изменения возврата {x.Id} изменилась с {currentChangedAt} на {newChangedAt}");
            await _botClient.SendMessage(
                chatId: 1406950293, // ID чата для отправки сообщений
                text: $"Дата изменения возврата {x.Id} изменилась с {currentChangedAt} на {newChangedAt}",
                cancellationToken: ct);
          }

          var newIsOpened = x.AdditionalInfo?.IsOpened ?? false;
          if (existingReturn.IsOpened != newIsOpened)
          {
            var currentIsOpened = existingReturn.IsOpened ? "открыт" : "закрыт";
            var newIsOpenedStr = newIsOpened ? "открыт" : "закрыт";
            _logger.LogInformation($"Статус возврата {x.Id} изменился с {currentIsOpened} на {newIsOpenedStr}");
            await _botClient.SendMessage(
                chatId: 1406950293, // ID чата для отправки сообщений
                text: $"Статус возврата {x.Id} изменился с {currentIsOpened} на {newIsOpenedStr}",
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
            @return.Info.ReturnStatus = Enum.TryParse(typeof(ReturnStatus), x.Visual?.Status?.SysName, out var status) ? (ReturnStatus)status : ReturnStatus.Unknown;
            @return.Info.ReturnReasonName = x.ReturnReasonName;
            @return.Info.OrderId = x.OrderId;
            db.Returns.Add(@return);
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
      _logger.LogInformation($"Ozon: получено {returns.Count} возвратов", returns?.Count ?? 0);
      _botClient.SendMessage(
           chatId: 1406950293, // ID чата для отправки сообщений
           text: $"Ozon: получено {returns?.Count} возвратов",
           cancellationToken: ct);
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
