using System.Text.Json.Serialization;

namespace automation.mbtdistr.ru.Services.Ozon.Models
{
  #region Response DTOs

  /// <summary>
  /// Обёртка для тела POST-запроса /v1/returns/list
  /// </summary>
  public class ReturnsListRequest
  {
    [JsonPropertyName("filter")]
    public Filter Filter { get; set; }

    [JsonPropertyName("limit")]
    public int Limit { get; set; }

    [JsonPropertyName("last_id")]
    public long? LastId { get; set; }
  }

  #endregion
}
