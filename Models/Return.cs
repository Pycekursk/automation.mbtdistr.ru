using automation.mbtdistr.ru.Services.Wildberries.Models;
using automation.mbtdistr.ru.Services.YandexMarket.Models;

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

    [Display(Name = "Кабинет"), JsonIgnore, System.Text.Json.Serialization.JsonIgnore]
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
    public string? OrderId { get; set; } // идентификатор заказа в системе Ozon/Wildberries/ЯндексМаркет

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

    [Display(Name = "Дата завершения")]
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

    public List<ReturnProduct>? Products { get; set; }

    /// <summary>
    /// Тип возврата (возврат или невыкуп)
    /// </summary>
    [JsonProperty("returnType")]
    [Display(Name = "Тип возврата")]
    public ReturnType ReturnType { get; set; }

    /// <summary>
    /// Метод преобразования объекта возврата в общий объект возврата.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="apiReturnObject"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public static Return Parse<T>(object apiReturnObject)
    {
      Return @return = new Return();
      var type = typeof(T);

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
          ParseYMReturn(ref @return, ymReturn);
          break;
        default:
          throw new NotImplementedException($"Неизвестный тип возврата: {type.Name}");
      }

      return @return;
    }

    private static void ParseClaim(ref Return @return, Claim claim)
    {
      @return.ReturnId = claim.Id;
      @return.OrderId = claim.Srid;
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
      @return.OrderId = returnInfo.OrderId.ToString();
      @return.OrderNumber = returnInfo.OrderNumber?.ToString();
      @return.ReturnReason = returnInfo.ReturnReasonName;
      @return.CreatedAt = returnInfo.Logistic?.ReturnDate;
      if (returnInfo?.Type?.ToLower() == "cancellation")
        @return.ReturnType = Models.ReturnType.Unredeemed;
      else if (returnInfo?.Type?.ToLower() == "clientreturn")
        @return.ReturnType = Models.ReturnType.Return;
      else
        @return.ReturnType = Models.ReturnType.Unknown;

      @return.Scheme = returnInfo.Schema?.ToUpper() == "FBS" ? SellScheme.FBS : returnInfo.Schema?.ToUpper() == "FBO" ? SellScheme.FBO : SellScheme.Unknown;

      if (returnInfo.Product != null)
      {
        ReturnProduct returnProduct = new ReturnProduct()
        {
          Sku = returnInfo.Product.Sku.ToString(),
          Count = returnInfo.Product.Quantity,
          OfferId = returnInfo.Product.OfferId,
          Name = returnInfo.Product.Name,
          Price = new Price()
          {
            Amount = (decimal?)returnInfo.Product.Price.Price,
            Currency = returnInfo.Product.Price.CurrencyCode,
          },

          //получаем все картинки из всех решений
        };
        @return.Products = new List<ReturnProduct> { returnProduct };
      }

      if (returnInfo.Place != null)
      {
        //id, name, address
        @return.TargetWarehouse = new Warehouse()
        {
          ExternalId = returnInfo.Place.Id.ToString(),
          Name = returnInfo.Place.Name,
          Address = new Address()
          {
            FullAddress = returnInfo.Place.Address,
          }
        };
      }
    }

    private static void ParseYMReturn(ref Return @return, YMReturn ymReturn)
    {
      @return.ChangedAt = ymReturn.UpdateDate;
      @return.OrderedAt = ymReturn?.Order?.CreationDate;
      @return.CreatedAt = ymReturn?.CreationDate;
      @return.ReturnId = ymReturn?.Id.ToString();
      @return.OrderId = ymReturn?.OrderId.ToString();
      @return.OrderNumber = ymReturn?.OrderId.ToString();
      @return.ReturnType = ymReturn.ReturnType;
      @return.Scheme = ymReturn?.ShipmentRecipientType == YMShipmentRecipientType.Shop ? SellScheme.FBS : SellScheme.FBO;
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
              Amount = (decimal?)ymReturn?.Order?.Items?.FirstOrDefault(i => i.OfferId == item.ShopSku)?.Price,
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
      if (ymReturn?.FulfillmentWarehouse != null)
      {
        @return.TargetWarehouse = new Warehouse()
        {
          ExternalId = ymReturn.FulfillmentWarehouse.Id.ToString(),
          Name = ymReturn.FulfillmentWarehouse.Name,
          Address = new Address()
          {
            City = ymReturn.FulfillmentWarehouse.Address?.City,
            Street = ymReturn.FulfillmentWarehouse.Address?.Street,
            House = ymReturn.FulfillmentWarehouse.Address?.Building,
            Office = ymReturn.FulfillmentWarehouse.Address?.Number,
          }
        };
        @return.TargetWarehouse.Address.FullAddress = $"{@return.TargetWarehouse.Address.City}, {@return.TargetWarehouse.Address.Street} {@return.TargetWarehouse.Address.House} {@return.TargetWarehouse.Address.Office}";
        if (ymReturn.FulfillmentWarehouse.Address?.Gps != null)
        {
          @return.TargetWarehouse.Address.Latitude = ymReturn.FulfillmentWarehouse.Address.Gps.Latitude;
          @return.TargetWarehouse.Address.Longitude = ymReturn.FulfillmentWarehouse.Address.Gps.Longitude;
        }
      }

    }
  }
}
