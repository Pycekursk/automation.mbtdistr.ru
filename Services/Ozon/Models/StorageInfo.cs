using System.Text.Json.Serialization;

namespace automation.mbtdistr.ru.Services.Ozon.Models
{
  /// <summary>
  /// Информация о хранении возврата: суммы, даты и прогнозы использования склада.
  /// </summary>
  public class StorageInfo
  {
    [JsonPropertyName("sum")]
    public Money Sum { get; set; }

    [JsonPropertyName("tariffication_first_date")]
    public DateTime? TarifficationFirstDate { get; set; }

    [JsonPropertyName("tariffication_start_date")]
    public DateTime? TarifficationStartDate { get; set; }

    [JsonPropertyName("arrived_moment")]
    public DateTime? ArrivedMoment { get; set; }

    [JsonPropertyName("days")]
    public int Days { get; set; }

    [JsonPropertyName("utilization_sum")]
    public Money UtilizationSum { get; set; }

    [JsonPropertyName("utilization_forecast_date")]
    public DateTime? UtilizationForecastDate { get; set; }
  }

  
}
