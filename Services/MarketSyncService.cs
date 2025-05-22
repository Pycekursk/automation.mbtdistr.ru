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

using Return = automation.mbtdistr.ru.Models.Return;
using ZXing;
using automation.mbtdistr.ru.Services.YandexMarket.Models;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using automation.mbtdistr.ru.Services.YandexMarket;
using Microsoft.Extensions.Options;

namespace automation.mbtdistr.ru.Services
{
  /// <summary>
  /// Фоновая служба для периодической синхронизации параметров площадок (возвратов, остатков и т.д.)
  /// </summary>
  public class MarketSyncService : BackgroundService
  {
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TimeSpan _interval;
    private readonly ITelegramBotClient _botClient;
    private const int _telegramMaxMessageLength = 4096;
    private readonly ApplicationDbContext _db;
    private readonly IOptions<AppSettings> _options;
    private string baseUrl;


    public delegate void ReturnStatusChangedEventHandler(ReturnStatusChangedEventArgs e);
    public delegate void SupplyStatusChangedEventHandler(SupplyStatusChangedEventArgs e);
    public static event ReturnStatusChangedEventHandler? ReturnStatusChanged;
    public static event SupplyStatusChangedEventHandler? SupplyStatusChanged;

    //класс для передачи данных события
    public class ReturnStatusChangedEventArgs : EventArgs
    {
      public Models.Return Return { get; set; }
      public string Message { get; set; }
      public int CabinetId { get; set; }

      public object? ApiDTO { get; set; } // DTO от API, если нужно

      public ReturnStatusChangedEventArgs(int cabinetId, Models.Return @return, string message, object? apiDTO = null)
      {
        CabinetId = cabinetId;
        Return = @return;
        Message = message;
      }
    }

    public class SupplyStatusChangedEventArgs : EventArgs
    {
      public YMSupplyRequest Supply { get; set; }
      public string Message { get; set; }
      public int CabinetId { get; set; }
      public SupplyStatusChangedEventArgs(int cabinetId, YMSupplyRequest supply, string message, object? apiDTO = null)
      {
        CabinetId = cabinetId;
        Supply = supply;
        Message = message;
      }
    }

    public MarketSyncService(IServiceScopeFactory scopeFactory, ITelegramBotClient botClient, IConfiguration config, IOptions<AppSettings> options)
    {
      _scopeFactory = scopeFactory;
      _db = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();
      _botClient = botClient;
      var minutes = config.GetValue<int>("MarketSync:IntervalMinutes", 25);
      _interval = TimeSpan.FromMinutes(minutes);

      _options = options;
      if (Program.Environment.IsDevelopment())
        baseUrl = _options.Value.DebugUrl;
      else
        baseUrl = _options.Value.ProductionUrl;

      MarketSyncService.ReturnStatusChanged += OnReturnStatusChanged;
      MarketSyncService.SupplyStatusChanged += OnSupplyStatusChanged;

      if (Program.Environment.IsDevelopment())
      {
        SyncAllAsync(CancellationToken.None);
      }
    }

    private async void OnReturnStatusChanged(ReturnStatusChangedEventArgs e)
    {
      //if (e.ApiDTO != null)
      //  await Extensions.SendDebugObject(e.ApiDTO, $"Обьект возврата: {e.Return.Id}");
      using ApplicationDbContext context = new ApplicationDbContext(new DbContextOptions<ApplicationDbContext>());
      var workers = context.Cabinets
        .Include(c => c.AssignedWorkers)
        .ThenInclude(w => w.NotificationOptions)
        .FirstOrDefault(c => c.Id == e.CabinetId)?.AssignedWorkers;
      if (workers == null)
        return;
      if (Program.Environment.IsDevelopment())
      {
        // await _botClient.SendMessage("1406950293", e.Message, ParseMode.Html);
        return;
      }
      foreach (var worker in workers)
      {
        if (worker.NotificationOptions.IsReceiveNotification)
          await _botClient.SendMessage(worker.TelegramId, e.Message, ParseMode.Html);
      }
    }

