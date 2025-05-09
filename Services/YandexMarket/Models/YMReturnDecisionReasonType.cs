using Newtonsoft.Json;

using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

using System.Text.Json.Serialization;

/// <summary>
/// Причины возврата.
/// </summary>
public enum YMReturnDecisionReasonType
{
  /// <summary>
  /// Бракованный товар (есть недостатки).
  /// </summary>
  [EnumMember(Value = "BAD_QUALITY")]
  [JsonPropertyName("BAD_QUALITY")]
  [JsonProperty("BAD_QUALITY")]
  [Display(Name = "Бракованный товар")]
  BadQuality,

  /// <summary>
  /// Товар не подошел.
  /// </summary>
  [EnumMember(Value = "DOES_NOT_FIT")]
  [JsonPropertyName("DOES_NOT_FIT")]
  [JsonProperty("DOES_NOT_FIT")]
  [Display(Name = "Товар не подошел")]
  DoesNotFit,

  /// <summary>
  /// Привезли не тот товар.
  /// </summary>
  [EnumMember(Value = "WRONG_ITEM")]
  [JsonPropertyName("WRONG_ITEM")]
  [JsonProperty("WRONG_ITEM")]
  [Display(Name = "Привезли не тот товар")]
  WrongItem,

  /// <summary>
  /// Товар поврежден при доставке.
  /// </summary>
  [EnumMember(Value = "DAMAGE_DELIVERY")]
  [JsonPropertyName("DAMAGE_DELIVERY")]
  [JsonProperty("DAMAGE_DELIVERY")]
  [Display(Name = "Товар поврежден при доставке")]
  DamageDelivery,

  /// <summary>
  /// Невозможно установить виновного в браке/пересорте.
  /// </summary>
  [EnumMember(Value = "LOYALTY_FAIL")]
  [JsonPropertyName("LOYALTY_FAIL")]
  [JsonProperty("LOYALTY_FAIL")]
  [Display(Name = "Невозможно установить виновного в браке/пересорте")]
  LoyaltyFail,

  /// <summary>
  /// Ошибочное описание товара по вине Маркета.
  /// </summary>
  [EnumMember(Value = "CONTENT_FAIL")]
  [JsonPropertyName("CONTENT_FAIL")]
  [JsonProperty("CONTENT_FAIL")]
  [Display(Name = "Ошибочное описание товара по вине Маркета")]
  ContentFail,

  /// <summary>
  /// Товар не привезли.
  /// </summary>
  [EnumMember(Value = "DELIVERY_FAIL")]
  [JsonPropertyName("DELIVERY_FAIL")]
  [JsonProperty("DELIVERY_FAIL")]
  [Display(Name = "Товар не привезли")]
  DeliveryFail,

  /// <summary>
  /// Причина не известна.
  /// </summary>
  [EnumMember(Value = "UNKNOWN")]
  [JsonPropertyName("UNKNOWN")]
  [JsonProperty("UNKNOWN")]
  [Display(Name = "Причина не известна")]
  Unknown = 0
}
