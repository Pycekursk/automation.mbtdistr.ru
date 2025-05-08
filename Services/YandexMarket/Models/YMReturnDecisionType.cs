using Newtonsoft.Json;

using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

using System.Text.Json.Serialization;

/// <summary>
/// Решение по возврату.
/// </summary>
public enum YMReturnDecisionType
{
  /// <summary>
  /// Вернуть деньги покупателю.
  /// </summary>
  [EnumMember(Value = "REFUND_MONEY")]
  [JsonPropertyName("REFUND_MONEY")]
  [JsonProperty("REFUND_MONEY")]
  [Display(Name = "Возврат денег")]
  RefundMoney,

  /// <summary>
  /// Вернуть деньги за товар и пересылку.
  /// </summary>
  [EnumMember(Value = "REFUND_MONEY_INCLUDING_SHIPMENT")]
  [JsonPropertyName("REFUND_MONEY_INCLUDING_SHIPMENT")]
  [JsonProperty("REFUND_MONEY_INCLUDING_SHIPMENT")]
  [Display(Name = "Возврат денег вместе с пересылкой")]
  RefundMoneyIncludingShipment,

  /// <summary>
  /// Отремонтировать товар.
  /// </summary>
  [EnumMember(Value = "REPAIR")]
  [JsonPropertyName("REPAIR")]
  [JsonProperty("REPAIR")]
  [Display(Name = "Отремонтировать товар")]
  Repair,

  /// <summary>
  /// Заменить товар.
  /// </summary>
  [EnumMember(Value = "REPLACE")]
  [JsonPropertyName("REPLACE")]
  [JsonProperty("REPLACE")]
  [Display(Name = "Заменить товар")]
  Replace,

  /// <summary>
  /// Взять товар на экспертизу.
  /// </summary>
  [EnumMember(Value = "SEND_TO_EXAMINATION")]
  [JsonPropertyName("SEND_TO_EXAMINATION")]
  [JsonProperty("SEND_TO_EXAMINATION")]
  [Display(Name = "Взять товар на экспертизу")]
  SendToExamination,

  /// <summary>
  /// Отказать в возврате.
  /// </summary>
  [EnumMember(Value = "DECLINE_REFUND")]
  [JsonPropertyName("DECLINE_REFUND")]
  [JsonProperty("DECLINE_REFUND")]
  [Display(Name = "Отказать в возврате")]
  DeclineRefund,

  /// <summary>
  /// Другое решение.
  /// </summary>
  [EnumMember(Value = "OTHER_DECISION")]
  [JsonPropertyName("OTHER_DECISION")]
  [JsonProperty("OTHER_DECISION")]
  [Display(Name = "Другое решение")]
  OtherDecision,

  /// <summary>
  /// Не указано.
  /// </summary>
  [EnumMember(Value = "UNKNOWN")]
  [JsonPropertyName("UNKNOWN")]
  [JsonProperty("UNKNOWN")]
  [Display(Name = "Не указано")]
  Unknown
}
