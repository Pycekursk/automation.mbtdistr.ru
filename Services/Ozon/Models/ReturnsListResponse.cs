using System.Text.Json.Serialization;

namespace automation.mbtdistr.ru.Services.Ozon.Models
{
  /// <summary>
  /// Модель ответа от API с коллекцией возвратов и флагом постраничности.
  /// </summary>
  public class ReturnsListResponse
  {
    [JsonPropertyName("returns")]
    public List<ReturnInfo> Returns { get; set; }

    [JsonPropertyName("has_next")]
    public bool HasNext { get; set; }
  }
}