    private async void OnSupplyStatusChanged(SupplyStatusChangedEventArgs e)
    {
      using ApplicationDbContext context = new ApplicationDbContext(new DbContextOptions<ApplicationDbContext>());
      // Null-check для cab и связанных работников
      var workers = context.Cabinets.Include(c => c.AssignedWorkers).ThenInclude(w => w.NotificationOptions).FirstOrDefault(c => c.Id == e.CabinetId)?.AssignedWorkers;
      if (workers == null)
        return;

      if (Program.Environment.IsDevelopment())
      {
        await _botClient.SendMessage("1406950293", e.Message, ParseMode.Html);
        return;
      }
      foreach (var worker in workers)
      {
        if (worker.NotificationOptions.IsReceiveNotification)
          await _botClient.SendMessage(worker.TelegramId, e.Message, ParseMode.Html);
      }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
      using var timer = new PeriodicTimer(_interval);
      while (await timer.WaitForNextTickAsync(stoppingToken))
      {
        try
        {
          if (!Program.Environment.IsDevelopment())
            await SyncAllAsync(stoppingToken);
        }
        catch (Exception ex)
        {
          await Extensions.SendDebugMessage($"Ошибка при синхронизации площадок\n{ex.Message}\n{ex.InnerException?.Message}\n{ex.StackTrace}");
        }
      }
    }

    private async Task SyncAllAsync(CancellationToken ct)
    {
      using var scope = _scopeFactory.CreateScope();
      var wbSvc = scope.ServiceProvider.GetRequiredService<WildberriesApiService>();
      var ozSvc = scope.ServiceProvider.GetRequiredService<OzonApiService>();
      var ymSvc = scope.ServiceProvider.GetRequiredService<YMApiService>();

      List<Cabinet> cabinets = new List<Cabinet>();
      try
      {
        // получаем все кабинеты из базы данных
        cabinets = await _db.Cabinets
          .Include(c => c.Settings)
          .ThenInclude(s => s.ConnectionParameters)
          .ToListAsync(ct);
      }
      catch (Exception ex)
      {
        await Extensions.SendDebugMessage($"Ошибка при получении кабинетов из БД\n{ex.Message}\n{ex.InnerException?.Message}\n{ex.StackTrace}");
        return;
      }
      // подтягиваем все кабинеты вместе с настройками

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


            List<Return> _returns = new List<Return>();
            foreach (var ret in returns)
            {
              var dbChangeDate = _db.Returns.Where(r => r.ReturnId == ret.Id.ToString()).Select(r => r.ChangedAt).FirstOrDefault();
              if (dbChangeDate != null && dbChangeDate == ret.Visual?.ChangeMoment)
                continue;
              var @return = Return.Parse<Ozon.Models.ReturnInfo>(ret);
              @return.CabinetId = cab.Id;
              if (@return.Warehouse != null)
                @return.Warehouse.Service = cab.Marketplace;





              _returns.Add(@return);
            }
            if (_returns.Count > 0)
            {
              _returns = await AddOrUpdateReturnsAsync(_returns, _db);
              //await Extensions.SendDebugObject<List<Return>>(_returns, $"Возвраты Ozon для кабинета {cab.Name} ({cab.Marketplace})");
            }
            //allReturns.AddRange(await ProcessOzonReturnsAsync(returns, cab, _db, ct));
          }

          else if (cab.Marketplace.Equals("WILDBERRIES", StringComparison.OrdinalIgnoreCase) || cab.Marketplace.Equals("WB", StringComparison.OrdinalIgnoreCase))
          {
            var response = await wbSvc.GetReturnsListAsync(cab) as Wildberries.Models.ReturnsListResponse;
            if (response?.Claims.Count > 0)
            {
              List<Return> _returns = new List<Return>();
              foreach (var claim in response.Claims)
              {
                var dbChangeDate = _db.Returns.Where(r => r.ReturnId == claim.Id.ToString()).Select(r => r.ChangedAt).FirstOrDefault();
                if (dbChangeDate != null && dbChangeDate == claim.DtUpdate)
                  continue;
                var @return = Return.Parse<Wildberries.Models.Claim>(claim);
                @return.CabinetId = cab.Id;
                _returns.Add(@return);
              }
              if (_returns.Count > 0)
              {
                _returns = await AddOrUpdateReturnsAsync(_returns, _db);
                await Extensions.SendDebugObject<List<Return>>(_returns, $"Возвраты Wildberries для кабинета {cab.Name} ({cab.Marketplace})");
              }
            }
          }

