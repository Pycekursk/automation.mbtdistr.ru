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
    private static OzonApiService? _ozSvc;
    private static WildberriesApiService? _wbSvc;
    private static YMApiService? _ymSvc;
    private const int _telegramMaxMessageLength = 4096;
    private readonly ApplicationDbContext _db;
    private readonly IOptions<AppSettings> _options;
    private static string? baseUrl;

    /// <summary>
    /// Делегат для события изменения статуса возврата.
    /// </summary>
    /// <param name="e"></param>
    public delegate void ReturnStatusChangedEventHandler(ReturnStatusChangedEventArgs e);

    /// <summary>
    /// Событие, вызываемое при изменении статуса возврата.
    /// </summary>
    /// <param name="e"></param>
    public delegate void SupplyStatusChangedEventHandler(SupplyStatusChangedEventArgs e);

    /// <summary>
    /// Событие, вызываемое при изменении статуса возврата.
    /// </summary>
    public static event ReturnStatusChangedEventHandler? ReturnStatusChanged;

    /// <summary>
    /// Событие, вызываемое при изменении статуса поставки.
    /// </summary>
    public static event SupplyStatusChangedEventHandler? SupplyStatusChanged;

    /// <summary>
    /// Аргументы события изменения статуса возврата.
    /// </summary>
    public class ReturnStatusChangedEventArgs : EventArgs
    {
      /// <summary>
      /// Возврат, для которого изменился статус.
      /// </summary>
      public Models.Return Return { get; set; }

      /// <summary>
      /// Сообщение для уведомления.
      /// </summary>
      public string Message { get; set; }

      /// <summary>
      /// Идентификатор кабинета, к которому относится возврат.
      /// </summary>
      public int CabinetId { get; set; }

      /// <summary>
      /// DTO-объект, полученный от API (опционально).
      /// </summary>
      public object? ApiDTO { get; set; }

      /// <summary>
      /// Конструктор аргументов события изменения статуса возврата.
      /// </summary>
      /// <param name="cabinetId">Идентификатор кабинета</param>
      /// <param name="return">Объект возврата</param>
      /// <param name="message">Сообщение</param>
      /// <param name="apiDTO">DTO-объект от API (опционально)</param>
      public ReturnStatusChangedEventArgs(int cabinetId, Models.Return @return, string message, object? apiDTO = null)
      {
        CabinetId = cabinetId;
        Return = @return;
        Message = message;
        ApiDTO = apiDTO;
      }
    }

    /// <summary>
    /// Аргументы события изменения статуса поставки.
    /// Используется для передачи информации о заявке на поставку, сообщении и идентификаторе кабинета.
    /// </summary>
    public class SupplyStatusChangedEventArgs : EventArgs
    {
      /// <summary>
      /// Заявка на поставку, для которой изменился статус.
      /// </summary>
      public YMSupplyRequest Supply { get; set; }

      /// <summary>
      /// Сообщение для уведомления.
      /// </summary>
      public string Message { get; set; }

      /// <summary>
      /// Идентификатор кабинета, к которому относится заявка.
      /// </summary>
      public int CabinetId { get; set; }

      /// <summary>
      /// Конструктор аргументов события изменения статуса поставки.
      /// </summary>
      /// <param name="cabinetId">Идентификатор кабинета</param>
      /// <param name="supply">Объект заявки на поставку</param>
      /// <param name="message">Сообщение</param>
      /// <param name="apiDTO">DTO-объект от API (опционально)</param>
      public SupplyStatusChangedEventArgs(int cabinetId, YMSupplyRequest supply, string message, object? apiDTO = null)
      {
        CabinetId = cabinetId;
        Supply = supply;
        Message = message;
      }
    }

    /// <summary>
    /// Конструктор сервиса синхронизации площадок.
    /// </summary>
    /// <param name="scopeFactory"></param>
    /// <param name="botClient"></param>
    /// <param name="config"></param>
    /// <param name="options"></param>
    public MarketSyncService(IServiceScopeFactory scopeFactory, ITelegramBotClient botClient, IConfiguration config, IOptions<AppSettings> options)
    {
      _scopeFactory = scopeFactory;
      var scope = _scopeFactory.CreateScope();
      _db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
      _ozSvc = scope.ServiceProvider.GetRequiredService<OzonApiService>();
      _wbSvc = scope.ServiceProvider.GetRequiredService<WildberriesApiService>();
      _ymSvc = scope.ServiceProvider.GetRequiredService<YMApiService>();
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

    /// <summary>
    /// Периодическая синхронизация площадок.
    /// </summary>
    /// <param name="stoppingToken"></param>
    /// <returns></returns>
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

    #region === публичный метод, вызываемый из HostedService ===


    private async Task SyncAllAsync(CancellationToken ct)
    {
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

      // List<Models.Return> allReturns = new List<Models.Return>();

      //SyncYandexMarketSupplies
      await Task.WhenAll(
          cabinets
              .Where(c => c.Marketplace.ToUpper() == "YANDEXMARKET")
              .Select(c => SyncYandexMarketSupplies(c))
      );

      await Task.WhenAll(
          cabinets
              .Where(c => c.Marketplace.ToUpper() == "OZON")
              .Select(c => SyncOzonSupplies(c))
      );


      // OZON products sync (без возвратов)
      await Task.WhenAll(
          cabinets
              .Where(c => c.Marketplace.ToUpper() == "OZON")
              .Select(c => SyncOzonProducts(c))
      );

      // OZON returns
      var ozonReturns = await Task.WhenAll(
          cabinets
              .Where(c => c.Marketplace.ToUpper() == "OZON")
              .Select(async c => await SyncOzonReturns(c))
      );

      await AddOrUpdateReturnsAsync(ozonReturns.SelectMany(r => r).ToList());

      // allReturns.AddRange(ozonReturns.SelectMany(r => r));

      // WILDBERRIES returns
      var wbReturns = await Task.WhenAll(
          cabinets
              .Where(c => c.Marketplace.ToUpper() == "WILDBERRIES")
              .Select(async c => await SyncWildberriesReturns(c))
      );
      await AddOrUpdateReturnsAsync(wbReturns.SelectMany(r => r).ToList());
      //  allReturns.AddRange(wbReturns.SelectMany(r => r));

      // YANDEXMARKET returns (с проверкой двух вариантов написания)
      var ymReturns = await Task.WhenAll(
          cabinets
              .Where(c =>
                  c.Marketplace.ToUpper() == "YANDEXMARKET")
              .Select(async c => await SyncYandexMarketReturns(c))
      );
      await AddOrUpdateReturnsAsync(ymReturns.SelectMany(r => r).ToList());
      //allReturns.AddRange(ymReturns.SelectMany(r => r));
      #region old block
      //foreach (var cab in cabinets)
      //{
      //  try
      //  {
      //    //if (cab.Marketplace.Equals("OZON", StringComparison.OrdinalIgnoreCase) || cab.Marketplace.Equals("OZ", StringComparison.OrdinalIgnoreCase) || cab.Marketplace.Equals("ОЗОН", StringComparison.OrdinalIgnoreCase) || cab.Marketplace.Equals("ОЗ", StringComparison.OrdinalIgnoreCase))
      //    //{

      //    //}

      //    //if (cab.Marketplace.Equals("WILDBERRIES", StringComparison.OrdinalIgnoreCase) || cab.Marketplace.Equals("WB", StringComparison.OrdinalIgnoreCase))
      //    //{
      //    //  var response = await wbSvc.GetReturnsListAsync(cab) as Wildberries.Models.ReturnsListResponse;
      //    //  if (response?.Claims.Count > 0)
      //    //  {
      //    //    List<Return> _returns = new List<Return>();
      //    //    foreach (var claim in response.Claims)
      //    //    {
      //    //      var dbChangeDate = _db.Returns.Where(r => r.ReturnId == claim.Id.ToString()).Select(r => r.ChangedAt).FirstOrDefault();
      //    //      if (dbChangeDate != null && dbChangeDate == claim.DtUpdate)
      //    //        continue;
      //    //      var @return = Return.Parse<Wildberries.Models.Claim>(claim);
      //    //      @return.CabinetId = cab.Id;
      //    //      _returns.Add(@return);
      //    //    }
      //    //    if (_returns.Count > 0)
      //    //    {
      //    //      _returns = await AddOrUpdateReturnsAsync(_returns, _db);
      //    //      await Extensions.SendDebugObject<List<Return>>(_returns, $"Возвраты Wildberries для кабинета {cab.Name} ({cab.Marketplace})");
      //    //    }
      //    //  }
      //    //}

      //    //else if (cab.Marketplace.Equals("YANDEXMARKET", StringComparison.OrdinalIgnoreCase) || cab.Marketplace.Equals("YANDEX MARKET", StringComparison.OrdinalIgnoreCase) || cab.Marketplace.Equals("YANDEX", StringComparison.OrdinalIgnoreCase) || cab.Marketplace.Equals("ЯНДЕКС", StringComparison.OrdinalIgnoreCase) || cab.Marketplace.Equals("ЯМ", StringComparison.OrdinalIgnoreCase) || cab.Marketplace.Equals("YM", StringComparison.OrdinalIgnoreCase))
      //    //{
      //    //  var _campaigns = await ymSvc.GetCampaignsAsync(cab);
      //    //  List<Return> returns = new List<Return>();
      //    //  foreach (var camp in _campaigns.Campaigns)
      //    //  {
      //    //    var returnResponse = await ymSvc.GetReturnsListAsync(cab, camp);
      //    //    if (returnResponse?.Result?.Items?.Count > 0)
      //    //    {
      //    //      foreach (var ret in returnResponse.Result.Items)
      //    //      {
      //    //        var dbChangeDate = _db.Returns.Where(r => r.ReturnId == ret.Id.ToString()).Select(r => r.ChangedAt).FirstOrDefault();
      //    //        if (dbChangeDate != null && dbChangeDate == ret.UpdateDate)
      //    //          continue;

      //    //        ret.Order = (await ymSvc.GetOrdersAsync(cab, camp, new long[] { ret.OrderId }))?.Items?.FirstOrDefault();
      //    //        if (ret.Items?.Count > 0)
      //    //        {
      //    //          foreach (var item in ret.Items)
      //    //          {
      //    //            var decision = item?.Decisions?.FirstOrDefault();
      //    //            if (decision != null && decision.Images?.Count > 0)
      //    //            {
      //    //              List<string> imagesUrl = new List<string>();
      //    //              foreach (var img in decision.Images)
      //    //              {
      //    //                var fileName = $"{ret.OrderId}_{ret.Id}_{decision.ReturnItemId}_{img}.jpg";
      //    //                var filePath = Path.Combine("wwwroot", "images", "returns", fileName);
      //    //                var fileDir = Path.GetDirectoryName(filePath);
      //    //                if (!Directory.Exists(fileDir))
      //    //                  Directory.CreateDirectory(fileDir);

      //    //                if (!System.IO.File.Exists(filePath))
      //    //                {
      //    //                  var image = await ymSvc.GetReturnImageAsync(cab, camp, ret.OrderId, ret.Id, decision.ReturnItemId, img);
      //    //                  var imageBytes = Convert.FromBase64String(image.Result.ImageData);
      //    //                  await System.IO.File.WriteAllBytesAsync(filePath, imageBytes);
      //    //                }
      //    //                var fileUrl = $"{baseUrl}/images/returns/{fileName}";
      //    //                imagesUrl.Add(fileUrl);
      //    //              }
      //    //              decision.Images = imagesUrl;
      //    //            }
      //    //          }
      //    //        }


      //    //        if (ret.LogisticPickupPoint != null)
      //    //        {
      //    //          var warehouse = await ymSvc.GetWarehouseByIdAsync(cab, ret.LogisticPickupPoint.Id);
      //    //          if (warehouse != null)
      //    //            ret.FulfillmentWarehouse = warehouse;
      //    //        }
      //    //        var @return = Return.Parse<YMReturn>(ret);
      //    //        @return.CabinetId = cab.Id;
      //    //        if (@return.TargetWarehouse != null)
      //    //        {
      //    //          @return.TargetWarehouse.Service = cab.Marketplace;
      //    //        }
      //    //        if (@return.Scheme == SellScheme.FBO)
      //    //        {
      //    //          //@return.OrderUrl = $"https://partner.market.yandex.ru/business/{camp.Business.Id}/returns?campaignId={camp.Id}&returnId={ret.Id}&partnerId=179624982&orderId={ret.OrderId}";
      //    //          //@return.Url = $"https://partner.market.yandex.ru/order/{ret.OrderId}?partnerId=179624982";
      //    //        }
      //    //        @return.Products?.ForEach(p => p.Url = $"https://partner.market.yandex.ru/supplier/{camp.Id}/assortment/offer-card?tld=ru&offerId={p.OfferId}");
      //    //        returns.Add(@return);
      //    //      }
      //    //    }

      //    //    if (returns.Count > 0)
      //    //    {
      //    //      returns = await AddOrUpdateReturnsAsync(returns, _db);
      //    //      //await Extensions.SendDebugObject<List<Return>>(returns, $"Возвраты ЯндексМаркет для кабинета {cab.Name} ({cab.Marketplace})");
      //    //    }

      //    //    var supplies = await ymSvc.GetSupplyRequests(cab, camp);
      //    //    if (supplies?.Result?.Items?.Count > 0)
      //    //    {
      //    //      foreach (var supple in supplies.Result.Items)
      //    //      {
      //    //        var suppleItems = await ymSvc.GetSupplyRequestItemsAsync(cab, camp, supple.ExternalId?.Id ?? 0);
      //    //        supple.Items = suppleItems?.Result?.Items;
      //    //        await ymSvc.AddOrUpdateSupplyRequestAsync(supple, _db);
      //    //      }
      //    //    }
      //    //  }
      //    //}

      //    // else throw new NotSupportedException($"Неизвестная площадка: {cab.Marketplace}");
      //  }
      //  catch (Exception ex)
      //  {
      //    await Extensions.SendDebugMessage($"Ошибка при синхронизации кабинета #{cab.Id}\n{cab.Name} ({cab.Marketplace})\n\n{ex.Message}\n{ex.StackTrace}\n\n{ex.InnerException?.Message}");
      //  }
      //}
      #endregion
    }
    #endregion

    private async Task<List<Return>> SyncWildberriesReturns(Cabinet cabinet)
    {
      List<Return> _returns = new List<Return>();
      using ApplicationDbContext db = new ApplicationDbContext(new DbContextOptions<ApplicationDbContext>());
      var response = await _wbSvc!.GetReturnsListAsync(cabinet) as Wildberries.Models.ReturnsListResponse;
      if (response?.Claims.Count > 0)
      {
        foreach (var claim in response.Claims)
        {
          var dbChangeDate = db.Returns.Where(r => r.ReturnId == claim.Id.ToString()).Select(r => r.ChangedAt).FirstOrDefault();
          if (dbChangeDate != null && dbChangeDate == claim.DtUpdate)
            continue;
          var @return = Return.Parse<Wildberries.Models.Claim>(claim);
          @return.CabinetId = cabinet.Id;
          _returns.Add(@return);
        }
      }
      return _returns;
    }

    /// <summary>
    /// Синхронизирует поставки YandexMarket и обновляет их в базе данных.
    /// </summary>
    /// <param name="cabinet"></param>
    /// <returns></returns>
    private async Task SyncYandexMarketSupplies(Cabinet cabinet)
    {
      var _campaigns = await _ymSvc.GetCampaignsAsync(cabinet);
      foreach (var camp in _campaigns.Campaigns)
      {
        if (camp.PlacementType == "FBS")
          continue; // Пропускаем FBS кампании, т.к. поставки для них не актуальны
        var supplies = await _ymSvc.GetSupplyRequests(cabinet, camp);
        if (supplies?.Result?.Items?.Count > 0)
        {
          foreach (var supple in supplies.Result.Items)
          {
            try
            {
              var suppleItems = await _ymSvc.GetSupplyRequestItemsAsync(cabinet, camp, supple.ExternalId?.Id ?? 0);
              supple.Items = suppleItems?.Result?.Items;
              supple.CabinetId = cabinet.Id;
              var dbsupple = await _ymSvc.AddOrUpdateSupplyRequestAsync(supple);
            }
            catch (Exception exc)
            {
              var message = $"Ошибка при синхронизации поставки {supple.Id} для кабинета {cabinet.Name} ({cabinet.Marketplace})\n{exc.Message}\n{exc.InnerException?.Message}\n{exc.StackTrace}";
              await Extensions.SendDebugMessage(message);
            }

          }
        }
      }
    }

    /// <summary>
    /// Парсит возвраты YandexMarket и возвращает список унифицированных возвратов.
    /// </summary>
    /// <param name="cabinet"></param>
    /// <returns></returns>
    private async Task<List<Return>> SyncYandexMarketReturns(Cabinet cabinet)
    {
      var _campaigns = await _ymSvc.GetCampaignsAsync(cabinet);
      List<Return> returns = new List<Return>();
      using ApplicationDbContext db = new ApplicationDbContext(new DbContextOptions<ApplicationDbContext>());
      foreach (var camp in _campaigns.Campaigns)
      {
        var returnResponse = await _ymSvc.GetReturnsListAsync(cabinet, camp);
        if (returnResponse?.Result?.Items?.Count > 0)
        {
          foreach (var ret in returnResponse.Result.Items)
          {
            var dbChangeDate = db.Returns.Where(r => r.ReturnId == ret.Id.ToString()).Select(r => r.ChangedAt).FirstOrDefault();
            if (dbChangeDate != null && dbChangeDate == ret.UpdateDate)
              continue;

            ret.Order = (await _ymSvc.GetOrdersAsync(cabinet, camp, new long[] { ret.OrderId }))?.Items?.FirstOrDefault();
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
                      var image = await _ymSvc.GetReturnImageAsync(cabinet, camp, ret.OrderId, ret.Id, decision.ReturnItemId, img);
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
              var warehouse = await _ymSvc.GetWarehouseByIdAsync(cabinet, ret.LogisticPickupPoint.Id);
              if (warehouse != null)
                ret.FulfillmentWarehouse = warehouse;
            }
            var @return = Return.Parse<YMReturn>(ret);
            @return.CabinetId = cabinet.Id;
            if (@return.TargetWarehouse != null)
            {
              @return.TargetWarehouse.Service = cabinet.Marketplace;
            }
            //if (@return.Scheme == SellScheme.FBO)
            //{
            //  //@return.OrderUrl = $"https://partner.market.yandex.ru/business/{camp.Business.Id}/returns?campaignId={camp.Id}&returnId={ret.Id}&partnerId=179624982&orderId={ret.OrderId}";
            //  //@return.Url = $"https://partner.market.yandex.ru/order/{ret.OrderId}?partnerId=179624982";
            //}
            @return.Products?.ForEach(p => p.Url = $"https://partner.market.yandex.ru/supplier/{camp.Id}/assortment/offer-card?tld=ru&offerId={p.OfferId}");
            returns.Add(@return);
          }
        }
      }
      return returns;
    }

    private async Task SyncOzonSupplies(Cabinet cabinet)
    {
      await _ozSvc!.GetSupplyRequests(cabinet);
      //if (supplies?.Count > 0)
      //{
      //  using ApplicationDbContext db = new ApplicationDbContext(new DbContextOptions<ApplicationDbContext>());
      //  foreach (var supply in supplies)
      //  {
      //    try
      //    {
      //      var dbSupply = await db.Supplies
      //          .Include(s => s.Items)
      //          .FirstOrDefaultAsync(s => s.SupplyId == supply.Id && s.CabinetId == cabinet.Id);
      //      if (dbSupply != null)
      //      {
      //        // Обновляем существующую поставку
      //        dbSupply.Status = supply.Status;
      //        dbSupply.ChangedAt = DateTime.UtcNow;
      //        // Обновляем товары в поставке
      //        foreach (var item in supply.Items)
      //        {
      //          var dbItem = dbSupply.Items.FirstOrDefault(i => i.ProductId == item.ProductId);
      //          if (dbItem != null)
      //          {
      //            dbItem.Quantity = item.Quantity;
      //          }
      //          else
      //          {
      //            dbSupply.Items.Add(new SupplyItem
      //            {
      //              ProductId = item.ProductId,
      //              Quantity = item.Quantity
      //            });
      //          }
      //        }
      //      }
      //      else
      //      {
      //        // Добавляем новую поставку
      //        dbSupply = new Supply
      //        {
      //          SupplyId = supply.Id,
      //          CabinetId = cabinet.Id,
      //          Status = supply.Status,
      //          CreatedAt = DateTime.UtcNow,
      //          ChangedAt = DateTime.UtcNow,
      //          Items = supply.Items.Select(i => new SupplyItem
      //          {
      //            ProductId = i.ProductId,
      //            Quantity = i.Quantity
      //          }).ToList()
      //        };
      //        db.Supplies.Add(dbSupply);
      //      }
      //    }
      //    catch (Exception exc)
      //    {
      //      await Extensions.SendDebugObject(
      //          supply,
      //          $"Ошибка синхронизации поставки {supply.Id} для кабинета {cabinet.Name} ({cabinet.Marketplace}):\n" +
      //          $"{exc.Message}\n{exc.InnerException?.Message}");
      //    }
      //  }
      //  await db.SaveChangesAsync();
      //}
    }

    #region === обработка товаров OZON (остался ваш метод) ===

    /// <summary>
    /// Синхронизирует список товаров из Ozon с локальной БД:
    /// - группирует продукты по OfferId, чтобы не было двукратной обработки;
    /// - объединяет все штрихкоды по каждому товару и убирает дубли;
    /// - добавляет новые штрихкоды, удаляет устаревшие, обновляет имя товара;
    /// - вставляет в БД полностью новые товары вместе со штрихкодами.
    /// </summary>
    private async Task<List<Product>> SyncOzonProducts(Cabinet cabinet)
    {
      // 1. Загружаем данные из Ozon
      var ids = await _ozSvc.GetOfferIdsAsync(cabinet);
      var allItems = await _ozSvc.GetProductsInfoAsync(cabinet, ids);

      // 2. Группируем товары по OfferId, объединяем штрихкоды
      var products = allItems
          .GroupBy(p => p.OfferId)
          .Select(gr =>
          {
            var first = gr.First();
            var merged = new Product
            {
              OfferId = first.OfferId,
              Name = first.Name,
              // все поля из first, кроме Barcodes
              Barcodes = gr
                  .SelectMany(p => p.Barcodes)
                  .Select(b => b.Barcode)
                  .Distinct()
                  .Select(code => new ProductBarcode { Barcode = code })
                  .ToList()
            };
            return merged;
          })
          .ToList();

      using var db = new ApplicationDbContext(new DbContextOptions<ApplicationDbContext>());

      foreach (var product in products)
      {
        try
        {
          // 3. Список входных штрихкодов (уже без дублей)
          var incomingCodes = product.Barcodes
                                     .Select(b => b.Barcode)
                                     .ToList();

          // 4. Ищем товар в БД по OfferId
          var dbProduct = await db.Products.Include(p => p.Barcodes)
                                   .FirstOrDefaultAsync(p => p.OfferId == product.OfferId);

          if (dbProduct != null)
          {
            // --- Обновляем существующий товар ---

            // 4.1. Читаем текущие штрихкоды из БД
            var existingCodes = await db.ProductBarcodes
                                        .Where(pb => pb.ProductId == dbProduct.Id)
                                        .Select(pb => pb.Barcode)
                                        .ToListAsync();

            // 4.2. Добавляем новые штрихкоды
            var toAdd = incomingCodes.Except(existingCodes);
            foreach (var code in toAdd)
            {
              db.ProductBarcodes.Add(new ProductBarcode
              {
                ProductId = dbProduct.Id,
                Barcode = code
              });
            }

            // 4.3. Удаляем устаревшие штрихкоды
            var toRemove = existingCodes.Except(incomingCodes);
            if (toRemove.Any())
            {
              var removeEntities = await db.ProductBarcodes
                                           .Where(pb => pb.ProductId == dbProduct.Id
                                                     && toRemove.Contains(pb.Barcode))
                                           .ToListAsync();
              db.ProductBarcodes.RemoveRange(removeEntities);
            }

            // 4.4. Обновляем имя (и другие поля, если нужно)
            dbProduct.Name = product.Name;
            // изменять навигационную коллекцию здесь не нужно
          }
          else
          {
            // --- Вставляем новый товар целиком ---
            db.Products.Add(product);
          }
        }
        catch (Exception exc)
        {
          await Extensions.SendDebugObject(
              product,
              $"Ошибка синхронизации товара {product.OfferId} ({product.Name}):\n" +
              $"{exc.Message}\n{exc.InnerException?.Message}");
        }
      }

      // 5. Сохраняем одним батчем
      await db.SaveChangesAsync();
      return products;
    }
    #endregion

    private async Task<List<Return>> SyncOzonReturns(Cabinet cabinet)
    {
      List<Return> _returns = new List<Return>();
      using ApplicationDbContext db = new ApplicationDbContext(new DbContextOptions<ApplicationDbContext>());
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
        var response = await _ozSvc.GetReturnsListAsync(cabinet, filter, lastId: lastId);
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

      foreach (var ret in returns)
      {
        var returnId = ret.Id.ToString();
        var dbChangeDate = db.Returns
            .Where(r => r.ReturnId == returnId)
            .Select(r => r.ChangedAt)
            .FirstOrDefault();
        if (DateTime.Equals(dbChangeDate, ret.Visual?.ChangeMoment))
          continue;

        var @return = Return.Parse<Ozon.Models.ReturnInfo>(ret);
        @return.CabinetId = cabinet.Id;
        _returns.Add(@return);
      }
      return _returns;
    }





    //#region === Wildberries: возвраты ===

    //private static async Task ProcessWbReturnsAsync(
    //    Cabinet cab,
    //    ApplicationDbContext db,
    //    WildberriesApiService wbSvc,
    //    CancellationToken ct)
    //{
    //  var response = await wbSvc.GetReturnsListAsync(cab) as Wildberries.Models.ReturnsListResponse;
    //  if (response?.Claims.Count == 0) return;

    //  // существующие возвраты одним запросом
    //  var existing = await db.Returns.AsNoTracking()
    //      .Where(r => r.CabinetId == cab.Id)
    //      .ToDictionaryAsync(r => r.ReturnId, ct);

    //  var changed = new List<Return>();
    //  foreach (var claim in response.Claims)
    //  {
    //    if (existing.TryGetValue(claim.Id.ToString(), out var dbRet) &&
    //        dbRet.ChangedAt == claim.DtUpdate)
    //      continue;

    //    var model = Return.Parse<Wildberries.Models.Claim>(claim);
    //    model.CabinetId = cab.Id;
    //    changed.Add(model);
    //  }

    //  if (changed.Count > 0)
    //  {
    //    await AddOrUpdateReturnsAsync(changed, db);
    //    await Extensions.SendDebugObject(changed,
    //        $"Возвраты Wildberries для кабинета {cab.Name} ({cab.Marketplace})");
    //  }
    //}

    // #endregion



    #region === хелперы ===
    /// <summary>
    /// Массовое добавление или обновление списка возвратов.
    /// При обновлении сохраняем старый идентификатор базы (PK),
    /// чтобы EF Core не пытался изменить ключ.
    /// </summary>
    private static async Task<List<Return>> AddOrUpdateReturnsAsync(List<Return> returns)
    {
      if (returns.Count == 0)
        return returns;

      using ApplicationDbContext db = new ApplicationDbContext(new DbContextOptions<ApplicationDbContext>());


      try
      {
        var geoApiKey = Program.Configuration.GetValue<string>("YandexGeo:ApiKey");
        var geoService = new YandexGeocoderService(geoApiKey);

        foreach (var ret in returns)
        {
          var cabinet = await db.Cabinets
               .FirstOrDefaultAsync(c => c.Id == ret.CabinetId);
          if (cabinet != null)
            ret.Cabinet = cabinet;

          if (ret.CurrentWarehouse != null)
          {
            // проверяем, что склад существует в БД
            var existingWarehouse = await db.Warehouses
                .FirstOrDefaultAsync(w => w.ExternalId == ret.CurrentWarehouse.ExternalId);
            if (existingWarehouse == null)
            {
              if (string.IsNullOrEmpty(ret.CurrentWarehouse?.Address?.FullAddress))
              {
                ret.CurrentWarehouse!.Address!.FullAddress = string.Empty;
                if (!string.IsNullOrEmpty(ret.CurrentWarehouse.Address.Country))
                  ret.CurrentWarehouse.Address.FullAddress += $"{ret.CurrentWarehouse.Address.Country}, ";
                if (!string.IsNullOrEmpty(ret.CurrentWarehouse.Address.City))
                  ret.CurrentWarehouse.Address.FullAddress += $"{ret.CurrentWarehouse.Address.City}, ";
                if (!string.IsNullOrEmpty(ret.CurrentWarehouse.Address.Street))
                  ret.CurrentWarehouse.Address.FullAddress += $"{ret.CurrentWarehouse.Address.Street}, ";
                if (!string.IsNullOrEmpty(ret.CurrentWarehouse.Address.House))
                  ret.CurrentWarehouse.Address.FullAddress += $"{ret.CurrentWarehouse.Address.House}, ";
                if (!string.IsNullOrEmpty(ret.CurrentWarehouse.Address.ZipCode))
                  ret.CurrentWarehouse.Address.FullAddress += $"{ret.CurrentWarehouse.Address.ZipCode}";
              }
              if (ret.CurrentWarehouse.Address?.Latitude == 0 || ret.CurrentWarehouse.Address?.Longitude == 0)
                ret.CurrentWarehouse.Address = await geoService.GetAddressAsync(ret.CurrentWarehouse.Address.FullAddress);

              ret.CurrentWarehouse.Service = ret.Cabinet.Marketplace;
              db.Warehouses.Add(ret.CurrentWarehouse);
            }
            else
            {
              ret.CurrentWarehouse = existingWarehouse;
            }
          }
          if (ret.TargetWarehouse != null)
          {
            // проверяем, что склад существует в БД
            var existingWarehouse = await db.Warehouses
                .FirstOrDefaultAsync(w => w.ExternalId == ret.TargetWarehouse.ExternalId);
            if (existingWarehouse == null)
            {
              if (string.IsNullOrEmpty(ret.TargetWarehouse?.Address?.FullAddress))
              {
                ret.TargetWarehouse!.Address!.FullAddress = string.Empty;
                if (!string.IsNullOrEmpty(ret.TargetWarehouse.Address.Country))
                  ret.TargetWarehouse.Address.FullAddress += $"{ret.TargetWarehouse.Address.Country}, ";
                if (!string.IsNullOrEmpty(ret.TargetWarehouse.Address.City))
                  ret.TargetWarehouse.Address.FullAddress += $"{ret.TargetWarehouse.Address.City}, ";
                if (!string.IsNullOrEmpty(ret.TargetWarehouse.Address.Street))
                  ret.TargetWarehouse.Address.FullAddress += $"{ret.TargetWarehouse.Address.Street}, ";
                if (!string.IsNullOrEmpty(ret.TargetWarehouse.Address.House))
                  ret.TargetWarehouse.Address.FullAddress += $"{ret.TargetWarehouse.Address.House}, ";
                if (!string.IsNullOrEmpty(ret.TargetWarehouse.Address.ZipCode))
                  ret.TargetWarehouse.Address.FullAddress += $"{ret.TargetWarehouse.Address.ZipCode}";
              }
              if (ret.TargetWarehouse.Address?.Latitude == 0 || ret.TargetWarehouse.Address?.Longitude == 0)
                ret.TargetWarehouse.Address = await geoService.GetAddressAsync(ret.TargetWarehouse.Address.FullAddress);
              ret.TargetWarehouse.Service = ret.Cabinet.Marketplace;
              db.Warehouses.Add(ret.TargetWarehouse);
              await db.SaveChangesAsync(); // Сохраняем склад сразу, чтобы избежать конфликта при добавлении возврата
            }
            else
            {
              ret.TargetWarehouse = existingWarehouse;
            }
          }

          // Ищем уже сохранённый возврат по внешнему ключу ReturnId
          var exists = await db.Returns
              .FirstOrDefaultAsync(r => r.ReturnId == ret.ReturnId);

          if (exists == null)
          {
            // Новый возврат — добавляем целиком
            db.Returns.Add(ret);

            await db.SaveChangesAsync();

            ReturnStatusChanged?.Invoke(new ReturnStatusChangedEventArgs(cabinet.Id, ret, null));
          }
          else
          {
            // Сохраняем PK из БД, чтобы не было конфликта
            ret.Id = exists.Id;

            // Копируем все поля из ret в существующую сущность,
            // включая ChangedAt, Status и т.п.
            db.Entry(exists).CurrentValues.SetValues(ret);

            await db.SaveChangesAsync();

            ReturnStatusChanged?.Invoke(new ReturnStatusChangedEventArgs(cabinet.Id, ret, null));
          }
        }

      }
      catch (Exception exc)
      {
        await Extensions.SendDebugMessage($"Ошибка при добавлении или обновлении складов: {exc.Message}\n{exc.InnerException?.Message}");
      }

      return returns;
    }


    #endregion

    #region === обработчики событий возвратов и поставок ===
    private async void OnReturnStatusChanged(ReturnStatusChangedEventArgs e)
    {
      using ApplicationDbContext context = new ApplicationDbContext(new DbContextOptions<ApplicationDbContext>());
      var workers = context.Cabinets
        .Include(c => c.AssignedWorkers)
        .ThenInclude(w => w.NotificationOptions)
        .FirstOrDefault(c => c.Id == e.CabinetId)?.AssignedWorkers;
      if (workers == null)
        return;

      e.Message ??= FormatReturnHtmlMessage(e.Return, e.Return.CreatedAt == e.Return.ChangedAt ? true : false);

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

    #endregion

    #region === форматирование сообщений для возвратов и поставок ===
    public static string FormatReturnHtmlMessage(Return x, bool? isNew, ReturnStatus? oldStatus = null)
    {
      var sb = new StringBuilder();
      // Реализация аналогичная с FormatReturnHtmlContent за исключением того, что здесь вместо <br> используется StringBuilder.AppendLine(" ")
      if (isNew.HasValue && isNew.Value)
      {
        sb.AppendLine($"<b>Новый возврат в {x.Cabinet.Marketplace.ToUpper()} / {x.Cabinet.Name}</b>");
        sb.AppendLine(" ");
      }
      else if (isNew.HasValue && !isNew.Value)
      {
        sb.AppendLine($"<b>Обновление возврата в {x.Cabinet.Marketplace.ToUpper()} / {x.Cabinet.Name}</b>");
        sb.AppendLine(" ");
      }
      sb.AppendLine($"<b>Схема:</b> {x.Scheme}");
      sb.AppendLine($"<b>Тип:</b> {x?.ReturnType.GetDisplayName()}");

      sb.AppendLine($"<b>ID возврата:</b> {x.ReturnId}");
      sb.AppendLine($"<b>ID заказа:</b> {x.OrderId}");
      sb.AppendLine($"<b>Номер заказа:</b> {x.OrderNumber}");
      sb.AppendLine($"<b>Дата заказа:</b> {x.OrderedAt}");
      if (!string.IsNullOrEmpty(x.ReturnReason))
        sb.AppendLine($"<b>Причина:</b> {x.ReturnReason}");
      if (!string.IsNullOrEmpty(x.ClientComment))
        sb.AppendLine($"<b>Комментарий:</b> {x.ClientComment}");
      sb.AppendLine(" ");
      if (x?.Products?.Count > 0)
      {
        sb.AppendLine("<b>Товары:</b>");
        int i = 1;
        foreach (var item in x.Products)
        {
          if (!string.IsNullOrEmpty(item.Url))
            sb.AppendLine($"<b>{i++}. </b><a href=\"{item.Url}\">{item.Name}</a>");
          else
            sb.AppendLine($"<b>{i++} </b>{item.Name}");
          sb.AppendLine($"<b>SKU:</b> {item.Sku}");
          sb.AppendLine($"<b>Артикул:</b> {item.OfferId}");
          sb.AppendLine($"<b>Количество:</b> {item.Count}");
          if (item.Images != null && item.Images.Count > 0)
          {
            sb.AppendLine($"<b>Фото:</b>");
            foreach (var img in item.Images)
            {
              sb.AppendLine($"<a href=\"{img.Url}\">{img.Url}</a>");
            }
          }
        }
        sb.AppendLine(" ");
      }

      sb.AppendLine($"<b>Создан:</b> {x.CreatedAt:dd.MM.yyyy HH:mm:ss}");
      sb.AppendLine($"<b>Обновлен:</b> {x.ChangedAt:dd.MM.yyyy HH:mm:ss}");
      sb.AppendLine(" ");
      if (x.TargetWarehouse?.Address != null)
      {
        x.TargetWarehouse.Address.FullAddress ??= $"{x.TargetWarehouse.Address.Country}, {x.TargetWarehouse.Address.City}, {x.TargetWarehouse.Address.Street}, {x.TargetWarehouse.Address.House}, {x.TargetWarehouse.Address.Office}";

        sb.AppendLine($"<b>Склад:</b> {x.TargetWarehouse.Name}");
        sb.AppendLine($"<b>Адрес:</b> {x.TargetWarehouse.Address.FullAddress}");
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
      sb.AppendLine($"<b>Локация:</b> {x.TargetWarehouse?.Name}");
      sb.AppendLine("<br>");
      if (x.TargetWarehouse?.Address != null)
      {
        sb.AppendLine($"<b>Адрес:</b>{x.TargetWarehouse.Address.Country}, {x.TargetWarehouse.Address.City}, {x.TargetWarehouse.Address.Street}, {x.TargetWarehouse.Address.House}, {x.TargetWarehouse.Address.Office}");
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
    #endregion
  }
}
