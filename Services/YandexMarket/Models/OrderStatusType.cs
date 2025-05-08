using Newtonsoft.Json;

using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace automation.mbtdistr.ru.Services.YandexMarket.Models
{
  /// <summary>
  /// Статус заказа.
  /// </summary>
  public enum OrderStatusType
  {
    /// <summary>
    /// Оформляется, подготовка к резервированию.
    /// </summary>
    [EnumMember(Value = "PLACING")]
    [JsonPropertyName("PLACING")]
    [JsonProperty("PLACING")]
    [Display(Name = "Оформляется")]
    Placing,

    /// <summary>
    /// Зарезервирован, но недооформлен.
    /// </summary>
    [EnumMember(Value = "RESERVED")]
    [JsonPropertyName("RESERVED")]
    [JsonProperty("RESERVED")]
    [Display(Name = "Зарезервирован")]
    Reserved,

    /// <summary>
    /// Оформлен, но еще не оплачен (если выбрана оплата при оформлении).
    /// </summary>
    [EnumMember(Value = "UNPAID")]
    [JsonPropertyName("UNPAID")]
    [JsonProperty("UNPAID")]
    [Display(Name = "Не оплачен")]
    Unpaid,

    /// <summary>
    /// Находится в обработке.
    /// </summary>
    [EnumMember(Value = "PROCESSING")]
    [JsonPropertyName("PROCESSING")]
    [JsonProperty("PROCESSING")]
    [Display(Name = "В обработке")]
    Processing,

    /// <summary>
    /// Передан в службу доставки.
    /// </summary>
    [EnumMember(Value = "DELIVERY")]
    [JsonPropertyName("DELIVERY")]
    [JsonProperty("DELIVERY")]
    [Display(Name = "Передан в доставку")]
    Delivery,

    /// <summary>
    /// Доставлен в пункт самовывоза.
    /// </summary>
    [EnumMember(Value = "PICKUP")]
    [JsonPropertyName("PICKUP")]
    [JsonProperty("PICKUP")]
    [Display(Name = "В пункте самовывоза")]
    Pickup,

    /// <summary>
    /// Получен покупателем.
    /// </summary>
    [EnumMember(Value = "DELIVERED")]
    [JsonPropertyName("DELIVERED")]
    [JsonProperty("DELIVERED")]
    [Display(Name = "Доставлен")]
    Delivered,

    /// <summary>
    /// Отменен.
    /// </summary>
    [EnumMember(Value = "CANCELLED")]
    [JsonPropertyName("CANCELLED")]
    [JsonProperty("CANCELLED")]
    [Display(Name = "Отменен")]
    Cancelled,

    /// <summary>
    /// Ожидает обработки со стороны продавца.
    /// </summary>
    [EnumMember(Value = "PENDING")]
    [JsonPropertyName("PENDING")]
    [JsonProperty("PENDING")]
    [Display(Name = "Ожидает обработки")]
    Pending,

    /// <summary>
    /// Возвращен частично.
    /// </summary>
    [EnumMember(Value = "PARTIALLY_RETURNED")]
    [JsonPropertyName("PARTIALLY_RETURNED")]
    [JsonProperty("PARTIALLY_RETURNED")]
    [Display(Name = "Частично возвращен")]
    PartiallyReturned,

    /// <summary>
    /// Возвращен полностью.
    /// </summary>
    [EnumMember(Value = "RETURNED")]
    [JsonPropertyName("RETURNED")]
    [JsonProperty("RETURNED")]
    [Display(Name = "Возвращен")]
    Returned,

    /// <summary>
    /// Неизвестный статус.
    /// </summary>
    [EnumMember(Value = "UNKNOWN")]
    [JsonPropertyName("UNKNOWN")]
    [JsonProperty("UNKNOWN")]
    [Display(Name = "Неизвестный статус")]
    Unknown
  }
}

