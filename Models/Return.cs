using automation.mbtdistr.ru.Services.Ozon.Models;
using automation.mbtdistr.ru.Services.Wildberries.Models;
using automation.mbtdistr.ru.Services.YandexMarket.Models;

using DevExpress.Data.Utils;

using Newtonsoft.Json;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace automation.mbtdistr.ru.Models
{
  /// <summary>
  /// Обобщенная информация о возвратах (для всех кабинетов).
  /// </summary>
  public class Return
  {
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity), DataGrid(false)]
    [Display(Name = "ID")]
    public int Id { get; set; }

    [Display(Name = "ID Кабинета"), ForeignKey(nameof(Cabinet)), DataGrid(false)]
    public int CabinetId { get; set; } // кабинет/бренд/ООО

    [Display(Name = "Кабинет")]
    public Cabinet Cabinet { get; set; }

    /// <summary>
    /// ID возврата в системе Claim.Id/ReturnInfo.Id/ReturnId
    /// </summary>
    [JsonProperty("returnId")]
    [Display(Name = "ID возврата")]
    public string? ReturnId { get; set; } // идентификатор возврата в системе Ozon/Wildberries/ЯндексМаркет

    /// <summary>
    /// Ссылка на возврат в системе Ozon/Wildberries/ЯндексМаркет
    /// </summary>
    [JsonProperty("url")]
    [Display(Name = "Ссылка на возврат")]
    public string? Url { get; set; } // ссылка на возврат в системе Ozon/Wildberries/ЯндексМаркет


    /// <summary>
    /// Id возврвата в системе Ozon/Wildberries/ЯндексМаркет
    /// </summary>
    [JsonProperty("orderId")]

    [Display(Name = "ID заказа")]
    public string? OrderExternalId { get; set; } // идентификатор заказа в системе Ozon/Wildberries/ЯндексМаркет

    /// <summary>
    /// Номер заказа в системе Ozon/Wildberries/ЯндексМаркет
    /// </summary>
    [JsonProperty("orderNumber")]
    [Display(Name = "Номер заказа")]
    public string? OrderNumber { get; set; } // номер заказа в системе Ozon/Wildberries/ЯндексМаркет

    /// <summary>
    /// Ссылка на заказ в системе Ozon/Wildberries/ЯндексМаркет
    /// </summary>
    [JsonProperty("orderUrl")]
    [Display(Name = "Ссылка на заказ")]
    public string? OrderUrl { get; set; } // ссылка на заказ в системе Ozon/Wildberries/ЯндексМаркет

    /// <summary>
    /// Дата создания возврата в системе Ozon/Wildberries/ЯндексМаркет
    /// </summary>
    [Display(Name = "Дата создания")]
    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// Номер отправления в системе Ozon/Wildberries/ЯндексМаркет
    /// </summary>
    [JsonProperty("postingNumber")]
    [Display(Name = "Номер отправления")]
    public string? PostingNumber { get; set; }

    /// <summary>
    /// Дата завершения возврата в системе Ozon/Wildberries/ЯндексМаркет
    /// </summary>
    [Display(Name = "Дата завершения")]
    [JsonProperty("resolvedAt")]
    public DateTime? ResolvedAt { get; set; }

    /// <summary>
    /// Дата последнего изменения возврата в системе Ozon/Wildberries/ЯндексМаркет
    /// </summary>
    [Display(Name = "Дата изменения")]
    public DateTime? ChangedAt { get; set; }

    /// <summary>
    /// Дата заказа в системе Ozon/Wildberries/ЯндексМаркет
    /// </summary>
    [Display(Name = "Дата заказа")]
    public DateTime? OrderedAt { get; internal set; }

    [DataGrid(false)]
    [Display(Name = "Компенсация")]
    public Compensation? Compensation { get; set; }

    [JsonIgnore, System.Text.Json.Serialization.JsonIgnore]
    [ForeignKey(nameof(Order))]
    public int? OrderId { get; set; } // идентификатор заказа, к которому относится возврат 

    [JsonIgnore, System.Text.Json.Serialization.JsonIgnore]
    public Order? Order { get; set; } // заказ, к которому относится возврат

    /// <summary>
    /// Идентификатор склада/ПВЗ, где находится возврат
    /// </summary>
    [ForeignKey(nameof(CurrentWarehouse)), DataGrid(false)]
    [Display(Name = "Идентификатор текущего склада")]
    public int? CurrentWarehouseId { get; set; }

    /// <summary>
    /// Склад/ПВЗ, где находится возврат
    /// </summary>
    [JsonProperty("currentWarehouse")]
    [Display(Name = "Текущий склад")]
    public Warehouse? CurrentWarehouse { get; set; }

    /// <summary>
    /// Склад/ПВЗ, куда направлен возврат
    /// </summary>
    [ForeignKey(nameof(TargetWarehouse)), DataGrid(false)]
    [Display(Name = "Идентификатор целевого склада")]
    public int? TargetWarehouseId { get; set; }

    /// <summary>
    /// Склад/ПВЗ, куда направлен возврат
    /// </summary>
    [JsonProperty("warehouse")]
    [Display(Name = "Целевой склад")]
    public Warehouse? TargetWarehouse { get; set; } // склад/ПВЗ, куда направлен возврат

    /// <summary>
    /// Причина возврата в системе Ozon/Wildberries/ЯндексМаркет
    /// </summary>
    [Display(Name = "Причина возврата")]
    public string? ReturnReason { get; set; } = string.Empty; // причина возврата в системе Ozon/Wildberries/ЯндексМаркет

    /// <summary>
    /// Комментарий к возврату в системе Ozon/Wildberries/ЯндексМаркет
    /// </summary>
    [Display(Name = "Комментарий к возврату")]
    [JsonProperty("clientComment")]
    public string ClientComment { get; set; } = string.Empty; // комментарий к возврату

    /// <summary>
    /// Схема реализации (FBS/FBO)
    /// </summary>
    [JsonProperty("scheme")]
    [Display(Name = "Схема реализации")]
    public SellScheme Scheme { get; set; }

    [JsonProperty("products")]
    [Display(Name = "Товары")]
    public List<ReturnProduct>? Products { get; set; }

    /// <summary>
    /// Тип возврата (возврат или невыкуп)
    /// </summary>
    [JsonProperty("returnType")]
    [Display(Name = "Тип возврата")]
    public ReturnType ReturnType { get; set; }

    /// <summary>
    /// Подробности о хранении (стоимость, ключевых даты и т.д.)
    /// </summary>
    [JsonProperty("storage")]
    [Display(Name = "Хранение")]
    public Storage? Storage { get; set; }

    /// <summary>
    /// Статус транспортировки возврата
    /// </summary>
    [JsonProperty("shipmentStatus")]
    [Display(Name = "Статус отправления")]
    public YMReturnShipmentStatusType ShipmentStatus { get; set; } = YMReturnShipmentStatusType.Unknown;

    /// <summary>
    /// Активность возврата (активен или нет)
    /// </summary>
    [JsonProperty("active")]
    [Display(Name = "Активный")]
    [DataGrid(false)]
    public bool Active { get; set; } = true;

    /// <summary>
    /// Метод преобразования объекта возврата в общий объект возврата.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="apiReturnObject"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public static Return Parse<T>(object apiReturnObject, Order? order = null)
    {
      Return @return = new Return();
      var type = typeof(T);

      Extensions.SendDebugObject(apiReturnObject);

      switch (type.Name)
      {
        case nameof(Claim):
          var claim = (Claim)apiReturnObject;
          ParseClaim(ref @return, claim);
          break;
        case nameof(automation.mbtdistr.ru.Services.Ozon.Models.ReturnInfo):
          var returnInfo = (automation.mbtdistr.ru.Services.Ozon.Models.ReturnInfo)apiReturnObject;
          ParseReturnInfo(ref @return, returnInfo);
          break;
        case nameof(YMReturn):
          var ymReturn = (YMReturn)apiReturnObject;
          ParseYMReturn(ref @return, ymReturn, order);
          break;
        default:
          throw new NotImplementedException($"Неизвестный тип возврата: {type.Name}");
      }

      return @return;
    }

    private static void ParseClaim(ref Return @return, Claim claim)
    {
      @return.ReturnId = claim.Id;
      @return.OrderExternalId = claim.Srid;
      @return.ChangedAt = claim.DtUpdate;
      @return.OrderedAt = claim.OrderDt;
      @return.CreatedAt = claim.Dt;
      @return.ClientComment = claim.UserComment;
      @return.ReturnReason = claim.UserComment;
      @return.Products ??= new List<ReturnProduct>();

      if (!string.IsNullOrEmpty(claim.ImtName))
      {
        var product = new ReturnProduct()
        {
          Name = claim.ImtName,
          Sku = claim.NmId.ToString(),
          Count = 1
        };

        if (claim?.Photos?.Count > 0)
          foreach (var photo in claim.Photos)
          {
            ReturnImage returnImage = new ReturnImage()
            {
              Url = photo
            };
            product.Images ??= new List<ReturnImage>();
            product.Images.Add(returnImage);
          }
        if (claim?.VideoPaths?.Count > 0)
          foreach (var video in claim.VideoPaths)
          {
            ReturnImage returnImage = new ReturnImage()
            {
              Url = video
            };
            product.Images ??= new List<ReturnImage>();
            product.Images.Add(returnImage);
          }

        @return.Products.Add(product);
      }
    }

    private static void ParseReturnInfo(ref Return @return, automation.mbtdistr.ru.Services.Ozon.Models.ReturnInfo returnInfo)
    {
      @return.ChangedAt = returnInfo.Visual?.ChangeMoment;
      @return.ReturnId = returnInfo.Id.ToString();
      @return.OrderExternalId = returnInfo.OrderId.ToString();
      @return.OrderNumber = returnInfo.OrderNumber?.ToString();
      @return.ReturnReason = returnInfo.ReturnReasonName;
      @return.CreatedAt = returnInfo.Logistic?.ReturnDate;
      @return.PostingNumber = returnInfo.PostingNumber;

      if (returnInfo?.Type?.ToLower() == "cancellation")
        @return.ReturnType = Models.ReturnType.Unredeemed;
      else if (returnInfo?.Type?.ToLower() == "clientreturn")
        @return.ReturnType = Models.ReturnType.Return;
      else if (returnInfo?.Type?.ToLower() == "partialreturn")
        @return.ReturnType = Models.ReturnType.PartialReturn;
      else if (returnInfo?.Type?.ToLower() == "fullreturn")
        @return.ReturnType = Models.ReturnType.FullReturn;
      else
        @return.ReturnType = Models.ReturnType.Unknown;

      @return.Scheme = returnInfo?.Schema?.ToUpper() == "FBS" ? SellScheme.FBS : returnInfo?.Schema?.ToUpper() == "FBO" ? SellScheme.FBO : SellScheme.Unknown;

      if (returnInfo?.Visual?.Status?.SysName != null)
        @return.ShipmentStatus = OzStatusMapper.ToReturnShipmentStatus(returnInfo.Visual.Status.SysName.GetValueOrDefault());

      if (returnInfo?.Logistic?.FinalMoment != null)
        @return.ResolvedAt = returnInfo?.Logistic?.FinalMoment;

      @return.Active = @return.ResolvedAt == null ? true : false;

      if (returnInfo?.Product != null)
      {
        ReturnProduct returnProduct = new ReturnProduct()
        {
          Sku = returnInfo.Product.Sku.ToString(),
          Count = returnInfo.Product.Quantity,
          OfferId = returnInfo.Product.OfferId,
          Name = returnInfo.Product.Name,
          Price = new Price()
          {
            Amount = (double?)returnInfo.Product.Price.Price,
            Currency = returnInfo.Product.Price.CurrencyCode,
          },
        };
        @return.Products = new List<ReturnProduct> { returnProduct };
      }
      if (returnInfo?.Place != null)
      {
        //id, name, address
        @return.CurrentWarehouse = new Warehouse()
        {
          ExternalId = returnInfo.Place.Id.ToString(),
          Name = returnInfo.Place.Name,
          Service = "OZON",
          Address = new Address()
          {
            FullAddress = returnInfo.Place.Address,
          }
        };
      }
      if (returnInfo?.TargetPlace != null)
      {
        @return.TargetWarehouse = new Warehouse()
        {
          ExternalId = returnInfo.TargetPlace.Id.ToString(),
          Name = returnInfo.TargetPlace.Name,
          Service = "OZON",
          Address = new Address()
          {
            FullAddress = returnInfo.TargetPlace.Address,
          }
        };
      }
      if (returnInfo?.Storage != null)
      {
        @return.Storage = new Storage()
        {
          ArrivedDate = returnInfo.Storage.ArrivedMoment,
          UtilizationForecastDate = returnInfo.Storage.UtilizationForecastDate,
          Days = returnInfo.Storage.Days,
          Price = returnInfo.Storage.Sum.Price
        };
      }
    }

    private static void ParseYMReturn(ref Return @return, YMReturn ymReturn, Order? order = null)
    {
      @return.ChangedAt = ymReturn.UpdateDate;
      @return.OrderedAt = ymReturn?.Order?.CreationDate;
      @return.CreatedAt = ymReturn?.CreationDate;
      @return.ReturnId = ymReturn?.Id.ToString();
      @return.OrderExternalId = ymReturn?.OrderId.ToString();
      @return.OrderNumber = ymReturn?.OrderId.ToString();
      @return.ReturnType = ymReturn.ReturnType;
      @return.Scheme = ymReturn.SellScheme;

      @return.Order = order ?? null;
      @return.CabinetId = @return.Order?.CabinetId ?? 0;

      @return.ResolvedAt = ymReturn?.RefundStatus == YMRefundStatus.Refunded ? ymReturn.UpdateDate : null;
      @return.Active = @return.ResolvedAt != null ? false : true;

      @return.ShipmentStatus = ymReturn?.ShipmentStatus ?? YMReturnShipmentStatusType.Unknown;

      @return.Products ??= new List<ReturnProduct>();
      if (ymReturn?.Items?.Count > 0)
        foreach (var item in ymReturn.Items)
        {
          ReturnProduct returnProduct = new ReturnProduct()
          {
            Sku = item.MarketSku.ToString(),
            Count = item.Count,
            OfferId = item.ShopSku,
            Name = ymReturn?.Order?.Items?.FirstOrDefault(i => i.OfferId == item.ShopSku)?.OfferName,
            Price = new Price()
            {
              Amount = (double?)ymReturn?.Order?.Items?.FirstOrDefault(i => i.OfferId == item.ShopSku)?.Price,
              Currency = ymReturn?.Order?.Currency.ToString(),
            },
            //получаем все картинки из всех решений
          };
          if (item?.Decisions?.Count > 0)
          {
            @return.ClientComment += $"{string.Join("\n", item.Decisions.Select(d => d.Comment))}\n";
            @return.ReturnReason += $"{string.Join(", ", item.Decisions.Select(d => $"{d.ReasonType.GetDisplayName()}, {d.SubreasonType?.GetDisplayName()}"))}\n";
            returnProduct.Images = item?.Decisions?.SelectMany(d => d.Images)?.Select(i => new ReturnImage() { Url = i })?.ToList();
          }
          @return.Products.Add(returnProduct);
        }
      //if (ymReturn?.FulfillmentWarehouse != null)
      //{
      //  if (ymReturn.ShipmentStatus == YMReturnShipmentStatusType.InTransit)
      //  {
      //    @return.TargetWarehouse = new Warehouse()
      //    {
      //      ExternalId = ymReturn.FulfillmentWarehouse.Id.ToString(),
      //      Name = ymReturn.FulfillmentWarehouse.Name,
      //      Address = new Address()
      //      {
      //        City = ymReturn.FulfillmentWarehouse.Address?.City,
      //        Street = ymReturn.FulfillmentWarehouse.Address?.Street,
      //        House = ymReturn.FulfillmentWarehouse.Address?.Building,
      //        Office = ymReturn.FulfillmentWarehouse.Address?.Number,
      //      }
      //    };
      //    if (ymReturn.FulfillmentWarehouse.Address?.Gps != null)
      //    {
      //      @return.TargetWarehouse.Address.Latitude = (double)ymReturn.FulfillmentWarehouse.Address.Gps.Latitude;
      //      @return.TargetWarehouse.Address.Longitude = (double)ymReturn.FulfillmentWarehouse.Address.Gps.Longitude;
      //    }

      //  }
      //  else
      //  {

      //    @return.CurrentWarehouse = new Warehouse()
      //    {
      //      ExternalId = ymReturn.FulfillmentWarehouse.Id.ToString(),
      //      Name = ymReturn.FulfillmentWarehouse.Name,
      //      Address = new Address()
      //      {
      //        City = ymReturn.FulfillmentWarehouse.Address?.City,
      //        Street = ymReturn.FulfillmentWarehouse.Address?.Street,
      //        House = ymReturn.FulfillmentWarehouse.Address?.Building,
      //        Office = ymReturn.FulfillmentWarehouse.Address?.Number,
      //      }
      //    };
      //    if (ymReturn.FulfillmentWarehouse.Address?.Gps != null)
      //    {
      //      @return.CurrentWarehouse.Address.Latitude = (double)ymReturn.FulfillmentWarehouse.Address.Gps.Latitude;
      //      @return.CurrentWarehouse.Address.Longitude = (double)ymReturn.FulfillmentWarehouse.Address.Gps.Longitude;
      //    }
      //  }
      //}
    }
  }


  public static class OzStatusMapper
  {
    private static readonly IReadOnlyDictionary<OZVisualStatus, YMReturnShipmentStatusType> _map
        = new Dictionary<OZVisualStatus, YMReturnShipmentStatusType>
        {
          // ————————— Начальные / споровые этапы (логистика ещё не задействована)
          [OZVisualStatus.DisputeOpened] = YMReturnShipmentStatusType.Created,
          [OZVisualStatus.DisputeYouOpened] = YMReturnShipmentStatusType.Created,
          [OZVisualStatus.DisputeOpening] = YMReturnShipmentStatusType.Created,
          [OZVisualStatus.OnSellerApproval] = YMReturnShipmentStatusType.Created,
          [OZVisualStatus.OnSellerClarification] = YMReturnShipmentStatusType.Created,
          [OZVisualStatus.OnSellerClarificationAfterPartialCompensation]
                                                                       = YMReturnShipmentStatusType.Created,
          [OZVisualStatus.OfferedPartialCompensation] = YMReturnShipmentStatusType.Created,
          [OZVisualStatus.CompensationOffered] = YMReturnShipmentStatusType.Created,
          [OZVisualStatus.WaitingCompensation] = YMReturnShipmentStatusType.Created,
          [OZVisualStatus.SendingError] = YMReturnShipmentStatusType.Created,
          [OZVisualStatus.CompensationRejected] = YMReturnShipmentStatusType.Created,
          [OZVisualStatus.CompensationRejectedBySla] = YMReturnShipmentStatusType.Created,
          [OZVisualStatus.CompensationRejectedBySeller] = YMReturnShipmentStatusType.Created,
          [OZVisualStatus.Approved] = YMReturnShipmentStatusType.Created,
          [OZVisualStatus.ApprovedByOzon] = YMReturnShipmentStatusType.Created,
          [OZVisualStatus.WaitingShipment] = YMReturnShipmentStatusType.Created,

          // ————————— Прибытие на пункт выдачи
          [OZVisualStatus.ArrivedAtReturnPlace] = YMReturnShipmentStatusType.ReadyForPickup,

          // ————————— Транспортировка
          [OZVisualStatus.MovingToSeller] = YMReturnShipmentStatusType.InTransit,
          [OZVisualStatus.ReturningToSellerByCourier] = YMReturnShipmentStatusType.InTransit,
          [OZVisualStatus.MovingToOzon] = YMReturnShipmentStatusType.InTransit,

          // ————————— Конечная доставка
          [OZVisualStatus.ReceivedBySeller] = YMReturnShipmentStatusType.Picked,
          [OZVisualStatus.ReturnedToOzon] = YMReturnShipmentStatusType.FulfilmentReceived,

          // ————————— Утилизация
          [OZVisualStatus.Utilizing] = YMReturnShipmentStatusType.PreparedForUtilization,
          [OZVisualStatus.Utilized] = YMReturnShipmentStatusType.Utilized,

          // ————————— Отмена / финальное закрытие выплатой
          [OZVisualStatus.Cancelled] = YMReturnShipmentStatusType.Cancelled,
          [OZVisualStatus.Rejected] = YMReturnShipmentStatusType.Cancelled,
          [OZVisualStatus.CrmRejected] = YMReturnShipmentStatusType.Cancelled,
          [OZVisualStatus.CancelledDisputeNotOpen] = YMReturnShipmentStatusType.Cancelled,
          [OZVisualStatus.ReturnMoneyApproved] = YMReturnShipmentStatusType.Cancelled,
          [OZVisualStatus.PartialCompensationReturned] = YMReturnShipmentStatusType.Cancelled,
          [OZVisualStatus.ReturnCompensated] = YMReturnShipmentStatusType.Cancelled,
          [OZVisualStatus.PartialCompensationInProcess] = YMReturnShipmentStatusType.Cancelled,
          [OZVisualStatus.MoneyReturned] = YMReturnShipmentStatusType.Cancelled,
          [OZVisualStatus.MoneyReturnedBySystem] = YMReturnShipmentStatusType.Cancelled
        };

    /// <summary>
    /// Преобразует визуальный статус Ozon в прежде логистический статус возвратной отправки.
    /// </summary>
    public static YMReturnShipmentStatusType ToReturnShipmentStatus(this OZVisualStatus status)
        => _map[status];
  }
}