          else if (cab.Marketplace.Equals("YANDEXMARKET", StringComparison.OrdinalIgnoreCase) || cab.Marketplace.Equals("YANDEX MARKET", StringComparison.OrdinalIgnoreCase) || cab.Marketplace.Equals("YANDEX", StringComparison.OrdinalIgnoreCase) || cab.Marketplace.Equals("ЯНДЕКС", StringComparison.OrdinalIgnoreCase) || cab.Marketplace.Equals("ЯМ", StringComparison.OrdinalIgnoreCase) || cab.Marketplace.Equals("YM", StringComparison.OrdinalIgnoreCase))
          {
            var _campaigns = await ymSvc.GetCampaignsAsync(cab);
            List<Return> returns = new List<Return>();
            foreach (var camp in _campaigns.Campaigns)
            {
              var returnResponse = await ymSvc.GetReturnsListAsync(cab, camp);
              if (returnResponse?.Result?.Items?.Count > 0)
              {
                foreach (var ret in returnResponse.Result.Items)
                {
                  var dbChangeDate = _db.Returns.Where(r => r.ReturnId == ret.Id.ToString()).Select(r => r.ChangedAt).FirstOrDefault();
                  if (dbChangeDate != null && dbChangeDate == ret.UpdateDate)
                    continue;

                  ret.Order = (await ymSvc.GetOrdersAsync(cab, camp, new long[] { ret.OrderId }))?.Items?.FirstOrDefault();
                  if (ret.Items?.Count > 0)
                  {
                    foreach (var item in ret.Items)
                    {
                      var decision = item?.Decisions?.FirstOrDefault();
                      if (decision != null && decision.Images?.Count > 0)
                      {
                        List<string> imagesUrl = new List<string>();
                        foreach (var img in decision.Images)
                        {
                          var fileName = $"{ret.OrderId}_{ret.Id}_{decision.ReturnItemId}_{img}.jpg";
                          var filePath = Path.Combine("wwwroot", "images", "returns", fileName);
                          var fileDir = Path.GetDirectoryName(filePath);
                          if (!Directory.Exists(fileDir))
                            Directory.CreateDirectory(fileDir);

                          if (!System.IO.File.Exists(filePath))
                          {
                            var image = await ymSvc.GetReturnImageAsync(cab, camp, ret.OrderId, ret.Id, decision.ReturnItemId, img);
                            var imageBytes = Convert.FromBase64String(image.Result.ImageData);
                            await System.IO.File.WriteAllBytesAsync(filePath, imageBytes);
                          }
                          var fileUrl = $"{baseUrl}/images/returns/{fileName}";
                          imagesUrl.Add(fileUrl);
                        }
                        decision.Images = imagesUrl;
                      }
                    }
                  }


                  if (ret.LogisticPickupPoint != null)
                  {
                    var warehouse = await ymSvc.GetWarehouseByIdAsync(cab, ret.LogisticPickupPoint.Id);
                    if (warehouse != null)
                      ret.FulfillmentWarehouse = warehouse;
                  }
                  var @return = Return.Parse<YMReturn>(ret);
                  @return.CabinetId = cab.Id;
                  if (@return.Warehouse != null)
                  {
                    @return.Warehouse.Service = cab.Marketplace;
                  }
                  if (@return.Scheme == SellScheme.FBO)
                  {
                    //@return.OrderUrl = $"https://partner.market.yandex.ru/business/{camp.Business.Id}/returns?campaignId={camp.Id}&returnId={ret.Id}&partnerId=179624982&orderId={ret.OrderId}";
                    //@return.Url = $"https://partner.market.yandex.ru/order/{ret.OrderId}?partnerId=179624982";
                  }
                  @return.Products?.ForEach(p => p.Url = $"https://partner.market.yandex.ru/supplier/{camp.Id}/assortment/offer-card?tld=ru&offerId={p.OfferId}");
                  returns.Add(@return);
                }
              }

              if (returns.Count > 0)
              {
                returns = await AddOrUpdateReturnsAsync(returns, _db);
                //await Extensions.SendDebugObject<List<Return>>(returns, $"Возвраты ЯндексМаркет для кабинета {cab.Name} ({cab.Marketplace})");
              }

              var supplies = await ymSvc.GetSupplyRequests(cab, camp);
              if (supplies?.Result?.Items?.Count > 0)
              {
                foreach (var supple in supplies.Result.Items)
                {
                  var suppleItems = await ymSvc.GetSupplyRequestItemsAsync(cab, camp, supple.ExternalId?.Id ?? 0);
                  supple.Items = suppleItems?.Result?.Items;
                  await ymSvc.AddOrUpdateSupplyRequestAsync(supple, _db);
                }
              }
            }
          }

