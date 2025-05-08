using Newtonsoft.Json;

using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace automation.mbtdistr.ru.Services.YandexMarket.Models
{
  /// <summary>
  /// Этап обработки заказа (если он имеет статус PROCESSING) или причина отмены заказа (если он имеет статус CANCELLED).
  /// </summary>
  public enum OrderSubstatusType
  {
    /// <summary>
    /// Заказ подтвержден, его можно начать обрабатывать.
    /// </summary>
    [EnumMember(Value = "STARTED")]
    [JsonPropertyName("STARTED")]
    [JsonProperty("STARTED")]
    [Display(Name = "Начат")]
    Started,

    /// <summary>
    /// Заказ собран и готов к отправке.
    /// </summary>
    [EnumMember(Value = "READY_TO_SHIP")]
    [JsonPropertyName("READY_TO_SHIP")]
    [JsonProperty("READY_TO_SHIP")]
    [Display(Name = "Готов к отправке")]
    ReadyToShip,

    /// <summary>
    /// Покупатель не завершил оформление зарезервированного заказа в течение 10 минут.
    /// </summary>
    [EnumMember(Value = "RESERVATION_EXPIRED")]
    [JsonPropertyName("RESERVATION_EXPIRED")]
    [JsonProperty("RESERVATION_EXPIRED")]
    [Display(Name = "Резерв истек")]
    ReservationExpired,

    /// <summary>
    /// Покупатель не оплатил заказ (для типа оплаты PREPAID) в течение 30 минут.
    /// </summary>
    [EnumMember(Value = "USER_NOT_PAID")]
    [JsonPropertyName("USER_NOT_PAID")]
    [JsonProperty("USER_NOT_PAID")]
    [Display(Name = "Не оплачен")]
    UserNotPaid,

    /// <summary>
    /// Не удалось связаться с покупателем.
    /// </summary>
    [EnumMember(Value = "USER_UNREACHABLE")]
    [JsonPropertyName("USER_UNREACHABLE")]
    [JsonProperty("USER_UNREACHABLE")]
    [Display(Name = "Покупатель недоступен")]
    UserUnreachable,

    /// <summary>
    /// Покупатель отменил заказ по личным причинам.
    /// </summary>
    [EnumMember(Value = "USER_CHANGED_MIND")]
    [JsonPropertyName("USER_CHANGED_MIND")]
    [JsonProperty("USER_CHANGED_MIND")]
    [Display(Name = "Покупатель передумал")]
    UserChangedMind,

    /// <summary>
    /// Покупателя не устроили условия доставки.
    /// </summary>
    [EnumMember(Value = "USER_REFUSED_DELIVERY")]
    [JsonPropertyName("USER_REFUSED_DELIVERY")]
    [JsonProperty("USER_REFUSED_DELIVERY")]
    [Display(Name = "Отказ от доставки")]
    UserRefusedDelivery,

    /// <summary>
    /// Покупателю не подошел товар.
    /// </summary>
    [EnumMember(Value = "USER_REFUSED_PRODUCT")]
    [JsonPropertyName("USER_REFUSED_PRODUCT")]
    [JsonProperty("USER_REFUSED_PRODUCT")]
    [Display(Name = "Отказ от товара")]
    UserRefusedProduct,

    /// <summary>
    /// Магазин не может выполнить заказ.
    /// </summary>
    [EnumMember(Value = "SHOP_FAILED")]
    [JsonPropertyName("SHOP_FAILED")]
    [JsonProperty("SHOP_FAILED")]
    [Display(Name = "Ошибка магазина")]
    ShopFailed,

    /// <summary>
    /// Покупателя не устроило качество товара.
    /// </summary>
    [EnumMember(Value = "USER_REFUSED_QUALITY")]
    [JsonPropertyName("USER_REFUSED_QUALITY")]
    [JsonProperty("USER_REFUSED_QUALITY")]
    [Display(Name = "Отказ из-за качества")]
    UserRefusedQuality,

    /// <summary>
    /// Покупатель решил заменить товар другим по собственной инициативе.
    /// </summary>
    [EnumMember(Value = "REPLACING_ORDER")]
    [JsonPropertyName("REPLACING_ORDER")]
    [JsonProperty("REPLACING_ORDER")]
    [Display(Name = "Замена заказа")]
    ReplacingOrder,

    /// <summary>
    /// Значение более не используется.
    /// </summary>
    [EnumMember(Value = "PROCESSING_EXPIRED")]
    [JsonPropertyName("PROCESSING_EXPIRED")]
    [JsonProperty("PROCESSING_EXPIRED")]
    [Display(Name = "Истек срок обработки")]
    ProcessingExpired,

    /// <summary>
    /// Закончился срок хранения заказа в ПВЗ.
    /// </summary>
    [EnumMember(Value = "PICKUP_EXPIRED")]
    [JsonPropertyName("PICKUP_EXPIRED")]
    [JsonProperty("PICKUP_EXPIRED")]
    [Display(Name = "Истек срок хранения")]
    PickupExpired,

    /// <summary>
    /// Заказ переносили слишком много раз.
    /// </summary>
    [EnumMember(Value = "TOO_MANY_DELIVERY_DATE_CHANGES")]
    [JsonPropertyName("TOO_MANY_DELIVERY_DATE_CHANGES")]
    [JsonProperty("TOO_MANY_DELIVERY_DATE_CHANGES")]
    [Display(Name = "Слишком много переносов")]
    TooManyDeliveryDateChanges,

    /// <summary>
    /// Заказ доставляется слишком долго.
    /// </summary>
    [EnumMember(Value = "TOO_LONG_DELIVERY")]
    [JsonPropertyName("TOO_LONG_DELIVERY")]
    [JsonProperty("TOO_LONG_DELIVERY")]
    [Display(Name = "Слишком долгая доставка")]
    TooLongDelivery,

    /// <summary>
    /// Техническая ошибка на стороне Маркета.
    /// </summary>
    [EnumMember(Value = "TECHNICAL_ERROR")]
    [JsonPropertyName("TECHNICAL_ERROR")]
    [JsonProperty("TECHNICAL_ERROR")]
    [Display(Name = "Техническая ошибка")]
    TechnicalError
  }
}

