using automation.mbtdistr.ru.Models;

using Newtonsoft.Json;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace automation.mbtdistr.ru.Services.YandexMarket.Models
{
  /// <summary>
  /// Возврат товара.
  /// </summary>
  public class YMReturn
  {
    /// <summary>
    /// Уникальный идентификатор возврата.
    /// </summary>
    [JsonPropertyName("id")]
    [JsonProperty("id")]
    public long Id { get; set; }

    /// <summary>
    /// Идентификатор заказа, связанного с возвратом.
    /// </summary>
    [JsonPropertyName("orderId")]
    [JsonProperty("orderId")]
    public long OrderId { get; set; }

    /// <summary>
    /// Дата создания возврата.
    /// </summary>
    [JsonPropertyName("creationDate")]
    [JsonProperty("creationDate")]
    public DateTime CreationDate { get; set; }

    /// <summary>
    /// Дата последнего обновления возврата.
    /// </summary>
    [JsonPropertyName("updateDate")]
    [JsonProperty("updateDate")]
    public DateTime UpdateDate { get; set; }

    /// <summary>
    /// Логистический пункт выдачи, связанный с возвратом.
    /// </summary>
    [JsonPropertyName("logisticPickupPoint")]
    [JsonProperty("logisticPickupPoint")]
    public YMLogisticPickupPoint? LogisticPickupPoint { get; set; }

    /// <summary>
    /// Тип получателя отправления.
    /// </summary>
    [JsonPropertyName("shipmentRecipientType")]
    [JsonProperty("shipmentRecipientType")]
    public YMShipmentRecipientType? ShipmentRecipientType { get; set; }

    /// <summary>
    /// Тип возврата.
    /// </summary>
    [JsonPropertyName("returnType")]
    [JsonProperty("returnType")]
    [Display(Name = "Тип возврата")]
    public ReturnType ReturnType { get; set; } = ReturnType.Unknown;

    /// <summary>
    /// Статус отправления.
    /// </summary>
    [JsonPropertyName("shipmentStatus")]
    [JsonProperty("shipmentStatus")]
    public YMReturnShipmentStatusType? ShipmentStatus { get; set; }

    /// <summary>
    /// Статус возврата денег
    /// </summary>
    [JsonPropertyName("refundStatus")]
    [JsonProperty("refundStatus")]
    public YMRefundStatus? RefundStatus { get; set; }

    /// <summary>
    /// Сумма и валюта возврата.
    /// </summary>
    [JsonPropertyName("amount")]
    [JsonProperty("amount")]
    public Amount? Amount { get; set; }

    /// <summary>
    /// Список товаров в возврате.
    /// </summary>
    [JsonPropertyName("items")]
    [JsonProperty("items")]
    public List<YMItem> Items { get; set; } = new();

    [JsonPropertyName("order")]
    [JsonProperty("order")]
    [NotMapped]
    public YMOrder? Order { get; set; }
  }
}
