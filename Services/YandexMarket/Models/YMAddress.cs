using Newtonsoft.Json;

using System.Text.Json.Serialization;

namespace automation.mbtdistr.ru.Services.YandexMarket.Models
{
  /// <summary>
  /// Адрес.
  /// </summary>
  public class YMAddress
  {
    /// <summary>
    /// Страна.
    /// </summary>
    [JsonPropertyName("country")]
    [JsonProperty("country")]
    public string Country { get; set; } = string.Empty;

    /// <summary>
    /// Город.
    /// </summary>
    [JsonPropertyName("city")]
    [JsonProperty("city")]
    public string City { get; set; } = string.Empty;

    /// <summary>
    /// Улица.
    /// </summary>
    [JsonPropertyName("street")]
    [JsonProperty("street")]
    public string Street { get; set; } = string.Empty;

    /// <summary>
    /// Дом.
    /// </summary>
    [JsonPropertyName("house")]
    [JsonProperty("house")]
    public string House { get; set; } = string.Empty;

    /// <summary>
    /// Почтовый индекс.
    /// </summary>
    [JsonPropertyName("postcode")]
    [JsonProperty("postcode")]
    public string Postcode { get; set; } = string.Empty;
  }

}
