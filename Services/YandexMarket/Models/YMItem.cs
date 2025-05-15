using Newtonsoft.Json;

using System.Text.Json.Serialization;

namespace automation.mbtdistr.ru.Services.YandexMarket.Models
{
  /// <summary>
  /// Товар в возврате.
  /// </summary>
  public class YMItem
  {
    [JsonPropertyName("decisions")]
    [JsonProperty("decisions")]
    public List<YMDecision>? Decisions { get; set; }

    /// <summary>
    /// Маркет SKU.
    /// </summary>
    [JsonPropertyName("marketSku")]
    [JsonProperty("marketSku")]
    public long MarketSku { get; set; }

    /// <summary>
    /// Магазинный SKU.
    /// </summary>
    [JsonPropertyName("shopSku")]
    [JsonProperty("shopSku")]
    public string ShopSku { get; set; } = string.Empty;

    /// <summary>
    /// Количество.
    /// </summary>
    [JsonPropertyName("count")]
    [JsonProperty("count")]
    public int Count { get; set; }

    /// <summary>
    /// Экземпляры товара.
    /// </summary>
    [JsonPropertyName("instances")]
    [JsonProperty("instances")]
    public List<Instance> Instances { get; set; } = new();

    /// <summary>
    /// Треки доставки.
    /// </summary>
    [JsonPropertyName("tracks")]
    [JsonProperty("tracks")]
    public List<Track> Tracks { get; set; } = new();
  }

}
