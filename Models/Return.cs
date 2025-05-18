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
    [JsonProperty("returnNumber")]
    [Display(Name = "Номер возврата")]
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

    [ForeignKey(nameof(Warehouse)), DataGrid(false)]
    [Display(Name = "ID склада")]
    public int? WarehouseId { get; set; } // идентификатор склада/ПВЗ, куда возвращается товар

    [JsonProperty("warehouse")]
    [Display(Name = "Склад")]
    public Warehouse? Warehouse { get; set; } // склад/ПВЗ, куда возвращается товар

    //[DataGrid(false), NotMapped]
    //[Display(Name = "Информация о возврате")]
    //public ReturnMainInfo Info { get; set; } = new ReturnMainInfo(); // информация о возврате

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
    public SellScheme? Scheme { get; set; }

    public List<ReturnProduct>? Products { get; set; }

    /// <summary>
    /// Тип возврата (возврат или невыкуп)
    /// </summary>
    [JsonProperty("returnType")]
    [Display(Name = "Тип возврата")]
    public ReturnType? ReturnType { get; set; }

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
      @return.ReturnType = returnInfo.Type == "Cancellation" ? automation.mbtdistr.ru.Models.ReturnType.Unredeemed : automation.mbtdistr.ru.Models.ReturnType.Return;

      @return.Scheme = returnInfo.Schema == "FBS" ? SellScheme.FBS : SellScheme.FBO;

    }

    private static void ParseYMReturn(ref Return @return, YMReturn ymReturn)
    {
      @return.ChangedAt = ymReturn.UpdateDate;
      @return.OrderedAt = ymReturn?.Order?.CreationDate;
      @return.CreatedAt = ymReturn?.CreationDate;
      @return.ReturnId = ymReturn?.Id.ToString();
      @return.OrderId = ymReturn?.OrderId.ToString();
      @return.OrderNumber = ymReturn?.OrderId.ToString();
      @return.ReturnType = ymReturn?.ReturnType;
      @return.Scheme = ymReturn?.ShipmentRecipientType == YMShipmentRecipientType.Shop ? SellScheme.FBS : SellScheme.FBO;
      @return.Products ??= new List<ReturnProduct>();

      if (ymReturn?.Items?.Count > 0)
        foreach (var item in ymReturn.Items)
        {
          ReturnProduct returnProduct = new ReturnProduct()
          {
            Count = item.Count,
            Sku = item.MarketSku.ToString(),

          };

          @return.Products.Add(returnProduct);

          if (item?.Decisions?.Count > 0)
          {
            @return.ClientComment += $"{string.Join("\n", item.Decisions.Select(d => d.Comment))}\n";
            @return.ReturnReason += $"{string.Join(", ", item.Decisions.Select(d => $"{d.ReasonType.GetDisplayName()}, {d.SubreasonType?.GetDisplayName()}"))}\n";
            returnProduct.Images = item?.Decisions?.SelectMany(d => d.Images)?.Select(i => new ReturnImage() { Url = i })?.ToList();
          }
        }
      if (ymReturn?.LogisticPickupPoint != null)
      {
        @return.Warehouse = new Warehouse()
        {
          ExternalId = ymReturn.LogisticPickupPoint.Id.ToString(),
          Name = ymReturn.LogisticPickupPoint.Name,
          Address = new Address()
          {
            Country = ymReturn.LogisticPickupPoint.Address?.Country,
            City = ymReturn.LogisticPickupPoint.Address?.City,
            Street = ymReturn.LogisticPickupPoint.Address?.Street,
            House = ymReturn.LogisticPickupPoint.Address?.House
          }
        };
      }
    }
  }
}