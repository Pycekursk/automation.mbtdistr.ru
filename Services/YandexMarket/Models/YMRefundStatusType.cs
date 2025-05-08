using Newtonsoft.Json;

using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

using System.Text.Json.Serialization;

/// <summary>
/// Статус возврата денег.
/// </summary>
public enum YMRefundStatusType
{
  [EnumMember(Value = "STARTED_BY_USER")]
  [JsonPropertyName("STARTED_BY_USER")]
  [JsonProperty("STARTED_BY_USER")]
  [Display(Name = "Создан клиентом")]
  StartedByUser,

  [EnumMember(Value = "REFUND_IN_PROGRESS")]
  [JsonPropertyName("REFUND_IN_PROGRESS")]
  [JsonProperty("REFUND_IN_PROGRESS")]
  [Display(Name = "Ждет решения")]
  RefundInProgress,

  [EnumMember(Value = "REFUNDED")]
  [JsonPropertyName("REFUNDED")]
  [JsonProperty("REFUNDED")]
  [Display(Name = "Возврат завершен")]
  Refunded,

  [EnumMember(Value = "FAILED")]
  [JsonPropertyName("FAILED")]
  [JsonProperty("FAILED")]
  [Display(Name = "Возврат невозможен")]
  Failed,

  [EnumMember(Value = "WAITING_FOR_DECISION")]
  [JsonPropertyName("WAITING_FOR_DECISION")]
  [JsonProperty("WAITING_FOR_DECISION")]
  [Display(Name = "Ожидает решения")]
  WaitingForDecision,

  [EnumMember(Value = "DECISION_MADE")]
  [JsonPropertyName("DECISION_MADE")]
  [JsonProperty("DECISION_MADE")]
  [Display(Name = "Решение принято")]
  DecisionMade,

  [EnumMember(Value = "REFUNDED_WITH_BONUSES")]
  [JsonPropertyName("REFUNDED_WITH_BONUSES")]
  [JsonProperty("REFUNDED_WITH_BONUSES")]
  [Display(Name = "Возврат баллами или промокодом")]
  RefundedWithBonuses,

  [EnumMember(Value = "REFUNDED_BY_SHOP")]
  [JsonPropertyName("REFUNDED_BY_SHOP")]
  [JsonProperty("REFUNDED_BY_SHOP")]
  [Display(Name = "Возврат магазином")]
  RefundedByShop,

  [EnumMember(Value = "COMPLETE_WITHOUT_REFUND")]
  [JsonPropertyName("COMPLETE_WITHOUT_REFUND")]
  [JsonProperty("COMPLETE_WITHOUT_REFUND")]
  [Display(Name = "Возврат не требуется")]
  CompleteWithoutRefund,

  [EnumMember(Value = "CANCELLED")]
  [JsonPropertyName("CANCELLED")]
  [JsonProperty("CANCELLED")]
  [Display(Name = "Возврат отменен")]
  Cancelled,

  [EnumMember(Value = "UNKNOWN")]
  [JsonPropertyName("UNKNOWN")]
  [JsonProperty("UNKNOWN")]
  [Display(Name = "Неизвестный статус")]
  Unknown
}
