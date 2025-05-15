using Newtonsoft.Json;

using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace automation.mbtdistr.ru.Services.YandexMarket.Models
{

  /// <summary>
  /// Экземпляр товара.
  /// </summary>
  public class Instance
  {
    /// <summary>
    /// Статус экземпляра.
    /// </summary>
    [JsonPropertyName("status")]
    [JsonProperty("status")]
    public string Status { get; set; } = string.Empty;
  }

  /// <summary>
  /// Трек доставки.
  /// </summary>
  public class Track
  {
    /// <summary>
    /// Трек-код.
    /// </summary>
    [JsonPropertyName("trackCode")]
    [JsonProperty("trackCode")]
    public string TrackCode { get; set; } = string.Empty;
  }

  /// <summary>
  /// Запрос на получение списка возвратов.
  /// </summary>
  public class ReturnsListRequest
  {
    /// <summary>
    /// Фильтр запроса.
    /// </summary>
    [JsonPropertyName("filter")]
    [JsonProperty("filter")]
    public YMFilter Filter { get; set; } = new();

    /// <summary>
    /// Ограничение количества результатов.
    /// </summary>
    [JsonPropertyName("limit")]
    [JsonProperty("limit")]
    public int Limit { get; set; }

    /// <summary>
    /// Последний идентификатор возврата (для пагинации).
    /// </summary>
    [JsonPropertyName("lastId")]
    [JsonProperty("lastId")]
    public long? LastId { get; set; }
  }

}
