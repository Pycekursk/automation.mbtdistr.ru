using Newtonsoft.Json;

using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

using System.Text.Json.Serialization;

/// <summary>
/// Статус возврата денег.
/// </summary>
/// <summary>
/// Перечисление возможных статусов возврата средств:
/// 
/// STARTED_BY_USER — возврат инициирован пользователем из личного кабинета.
/// REFUND_IN_PROGRESS — возврат ожидает решения, обработка еще не завершена.
/// REFUNDED — возврат успешно завершен, все необходимые транзакции выполнены.
/// FAILED — возврат невозможен по техническим или иным причинам.
/// WAITING_FOR_DECISION — возврат на рассмотрении, решение еще не принято.
/// DECISION_MADE — решение по возврату принято, но транзакции еще не завершены.
/// REFUNDED_WITH_BONUSES — возврат выполнен в виде бонусов Плюса или промокода.
/// REFUNDED_BY_SHOP — возврат средств произведен напрямую магазином.
/// COMPLETE_WITHOUT_REFUND — возврат не требуется (например, проблема решена без возврата денег).
/// CANCELLED — процесс возврата отменен до завершения.
/// UNKNOWN — статус возврата не определен (например, при ошибке в интеграции или новых, нераспознанных значениях).
/// </summary>
public enum YMRefundStatusType
{
  /// <summary>
  /// Возврат инициирован пользователем из личного кабинета.
  /// </summary>
  [EnumMember(Value = "STARTED_BY_USER")]
  [JsonPropertyName("STARTED_BY_USER")]
  [JsonProperty("STARTED_BY_USER")]
  [Display(Name = "Создан клиентом")]
  StartedByUser,

  /// <summary>
  /// Возврат ожидает решения, обработка еще не завершена.
  /// </summary>
  [EnumMember(Value = "REFUND_IN_PROGRESS")]
  [JsonPropertyName("REFUND_IN_PROGRESS")]
  [JsonProperty("REFUND_IN_PROGRESS")]
  [Display(Name = "Ждет решения")]
  RefundInProgress,

  /// <summary>
  /// Возврат успешно завершен, все необходимые транзакции выполнены.
  /// </summary>
  [EnumMember(Value = "REFUNDED")]
  [JsonPropertyName("REFUNDED")]
  [JsonProperty("REFUNDED")]
  [Display(Name = "Возврат завершен")]
  Refunded,

  /// <summary>
  /// Возврат невозможен по техническим или иным причинам.
  /// </summary>
  [EnumMember(Value = "FAILED")]
  [JsonPropertyName("FAILED")]
  [JsonProperty("FAILED")]
  [Display(Name = "Возврат невозможен")]
  Failed,

  /// <summary>
  /// Возврат на рассмотрении, решение еще не принято.
  /// </summary>
  [EnumMember(Value = "WAITING_FOR_DECISION")]
  [JsonPropertyName("WAITING_FOR_DECISION")]
  [JsonProperty("WAITING_FOR_DECISION")]
  [Display(Name = "Ожидает решения")]
  WaitingForDecision,

  /// <summary>
  /// Решение по возврату принято, но транзакции еще не завершены.
  /// </summary>
  [EnumMember(Value = "DECISION_MADE")]
  [JsonPropertyName("DECISION_MADE")]
  [JsonProperty("DECISION_MADE")]
  [Display(Name = "Решение принято")]
  DecisionMade,

  /// <summary>
  /// Возврат выполнен в виде бонусов Плюса или промокода.
  /// </summary>
  [EnumMember(Value = "REFUNDED_WITH_BONUSES")]
  [JsonPropertyName("REFUNDED_WITH_BONUSES")]
  [JsonProperty("REFUNDED_WITH_BONUSES")]
  [Display(Name = "Возврат баллами или промокодом")]
  RefundedWithBonuses,

  /// <summary>
  /// Возврат средств произведен напрямую магазином.
  /// </summary>
  [EnumMember(Value = "REFUNDED_BY_SHOP")]
  [JsonPropertyName("REFUNDED_BY_SHOP")]
  [JsonProperty("REFUNDED_BY_SHOP")]
  [Display(Name = "Возврат магазином")]
  RefundedByShop,

  /// <summary>
  /// Возврат не требуется (например, проблема решена без возврата денег).
  /// </summary>
  [EnumMember(Value = "COMPLETE_WITHOUT_REFUND")]
  [JsonPropertyName("COMPLETE_WITHOUT_REFUND")]
  [JsonProperty("COMPLETE_WITHOUT_REFUND")]
  [Display(Name = "Возврат не требуется")]
  CompleteWithoutRefund,

  /// <summary>
  /// Процесс возврата отменен до завершения.
  /// </summary>
  [EnumMember(Value = "CANCELLED")]
  [JsonPropertyName("CANCELLED")]
  [JsonProperty("CANCELLED")]
  [Display(Name = "Возврат отменен")]
  Cancelled,

  /// <summary>
  /// Статус возврата не определен (например, при ошибке в интеграции или новых, нераспознанных значениях).
  /// </summary>
  [EnumMember(Value = "UNKNOWN")]
  [JsonPropertyName("UNKNOWN")]
  [JsonProperty("UNKNOWN")]
  [Display(Name = "Неизвестный статус")]
  Unknown = 0
}
