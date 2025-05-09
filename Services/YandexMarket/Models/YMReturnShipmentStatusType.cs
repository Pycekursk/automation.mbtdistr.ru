using Newtonsoft.Json;

using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

using System.Text.Json.Serialization;

/// <summary>
/// Статус передачи возврата.
/// </summary>
public enum YMReturnShipmentStatusType
{
  [EnumMember(Value = "CREATED")]
  [JsonPropertyName("CREATED")]
  [JsonProperty("CREATED")]
  [Display(Name = "Возврат создан")]
  Created,

  [EnumMember(Value = "RECEIVED")]
  [JsonPropertyName("RECEIVED")]
  [JsonProperty("RECEIVED")]
  [Display(Name = "Принят у покупателя")]
  Received,

  [EnumMember(Value = "IN_TRANSIT")]
  [JsonPropertyName("IN_TRANSIT")]
  [JsonProperty("IN_TRANSIT")]
  [Display(Name = "Возврат в пути")]
  InTransit,

  [EnumMember(Value = "READY_FOR_PICKUP")]
  [JsonPropertyName("READY_FOR_PICKUP")]
  [JsonProperty("READY_FOR_PICKUP")]
  [Display(Name = "Готов к выдаче магазину")]
  ReadyForPickup,

  [EnumMember(Value = "PICKED")]
  [JsonPropertyName("PICKED")]
  [JsonProperty("PICKED")]
  [Display(Name = "Выдан магазину")]
  Picked,

  [EnumMember(Value = "LOST")]
  [JsonPropertyName("LOST")]
  [JsonProperty("LOST")]
  [Display(Name = "Утерян при транспортировке")]
  Lost,

  [EnumMember(Value = "EXPIRED")]
  [JsonPropertyName("EXPIRED")]
  [JsonProperty("EXPIRED")]
  [Display(Name = "Не принесен вовремя")]
  Expired,

  [EnumMember(Value = "CANCELLED")]
  [JsonPropertyName("CANCELLED")]
  [JsonProperty("CANCELLED")]
  [Display(Name = "Отменен")]
  Cancelled,

  [EnumMember(Value = "FULFILMENT_RECEIVED")]
  [JsonPropertyName("FULFILMENT_RECEIVED")]
  [JsonProperty("FULFILMENT_RECEIVED")]
  [Display(Name = "Принят на складе Маркета")]
  FulfilmentReceived,

  [EnumMember(Value = "PREPARED_FOR_UTILIZATION")]
  [JsonPropertyName("PREPARED_FOR_UTILIZATION")]
  [JsonProperty("PREPARED_FOR_UTILIZATION")]
  [Display(Name = "Передан в утилизацию")]
  PreparedForUtilization,

  [EnumMember(Value = "NOT_IN_DEMAND")]
  [JsonPropertyName("NOT_IN_DEMAND")]
  [JsonProperty("NOT_IN_DEMAND")]
  [Display(Name = "Не забрали с почты")]
  NotInDemand,

  [EnumMember(Value = "UTILIZED")]
  [JsonPropertyName("UTILIZED")]
  [JsonProperty("UTILIZED")]
  [Display(Name = "Утилизирован")]
  Utilized,

  [EnumMember(Value = "READY_FOR_EXPROPRIATION")]
  [JsonPropertyName("READY_FOR_EXPROPRIATION")]
  [JsonProperty("READY_FOR_EXPROPRIATION")]
  [Display(Name = "Направлен на перепродажу")]
  ReadyForExpropriation,

  [EnumMember(Value = "RECEIVED_FOR_EXPROPRIATION")]
  [JsonPropertyName("RECEIVED_FOR_EXPROPRIATION")]
  [JsonProperty("RECEIVED_FOR_EXPROPRIATION")]
  [Display(Name = "Принят для перепродажи")]
  ReceivedForExpropriation,

  [EnumMember(Value = "UNKNOWN")]
  [JsonPropertyName("UNKNOWN")]
  [JsonProperty("UNKNOWN")]
  [Display(Name = "Неизвестный статус")]
  Unknown = 0
}
