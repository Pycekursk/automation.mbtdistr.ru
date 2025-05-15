using Newtonsoft.Json;

using System.Text.Json.Serialization;

namespace automation.mbtdistr.ru.Services.YandexMarket.Models
{
  /// <summary>
  /// Бизнес.
  /// </summary>
  public class YMBusiness
  {
    /// <summary>
    /// Идентификатор бизнеса.
    /// </summary>
    [JsonPropertyName("id")]
    [JsonProperty("id")]
    public long Id { get; set; }

    /// <summary>
    /// Название бизнеса.
    /// </summary>
    [JsonPropertyName("name")]
    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;
  }

}
