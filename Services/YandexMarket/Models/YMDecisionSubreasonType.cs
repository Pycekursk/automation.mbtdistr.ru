using Newtonsoft.Json;

using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

using System.Text.Json.Serialization;

/// <summary>
/// Детали причин возврата.
/// </summary>
public enum YMDecisionSubreasonType
{
  [EnumMember(Value = "USER_DID_NOT_LIKE")]
  [JsonPropertyName("USER_DID_NOT_LIKE")]
  [JsonProperty("USER_DID_NOT_LIKE")]
  [Display(Name = "Товар не понравился")]
  UserDidNotLike,

  [EnumMember(Value = "USER_CHANGED_MIND")]
  [JsonPropertyName("USER_CHANGED_MIND")]
  [JsonProperty("USER_CHANGED_MIND")]
  [Display(Name = "Передумал покупать")]
  UserChangedMind,

  [EnumMember(Value = "DELIVERED_TOO_LONG")]
  [JsonPropertyName("DELIVERED_TOO_LONG")]
  [JsonProperty("DELIVERED_TOO_LONG")]
  [Display(Name = "Длительный срок доставки")]
  DeliveredTooLong,

  [EnumMember(Value = "BAD_PACKAGE")]
  [JsonPropertyName("BAD_PACKAGE")]
  [JsonProperty("BAD_PACKAGE")]
  [Display(Name = "Повреждена заводская упаковка")]
  BadPackage,

  [EnumMember(Value = "DAMAGED")]
  [JsonPropertyName("DAMAGED")]
  [JsonProperty("DAMAGED")]
  [Display(Name = "Царапины, сколы")]
  Damaged,

  [EnumMember(Value = "NOT_WORKING")]
  [JsonPropertyName("NOT_WORKING")]
  [JsonProperty("NOT_WORKING")]
  [Display(Name = "Не работает")]
  NotWorking,

  [EnumMember(Value = "INCOMPLETENESS")]
  [JsonPropertyName("INCOMPLETENESS")]
  [JsonProperty("INCOMPLETENESS")]
  [Display(Name = "Некомплект")]
  Incompleteness,

  [EnumMember(Value = "WRAPPING_DAMAGED")]
  [JsonPropertyName("WRAPPING_DAMAGED")]
  [JsonProperty("WRAPPING_DAMAGED")]
  [Display(Name = "Повреждена транспортная упаковка")]
  WrappingDamaged,

  [EnumMember(Value = "ITEM_WAS_USED")]
  [JsonPropertyName("ITEM_WAS_USED")]
  [JsonProperty("ITEM_WAS_USED")]
  [Display(Name = "Следы использования")]
  ItemWasUsed,

  [EnumMember(Value = "BROKEN")]
  [JsonPropertyName("BROKEN")]
  [JsonProperty("BROKEN")]
  [Display(Name = "Товар разбит")]
  Broken,

  [EnumMember(Value = "BAD_FLOWERS")]
  [JsonPropertyName("BAD_FLOWERS")]
  [JsonProperty("BAD_FLOWERS")]
  [Display(Name = "Некачественные цветы")]
  BadFlowers,

  [EnumMember(Value = "WRONG_ITEM")]
  [JsonPropertyName("WRONG_ITEM")]
  [JsonProperty("WRONG_ITEM")]
  [Display(Name = "Не тот товар")]
  WrongItem,

  [EnumMember(Value = "WRONG_COLOR")]
  [JsonPropertyName("WRONG_COLOR")]
  [JsonProperty("WRONG_COLOR")]
  [Display(Name = "Неверный цвет")]
  WrongColor,

  [EnumMember(Value = "DID_NOT_MATCH_DESCRIPTION")]
  [JsonPropertyName("DID_NOT_MATCH_DESCRIPTION")]
  [JsonProperty("DID_NOT_MATCH_DESCRIPTION")]
  [Display(Name = "Несоответствие описанию")]
  DidNotMatchDescription,

  [EnumMember(Value = "WRONG_ORDER")]
  [JsonPropertyName("WRONG_ORDER")]
  [JsonProperty("WRONG_ORDER")]
  [Display(Name = "Чужой заказ")]
  WrongOrder,

  [EnumMember(Value = "WRONG_AMOUNT_DELIVERED")]
  [JsonPropertyName("WRONG_AMOUNT_DELIVERED")]
  [JsonProperty("WRONG_AMOUNT_DELIVERED")]
  [Display(Name = "Неверное количество")]
  WrongAmountDelivered,

  [EnumMember(Value = "PARCEL_MISSING")]
  [JsonPropertyName("PARCEL_MISSING")]
  [JsonProperty("PARCEL_MISSING")]
  [Display(Name = "Часть заказа отсутствует")]
  ParcelMissing,

  [EnumMember(Value = "INCOMPLETE")]
  [JsonPropertyName("INCOMPLETE")]
  [JsonProperty("INCOMPLETE")]
  [Display(Name = "Заказ не привезли полностью")]
  Incomplete,

  [EnumMember(Value = "UNKNOWN")]
  [JsonPropertyName("UNKNOWN")]
  [JsonProperty("UNKNOWN")]
  [Display(Name = "Детали причины не указаны")]
  Unknown = 0
}
