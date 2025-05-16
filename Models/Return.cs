using automation.mbtdistr.ru.Services.Wildberries.Models;
using automation.mbtdistr.ru.Services.YandexMarket.Models;

using Newtonsoft.Json;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

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

    [Display(Name = "Склад")]
    public Warehouse? Warehouse { get; set; } // склад/ПВЗ, куда возвращается товар

    [DataGrid(false)]
    [Display(Name = "Информация о возврате")]
    public ReturnMainInfo Info { get; set; } = new ReturnMainInfo(); // информация о возврате

    /// <summary>
    /// Причина возврата в системе Ozon/Wildberries/ЯндексМаркет
    /// </summary>
    [Display(Name = "Причина возврата")]
    public string? ReturnReason { get; set; } = string.Empty; // причина возврата в системе Ozon/Wildberries/ЯндексМаркет

    public string ClientComment { get; set; } = string.Empty; // комментарий к возврату

    public SellScheme? Scheme { get; set; }

    public List<ReturnProduct>? Products { get; set; }

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
        case nameof(ReturnInfo):
          var returnInfo = (ReturnInfo)apiReturnObject;
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
      @return.ChangedAt = claim.DtUpdate;
      @return.OrderedAt = claim.OrderDt;
      @return.CreatedAt = claim.Dt;
    }

    private static void ParseReturnInfo(ref Return @return, ReturnInfo returnInfo)
    {

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

      if (ymReturn?.Items?.Count > 0)
        foreach (var item in ymReturn.Items)
        {

          ReturnProduct returnProduct = new ReturnProduct()
          {
            Count = item.Count,
            Sku = item.MarketSku.ToString(),

          };

          if (item?.Decisions?.Count > 0)
          {
            @return.ClientComment += $"{item?.Decisions?.Select(d => d.Comment)?.FirstOrDefault()}\n";
            @return.ReturnReason += $"{item?.Decisions?.Select(d => $"{d.ReasonType.GetDisplayName()} {d.SubreasonType?.GetDisplayName()}")?.FirstOrDefault()}\n";
            returnProduct.Images = item?.Decisions?.SelectMany(d => d.Images)?.Select(i => new ReturnImage() { Url = i })?.ToList();
          }
        }
    }
  }

  /// <summary>
  /// Тип возврата.
  /// </summary>
  [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
  public enum ReturnType
  {
    /// <summary>
    /// Невыкуп.
    /// </summary>
    [EnumMember(Value = "UNREDEEMED")]

    [JsonProperty("UNREDEEMED")]
    [Display(Name = "Невыкуп")]
    Unredeemed,

    /// <summary>
    /// Возврат.
    /// </summary>
    [EnumMember(Value = "RETURN")]

    [JsonProperty("RETURN")]
    [Display(Name = "Возврат")]
    Return,

    /// <summary>
    /// Неизвестный тип.
    /// </summary>
    [EnumMember(Value = "UNKNOWN")]

    [JsonProperty("UNKNOWN")]
    [Display(Name = "Неизвестный тип")]
    Unknown = 0
  }

  public class ReturnProduct
  {
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Sku { get; set; }

    public string? OfferId { get; set; }

    public List<ReturnImage>? Images { get; set; }

    public int Count { get; set; }

    [ForeignKey("Return")]
    public int ReturnId { get; set; }

    [Newtonsoft.Json.JsonIgnore]
    public Return Return { get; set; }
  }

  public class ReturnImage
  {
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [ForeignKey("ReturnProduct")]
    public int ReturnProductId { get; set; }

    [Newtonsoft.Json.JsonIgnore]
    public ReturnProduct ReturnProduct { get; set; }

    public string Url { get; set; } = string.Empty;
  }

  /// <summary>
  /// Класс обьекта склада/пвз
  /// </summary>
  public class Warehouse
  {
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public string? ExternalId { get; set; }
    public string? Name { get; set; }
    public Address? Address { get; set; }
    public string? Phone { get; set; }

    [Newtonsoft.Json.JsonIgnore]
    public ICollection<Return>? Returns { get; set; } = new List<Return>();
  }

  public class Address
  {
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public string? Street { get; set; }
    public string? House { get; set; }
    public string? Office { get; set; }
    public string? ZipCode { get; set; }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }

    [ForeignKey("Warehouse")]
    public int WarehouseId { get; set; }

    [System.Text.Json.Serialization.JsonIgnore, Newtonsoft.Json.JsonIgnore]
    public Warehouse Warehouse { get; set; }
  }

  public class ReturnInfo
  {
    public ReturnInfo(int returnId)
    {
      ReturnId = returnId;
    }

    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public string ExternalId { get; set; } = string.Empty;

    [ForeignKey("Return")]
    public int ReturnId { get; set; }

    [System.Text.Json.Serialization.JsonIgnore, Newtonsoft.Json.JsonIgnore]
    public Return Return { get; set; }
  }
  [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
  public enum ReturnStatus
  {
    [EnumMember(Value = "DisputeOpened")]
    [Display(Name = "Открыт спор с покупателем")]
    DisputeOpened,

    [EnumMember(Value = "OnSellerApproval")]
    [Display(Name = "На согласовании у продавца")]
    OnSellerApproval,

    [EnumMember(Value = "ArrivedAtReturnPlace")]
    [Display(Name = "В пункте выдачи")]
    ArrivedAtReturnPlace,

    [EnumMember(Value = "OnSellerClarification")]
    [Display(Name = "На уточнении у продавца")]
    OnSellerClarification,

    [EnumMember(Value = "OnSellerClarificationAfterPartialCompensation")]
    [Display(Name = "На уточнении у продавца после частичной компенсации")]
    OnSellerClarificationAfterPartialCompensation,

    [EnumMember(Value = "OfferedPartialCompensation")]
    [Display(Name = "Предложена частичная компенсация")]
    OfferedPartialCompensation,

    [EnumMember(Value = "ReturnMoneyApproved")]
    [Display(Name = "Одобрен возврат денег")]
    ReturnMoneyApproved,

    [EnumMember(Value = "PartialCompensationReturned")]
    [Display(Name = "Вернули часть денег")]
    PartialCompensationReturned,

    [EnumMember(Value = "CancelledDisputeNotOpen")]
    [Display(Name = "Возврат отклонён, спор не открыт")]
    CancelledDisputeNotOpen,

    [EnumMember(Value = "Rejected")]
    [Display(Name = "Заявка отклонена")]
    Rejected,

    [EnumMember(Value = "CrmRejected")]
    [Display(Name = "Заявка отклонена Ozon")]
    CrmRejected,

    [EnumMember(Value = "Cancelled")]
    [Display(Name = "Заявка отменена")]
    Cancelled,

    [EnumMember(Value = "Approved")]
    [Display(Name = "Заявка одобрена продавцом")]
    Approved,

    [EnumMember(Value = "ApprovedByOzon")]
    [Display(Name = "Заявка одобрена Ozon")]
    ApprovedByOzon,

    [EnumMember(Value = "ReceivedBySeller")]
    [Display(Name = "Продавец получил возврат")]
    ReceivedBySeller,

    [EnumMember(Value = "MovingToSeller")]
    [Display(Name = "Возврат на пути к продавцу")]
    MovingToSeller,

    [EnumMember(Value = "ReturnCompensated")]
    [Display(Name = "Продавец получил компенсацию")]
    ReturnCompensated,

    [EnumMember(Value = "ReturningToSellerByCourier")]
    [Display(Name = "Курьер везёт возврат продавцу")]
    ReturningToSellerByCourier,

    [EnumMember(Value = "Utilizing")]
    [Display(Name = "На утилизации")]
    Utilizing,

    [EnumMember(Value = "Utilized")]
    [Display(Name = "Утилизирован")]
    Utilized,

    [EnumMember(Value = "MoneyReturned")]
    [Display(Name = "Покупателю вернули всю сумму")]
    MoneyReturned,

    [EnumMember(Value = "PartialCompensationInProcess")]
    [Display(Name = "Одобрен частичный возврат денег")]
    PartialCompensationInProcess,

    [EnumMember(Value = "DisputeYouOpened")]
    [Display(Name = "Продавец открыл спор")]
    DisputeYouOpened,

    [EnumMember(Value = "CompensationRejected")]
    [Display(Name = "Отказано в компенсации")]
    CompensationRejected,

    [EnumMember(Value = "DisputeOpening")]
    [Display(Name = "Обращение в поддержку отправлено")]
    DisputeOpening,

    [EnumMember(Value = "CompensationOffered")]
    [Display(Name = "Ожидает вашего решения по компенсации")]
    CompensationOffered,

    [EnumMember(Value = "WaitingCompensation")]
    [Display(Name = "Ожидает компенсации")]
    WaitingCompensation,

    [EnumMember(Value = "SendingError")]
    [Display(Name = "Ошибка при отправке обращения в поддержку")]
    SendingError,

    [EnumMember(Value = "CompensationRejectedBySla")]
    [Display(Name = "Истёк срок решения")]
    CompensationRejectedBySla,

    [EnumMember(Value = "CompensationRejectedBySeller")]
    [Display(Name = "Продавец отказался от компенсации")]
    CompensationRejectedBySeller,

    [EnumMember(Value = "MovingToOzon")]
    [Display(Name = "Едет на склад Ozon")]
    MovingToOzon,

    [EnumMember(Value = "ReturnedToOzon")]
    [Display(Name = "На складе Ozon")]
    ReturnedToOzon,

    [EnumMember(Value = "MoneyReturnedBySystem")]
    [Display(Name = "Быстрый возврат")]
    MoneyReturnedBySystem,

    [EnumMember(Value = "WaitingShipment")]
    [Display(Name = "Ожидает отправки")]
    WaitingShipment,

    #region yandex market statuses

    [EnumMember(Value = "CREATED")]
    [JsonProperty("CREATED")]
    [Display(Name = "Возврат создан")]
    Created,

    [EnumMember(Value = "RECEIVED")]
    [JsonProperty("RECEIVED")]
    [Display(Name = "Возврат принят у отправителя")]
    Received,

    [EnumMember(Value = "IN_TRANSIT")]
    [JsonProperty("IN_TRANSIT")]
    [Display(Name = "На пути к продавцу")]
    InTransit,

    [EnumMember(Value = "READY_FOR_PICKUP")]
    [JsonProperty("READY_FOR_PICKUP")]
    [Display(Name = "Возврат готов к выдаче магазину")]
    ReadyForPickup,

    [EnumMember(Value = "PICKED")]
    [JsonProperty("PICKED")]
    [Display(Name = "Возврат выдан магазину")]
    Picked,

    [EnumMember(Value = "RECEIVED_ON_FULFILLMENT")]
    [JsonProperty("RECEIVED_ON_FULFILLMENT")]
    [Display(Name = "Возврат принят на складе Маркета")]
    ReceivedOnFulfillment,

    [EnumMember(Value = "CANCELLED")]

    [JsonProperty("CANCELLED")]
    [Display(Name = "Возврат отменен")]
    YMCancelled,

    [EnumMember(Value = "LOST")]

    [JsonProperty("LOST")]
    [Display(Name = "Возврат утерян")]
    Lost,

    [EnumMember(Value = "UTILIZED")]

    [JsonProperty("UTILIZED")]
    [Display(Name = "Возврат утилизирован")]
    YMUtilized,

    [EnumMember(Value = "PREPARED_FOR_UTILIZATION")]
    [JsonProperty("PREPARED_FOR_UTILIZATION")]
    [Display(Name = "Возврат готов к утилизации")]
    PreparedForUtilization,

    [EnumMember(Value = "EXPROPRIATED")]
    [JsonProperty("EXPROPRIATED")]
    [Display(Name = "Товары в возврате направлены на перепродажу")]
    Expropriated,

    [EnumMember(Value = "NOT_IN_DEMAND")]
    [JsonProperty("NOT_IN_DEMAND")]
    [Display(Name = "Возврат не забрали с почты")]
    NotInDemand,

    #endregion

    [EnumMember(Value = "Unknown")]
    [Display(Name = "Неизвестен")]
    Unknown = 0
  }
}