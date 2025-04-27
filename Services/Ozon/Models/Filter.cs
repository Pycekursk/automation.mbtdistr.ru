using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace automation.mbtdistr.ru.Services.Ozon.Models
{
  #region Response DTOs
  /// <summary>
  /// Параметры фильтрации возвратов (по датам, статусам, идентификаторам и т.д.).
  /// </summary>
  public class Filter
  {
    [JsonPropertyName("logistic_return_date"), Display(Name = "Дата возврата")]
    public DateRange LogisticReturnDate { get; set; }

    [JsonPropertyName("storage_tariffication_start_date"), Display(Name = "Дата начала тарификации")]
    public DateRange StorageTarifficationStartDate { get; set; }

    [JsonPropertyName("visual_status_change_moment"), Display(Name = "Дата изменения визуального статуса")]
    public DateRange VisualStatusChangeMoment { get; set; }

    [JsonPropertyName("order_id")]
    public long? OrderId { get; set; }

    [JsonPropertyName("posting_numbers")]
    public List<string>? PostingNumbers { get; set; }

    [JsonPropertyName("product_name")]
    public string? ProductName { get; set; }

    [JsonPropertyName("offer_id")]
    public string? OfferId { get; set; }

    [JsonPropertyName("visual_status_name")]
    public string? VisualStatusName { get; set; }

    [JsonPropertyName("warehouse_id")]
    public int? WarehouseId { get; set; }

    [JsonPropertyName("barcode")]
    public string? Barcode { get; set; }

    [JsonPropertyName("return_schema")]
    public string? ReturnSchema { get; set; }
  }

  #endregion
}
