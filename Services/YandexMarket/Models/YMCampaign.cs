using Newtonsoft.Json;

using System.Text.Json.Serialization;

namespace automation.mbtdistr.ru.Services.YandexMarket.Models
{
  /// <summary>
  /// Кампания.
  /// </summary>
  public class YMCampaign
  {
    /// <summary>
    /// Домен кампании.
    /// </summary>
    [JsonPropertyName("domain")]
    [JsonProperty("domain")]
    public string Domain { get; set; } = string.Empty;

    /// <summary>
    /// Идентификатор кампании.
    /// </summary>
    [JsonPropertyName("id")]
    [JsonProperty("id")]
    public long Id { get; set; }

    /// <summary>
    /// Идентификатор клиента.
    /// </summary>
    [JsonPropertyName("clientId")]
    [JsonProperty("clientId")]
    public long ClientId { get; set; }

    /// <summary>
    /// Информация о бизнесе.
    /// </summary>
    [JsonPropertyName("business")]
    [JsonProperty("business")]
    public YMBusiness Business { get; set; } = new();

    /// <summary>
    /// Тип размещения.
    /// </summary>
    [JsonPropertyName("placementType")]
    [JsonProperty("placementType")]
    public string PlacementType { get; set; } = string.Empty;
  }

}
