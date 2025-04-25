using System.Text.Json.Serialization;

namespace automation.mbtdistr.ru.Services.Ozon.Models
{
  /// <summary>
  /// Детальная информация о конкретном возврате.
  /// </summary>
  public class ReturnInfo
  {
    [JsonPropertyName("exemplars")]
    public List<Exemplar>? Exemplars { get; set; }

    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("company_id")]
    public long CompanyId { get; set; }

    [JsonPropertyName("return_reason_name")]
    public string? ReturnReasonName { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("schema")]
    public string? Schema { get; set; }

    [JsonPropertyName("order_id")]
    public long OrderId { get; set; }

    [JsonPropertyName("order_number")]
    public string? OrderNumber { get; set; }

    [JsonPropertyName("place")]
    public Place? Place { get; set; }

    [JsonPropertyName("target_place")]
    public Place? TargetPlace { get; set; }

    [JsonPropertyName("storage")]
    public StorageInfo? Storage { get; set; }

    [JsonPropertyName("product")]
    public ProductInfo? Product { get; set; }

    [JsonPropertyName("logistic")]
    public LogisticInfo? Logistic { get; set; }

    [JsonPropertyName("visual")]
    public VisualInfo? Visual { get; set; }

    [JsonPropertyName("additional_info")]
    public AdditionalInfo? AdditionalInfo { get; set; }

    [JsonPropertyName("source_id")]
    public long SourceId { get; set; }

    [JsonPropertyName("posting_number")]
    public string? PostingNumber { get; set; }

    [JsonPropertyName("clearing_id")]
    public long? ClearingId { get; set; }

    [JsonPropertyName("return_clearing_id")]
    public long? ReturnClearingId { get; set; }
  }

  
}