          else throw new NotSupportedException($"Неизвестная площадка: {cab.Marketplace}");
        }
        catch (Exception ex)
        {
          await Extensions.SendDebugMessage($"Ошибка при синхронизации кабинета #{cab.Id}\n{cab.Name} ({cab.Marketplace})\n\n{ex.Message}\n{ex.StackTrace}\n\n{ex.InnerException?.Message}");
        }
      }
    }

    private async Task<List<Models.Return>> ProcessOzonReturnsAsync(
        List<Models.Return> returns,
        Cabinet cab,
        ApplicationDbContext db,
        CancellationToken ct)
    {
      //try
      //{
      //  List<Models.Return> returnList = new List<Models.Return>();
      //  // TODO: конкретно мапить response.Items → модели Return и upsert в db.Returns
      //  string message = string.Empty;
      //  //var admin = await db.Workers.FirstOrDefaultAsync(w => w.TelegramId == 1406950293.ToString(), ct);
      //  foreach (var x in returns)
      //  {
      //    // Проверяем, существует ли возврат с таким ID в базе данных
      //    var existingReturn = await db.Returns
      //      .Include(r => r.Compensation)
      //      .Include(r => r.Cabinet)
      //      .ThenInclude(c => c.AssignedWorkers)
      //      .FirstOrDefaultAsync(r => r.ReturnId == x.Id && r.CabinetId == cab.Id, ct);
      //    if (existingReturn != null)
      //    {
      //      if (existingReturn.Info.Id == 0)
      //        existingReturn.Info = new ReturnMainInfo { ClaimId = existingReturn.Id.ToString() };

      //      var newStatus = Enum.TryParse(typeof(ReturnStatus), x.Visual?.Status?.SysName, out var status) ? (ReturnStatus)status : ReturnStatus.Unknown;
      //      var newStatusStr = newStatus.GetDisplayName();
      //      var currentStatus = existingReturn.Info.ReturnStatus;
      //      var currentStatusStr = currentStatus.GetDisplayName();
      //      var newChangedAt = x.Visual?.ChangeMoment;
      //      var oldChangedAt = existingReturn.ChangedAt;

      //      existingReturn.Info.ReturnStatus = Enum.TryParse(typeof(ReturnStatus), x.Visual?.Status?.SysName, out status) ? (ReturnStatus)status : ReturnStatus.Unknown;
      //      existingReturn.Info.ReturnInfoId = x.Id;
      //      existingReturn.Info.ReturnReasonName = x.ReturnReasonName;
      //      existingReturn.ChangedAt = x.Visual?.ChangeMoment;
      //      existingReturn.Info.OrderId = x.OrderId;
      //      db.Returns.Update(existingReturn);
      //      returnList.Add(existingReturn);

      //      await db.SaveChangesAsync();

      //      if (currentStatus != newStatus || oldChangedAt != newChangedAt)
      //      {
      //        message = FormatReturnHtmlMessage(existingReturn, cab, false, currentStatus);
      //        ReturnStatusChanged?.Invoke(new ReturnStatusChangedEventArgs(cab.Id, existingReturn, message, x));
      //      }
      //    }
      //    else
      //    {
      //      try
      //      {
      //        Models.Return @return = new Models.Return();
      //        @return.CabinetId = cab.Id;
      //        @return.ChangedAt = x.Visual?.ChangeMoment;

      //        //if (x.Logistic.ReturnDate.HasValue)
      //        //  @return.OrderedAt = x.Logistic.ReturnDate.Value;
      //        // Updated line to handle potential null reference
      //        @return.CreatedAt = x.Logistic?.ReturnDate.GetValueOrDefault() ?? DateTime.MinValue;





      //        db.Returns.Add(@return);
      //        returnList.Add(@return);

      //        await db.SaveChangesAsync();

      //        message = FormatReturnHtmlMessage(@return, cab, true);
      //        ReturnStatusChanged?.Invoke(new ReturnStatusChangedEventArgs(cab.Id, @return, message, x));
      //      }
      //      catch (Exception ex)
      //      {
      //        await Extensions.SendDebugMessage($"Ошибка при обработке возврата Ozon\n{ex.Message}\n{ex.InnerException?.Message}\n{ex.StackTrace}");
      //      }
      //    }
      //  }
      //  return returnList;
      //}
      //catch (Exception ex)
      //{
      //  //ошибка при сохранении изменений в БД
      //  await Extensions.SendDebugMessage($"Ошибка при обработке возвратов Ozon\n{ex.Message}\n{ex.InnerException?.Message}\n{ex.StackTrace}");
      //  throw;
      //}

      return returns;
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
       //.Include(r => r.Info)
       .Include(r => r.Compensation)
       .Include(r => r.Cabinet)
           .ThenInclude(c => c.AssignedWorkers)
       .FirstOrDefault(r => r.ReturnId == claim.Id && r.CabinetId == cab.Id);

          if (existingReturn != null)
          {
            var oldChangedAt = existingReturn.ChangedAt;
            var newChangedAt = claim.DtUpdate;
            // Обновляем существующий возврат
            existingReturn.ReturnId = claim.Id;
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
            @return.ReturnId = claim.Id;
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


    /// <summary>
    /// Добавление или обновление возвратов в базе данных.
    /// </summary>
    /// <param name="returns"> Список возвратов для обработки.</param>
    /// <param name="db"> Контекст базы данных.</param>
    /// <returns> Список обработанных возвратов.</returns>
    public async Task<List<Models.Return>> AddOrUpdateReturnsAsync(List<Return> returns, ApplicationDbContext db)
    {
      var existing = _db.Returns
             .Include(r => r.Warehouse)
             .Include(r => r.Cabinet)
             .ThenInclude(c => c.AssignedWorkers)
             .ThenInclude(w => w.NotificationOptions)
             .Include(r => r.Products)
             .ThenInclude(p => p.Images)
             .ToList()
             .Where(r => !string.IsNullOrEmpty(r.ReturnId) && returns.Any(_r => $"{r.ReturnId}_{r.OrderId}" == $"{_r.ReturnId}_{_r.OrderId}"))
             .ToList();

      foreach (var ret in returns)
      {
        try
        {
          if (ret.Warehouse != null)
          {
            var key = $"{ret.Warehouse.ExternalId}_{ret.Warehouse.Service}";
            var warehouse = await db.Warehouses
              .FirstOrDefaultAsync(w => w.ExternalId + "_" + w.Service == key);
            if (warehouse == null)
            {
              // если у склада есть адрес и у адреса пустая строка TextAddress, то назначаем TextAddress значение суммы полей Address
              if (ret.Warehouse.Address != null && string.IsNullOrEmpty(ret.Warehouse.Address.FullAddress))
              {
                ret.Warehouse.Address.FullAddress = $"{ret.Warehouse.Address.Country}, {ret.Warehouse.Address}, {ret.Warehouse.Address.City}, {ret.Warehouse.Address.Street}, {ret.Warehouse.Address.House}, {ret.Warehouse.Address.Office}";
              }
              //проверяем если координаты пустые, то заполняем их
              if (ret?.Warehouse?.Address?.Latitude == 0 && ret.Warehouse.Address.Longitude == 0)
              {
                //получаем значение апи ключа YandexGeo из appsettings
                var apiKey = Program.Configuration.GetSection("YandexGeo:Key").Value;
                var geoService = new YandexGeocoderService(apiKey);
                var coordinates = await geoService.GetCoordinatesAsync(ret.Warehouse.Address.FullAddress);
                if (coordinates != null)
                {
                  ret.Warehouse.Address.Latitude = (decimal)coordinates.Value.lat;
                  ret.Warehouse.Address.Longitude = (decimal)coordinates.Value.lon;
                }
              }

              db.Warehouses.Add(ret.Warehouse);
              ret.Warehouse = warehouse;
              await db.SaveChangesAsync();
            }
            else
            {
              //проверяем если координаты пустые, то заполняем их
              if (ret?.Warehouse?.Address?.Latitude == 0 && ret.Warehouse.Address.Longitude == 0)
              {
                //получаем значение апи ключа YandexGeo из appsettings
                var apiKey = Program.Configuration.GetSection("YandexGeo:ApiKey").Value;
                var geoService = new YandexGeocoderService(apiKey);
                var coordinates = await geoService.GetCoordinatesAsync(ret.Warehouse.Address.FullAddress);
                if (coordinates != null)
                {
                  ret.Warehouse.Address.Latitude = (decimal)coordinates.Value.lat;
                  ret.Warehouse.Address.Longitude = (decimal)coordinates.Value.lon;
                }
              }
              ret.Warehouse = warehouse;
            }
          }

          var existingReturn = existing.FirstOrDefault(r => r.ReturnId == ret.ReturnId && r.OrderId == ret.OrderId);
          if (existingReturn != null)
          {
            var oldChangedAt = existingReturn.ChangedAt;
            var newChangedAt = ret.ChangedAt;

            // Обновляем существующий возврат
            existingReturn.ChangedAt = ret.ChangedAt;
            existingReturn.CreatedAt = ret.CreatedAt;
            existingReturn.OrderedAt = ret.OrderedAt;
            existingReturn.WarehouseId = ret.WarehouseId;

            //проверяем наличие продуктов в обьекте и если они есть свреям с базой
            if (ret.Products != null && ret.Products.Count > 0)
            {
              foreach (var product in ret.Products)
              {
                var existingProduct = db.ReturnProducts
                  .Include(p => p.Images)
                  .Include(p => p.Return)
                  .FirstOrDefault(p => p.Sku == product.Sku && p.ReturnId.ToString() == ret.ReturnId);
                if (existingProduct != null)
                {
                  existingProduct.Count = product.Count;
                  existingProduct.Images = product.Images;
                  db.ReturnProducts.Update(existingProduct);
                }
                else
                {
                  // сбрасываем PK, чтобы EF сгенерировал новый
                  //product.Id = 0;
                  //db.ReturnProducts.Add(product);
                }
              }
            }
            db.Returns.Update(existingReturn);
            await db.SaveChangesAsync();

            if (oldChangedAt != newChangedAt)
            {
              // Отправляем сообщение об обновлении возврата
              var message = FormatReturnHtmlMessage(existingReturn, db.Cabinets.FirstOrDefault(c => c.Id == existingReturn.CabinetId), false);
              ReturnStatusChanged?.Invoke(new ReturnStatusChangedEventArgs(existingReturn.CabinetId, existingReturn, message));
            }
          }
          else
          {
            // Добавляем новый возврат
            db.Returns.Add(ret);
            await db.SaveChangesAsync();

            var message = FormatReturnHtmlMessage(ret, db.Cabinets.FirstOrDefault(c => c.Id == ret.CabinetId), true);
            ReturnStatusChanged?.Invoke(new ReturnStatusChangedEventArgs(ret.CabinetId, ret, message));
          }

        }
        catch (Exception ex)
        {
          await Extensions.SendDebugMessage($"Ошибка при обработке возвратов ЯндексМаркет\n{ex.Message}");
        }
      }
      return returns;
    }

    public async Task ProcessYMSupplyRequestsAsync(
        List<Services.YandexMarket.Models.YMSupplyRequest> supplyRequests,
        Cabinet cab,
        ApplicationDbContext db,
        CancellationToken ct
      )
    {
      try
      {
        List<YMSupplyRequestLocation> locations = new List<YMSupplyRequestLocation>();

        //получаем все локации из базы данных
        var existingLocations = await db.YMLocations
          .Include(l => l.Address)
          .ToListAsync(ct);


        foreach (var supply in supplyRequests)
        {
          //проверяем есть ли такая локация в базе данных
          var existingLocation = existingLocations.FirstOrDefault(l => l.ServiceId == supply.TargetLocation?.ServiceId || l.ServiceId == supply?.TransitLocation?.ServiceId);
          if (existingLocation != null)
          {
            locations.Add(existingLocation);
          }

          var existingSupply = await db.YMSupplyRequests
            .Include(s => s.Cabinet)
            .ThenInclude(c => c.AssignedWorkers)
            .Include(c => c.TransitLocation)
            .ThenInclude(a => a.Address)
            .Include(s => s.TargetLocation)
            .ThenInclude(a => a.Address)
            .FirstOrDefaultAsync(s => s.Id == supply.ExternalId.Id && s.CabinetId == cab.Id, ct);
          if (existingSupply != null)
          {
            var oldChangedAt = existingSupply.UpdatedAt;
            var newChangedAt = supply.UpdatedAt;

            var oldStatus = existingSupply.Status;
            existingSupply.Status = supply.Status;
            existingSupply.UpdatedAt = supply.UpdatedAt;
            db.YMSupplyRequests.Update(existingSupply);

            if (oldChangedAt != newChangedAt)
            {
              var message = FormatSupplyHtmlMessage(supply, cab, false, oldStatus);
              SupplyStatusChanged?.Invoke(new SupplyStatusChangedEventArgs(cab.Id, existingSupply, message));
            }
          }
          else
          {
            YMSupplyRequest @supplyRequest = new YMSupplyRequest();
            @supplyRequest.CabinetId = cab.Id;
            @supplyRequest.ExternalId = supply.ExternalId;
            @supplyRequest.Status = supply.Status;
            @supplyRequest.UpdatedAt = supply.UpdatedAt;
            @supplyRequest.Type = supply.Type;
            @supplyRequest.Subtype = supply.Subtype;
            if (supply.TargetLocation != null)
              locations.Add(supply.TargetLocation);
            db.YMSupplyRequests.Add(@supplyRequest);

            var message = FormatSupplyHtmlMessage(@supplyRequest, cab, true);
            SupplyStatusChanged?.Invoke(new SupplyStatusChangedEventArgs(cab.Id, @supplyRequest, message));
          }
        }
      }
      catch (Exception ex)
      {
        await Extensions.SendDebugMessage($"Ошибка при обработке возвратов ЯндексМаркет\n{ex.Message}");
      }
    }

    public static string FormatReturnHtmlMessage(Return x, Cabinet cab, bool? isNew, ReturnStatus? oldStatus = null)
    {
      var sb = new StringBuilder();
      // Реализация аналогичная с FormatReturnHtmlContent за исключением того, что здесь вместо <br> используется StringBuilder.AppendLine(" ")
      if (isNew.HasValue && isNew.Value)
      {
        sb.AppendLine($"<b>Новый возврат в {cab.Marketplace.ToUpper()} / {cab.Name}</b>");
        sb.AppendLine(" ");
      }
      else if (isNew.HasValue && !isNew.Value)
      {
        sb.AppendLine($"<b>Обновление возврата в {cab.Marketplace.ToUpper()} / {cab.Name}</b>");
        sb.AppendLine(" ");
      }
      sb.AppendLine($"<b>Схема:</b> {x.Scheme}");
      sb.AppendLine($"<b>Тип возврата:</b> {x?.ReturnType?.GetDisplayName()}");

      sb.AppendLine($"<b>ID возврата:</b> {x.ReturnId}");
      sb.AppendLine($"<b>ID заказа:</b> {x.OrderId}");
      sb.AppendLine($"<b>Номер заказа:</b> {x.OrderNumber}");
      sb.AppendLine($"<b>Дата заказа:</b> {x.OrderedAt}");
      if (!string.IsNullOrEmpty(x.ReturnReason))
        sb.AppendLine($"<b>Причина возврата:</b> {x.ReturnReason}");
      if (!string.IsNullOrEmpty(x.ClientComment))
        sb.AppendLine($"<b>Комментарий:</b> {x.ClientComment}");
      sb.AppendLine(" ");
      sb.AppendLine(" ");
      if (x?.Products?.Count > 0)
      {
        sb.AppendLine("<b>Товары:</b>");
        int i = 1;
        foreach (var item in x.Products)
        {
          if (!string.IsNullOrEmpty(item.Url))
            sb.AppendLine($"<b>№ {i++}:</b><a href=\"{item.Url}\">{item.Name}</a>");
          else
            sb.AppendLine($"<b>№ {i++}:</b> {item.Name}");
          sb.AppendLine($"<b>SKU:</b> {item.Sku}");
          sb.AppendLine($"<b>Артикул:</b> {item.OfferId}");
          sb.AppendLine($"<b>Количество:</b> {item.Count}");
          if (item.Images != null && item.Images.Count > 0)
          {
            sb.AppendLine($"<b>Изображения:</b>");
            foreach (var img in item.Images)
            {
              sb.AppendLine($"<a href=\"{img.Url}\">{img.Url}</a>");
            }
          }
        }
        sb.AppendLine(" ");
        sb.AppendLine(" ");
      }

      sb.AppendLine($"<b>Создан:</b> {x.CreatedAt:dd.MM.yyyy HH:mm:ss}");
      sb.AppendLine($"<b>Обновлен:</b> {x.ChangedAt:dd.MM.yyyy HH:mm:ss}");
      sb.AppendLine(" ");
      if (x.Warehouse?.Address != null)
      {
        x.Warehouse.Address.FullAddress ??= $"{x.Warehouse.Address.Country}, {x.Warehouse.Address.City}, {x.Warehouse.Address.Street}, {x.Warehouse.Address.House}, {x.Warehouse.Address.Office}";

        sb.AppendLine($"<b>Склад:</b> {x.Warehouse.Name}");
        sb.AppendLine($"<b>Адрес:</b> {x.Warehouse.Address.FullAddress}");
      }
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
      sb.AppendLine($"<b>Схема:</b> {x.Scheme}");
      sb.AppendLine($"<b>ID возврата:</b> {x.ReturnId}");
      sb.AppendLine($"<b>ID заказа:</b> {x.OrderId}");
      sb.AppendLine($"<b>Номер заказа:</b> {x.OrderNumber}");
      sb.AppendLine($"<b>Дата заказа:</b> {x.OrderedAt}");

      if (!string.IsNullOrEmpty(x.ReturnReason))
        sb.AppendLine($"<b>Причина возврата:</b> {x.ReturnReason}");

      sb.AppendLine("<br>");
      sb.AppendLine("<br>");

      if (x?.Products?.Count > 0)
      {
        sb.AppendLine($"<b>Товары:</b>");
        int i = 1;
        foreach (var item in x.Products)
        {
          sb.AppendLine("<br>");
          sb.AppendLine($"<b>№ {i++}:</b>");
          sb.AppendLine($"<b>Наименование:</b> {item.Name}");
          sb.AppendLine($"<b>SKU:</b> {item.Sku}");
          sb.AppendLine($"<b>Артикул:</b> {item.OfferId}");
          sb.AppendLine($"<b>Количество:</b> {item.Count}");
        }
        sb.AppendLine("<br>");
        sb.AppendLine("<br>");
      }
      sb.AppendLine($"<b>Создан:</b> {x.CreatedAt:dd.MM.yyyy HH:mm:ss}");
      sb.AppendLine("<br>");
      sb.AppendLine($"<b>Обновлен:</b> {x.ChangedAt:dd.MM.yyyy HH:mm:ss}");
      sb.AppendLine("<br>");
      sb.AppendLine($"<b>Локация:</b> {x.Warehouse?.Name}");
      sb.AppendLine("<br>");
      if (x.Warehouse?.Address != null)
      {
        sb.AppendLine($"<b>Адрес:</b>{x.Warehouse.Address.Country}, {x.Warehouse.Address.City}, {x.Warehouse.Address.Street}, {x.Warehouse.Address.House}, {x.Warehouse.Address.Office}");
        sb.AppendLine("<br>");
      }
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

    public static string FormatSupplyHtmlMessage(
        YMSupplyRequest supply,
        Cabinet cab,
        bool? isNew,
        YMSupplyRequestStatusType? oldStatus = null)
    {
      var sb = new StringBuilder();
      if (isNew.HasValue && isNew.Value)
      {
        sb.AppendLine($"<b>НОВЫЙ ЗАПРОС НА ПОСТАВКУ</b>");
        sb.AppendLine($"{cab.Name} ({cab.Marketplace.ToUpper()})");
      }
      else if (isNew.HasValue && !isNew.Value)
      {
        sb.AppendLine($"<b>Обновление запроса на поставку</b>");
        sb.AppendLine($"{cab.Name} ({cab.Marketplace.ToUpper()})");
      }
      sb.AppendLine($"<b>ID Заявки:</b> {supply?.ExternalId?.Id}");
      sb.AppendLine($"<b>Тип:</b> {supply?.Type.GetDisplayName()}");
      sb.AppendLine($"<b>Подтип:</b> {supply?.Subtype.GetDisplayName()}");
      sb.AppendLine($"<b>Статус:</b> {supply?.Status.GetDisplayName()}");
      // При обновлении — показываем старый и новый статус
      if (oldStatus.HasValue && isNew.HasValue && !isNew.Value)
      {
        sb.AppendLine($"<b>Старый статус:</b> {oldStatus.Value.GetDisplayName()}");
        sb.AppendLine($"<b>Новый статус:</b> {supply.Status.GetDisplayName()}");
      }
      sb.AppendLine("");
      sb.AppendLine($"<b>Локация:</b> {supply.TargetLocation?.Name}");
      sb.AppendLine("");
      sb.AppendLine("");
      sb.AppendLine($"<b>Обновлен:</b> {supply.UpdatedAt:dd.MM.yyyy HH:mm:ss}");
      return sb.ToString();
    }

  }
}
