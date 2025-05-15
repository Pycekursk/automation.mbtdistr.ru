using Newtonsoft.Json;

using System.Text.Json.Serialization;

namespace automation.mbtdistr.ru.Services.YandexMarket.Models
{
  /// <summary>
  /// Данные пагинации.
  /// </summary>
  public class YMPaging
  {
    /// <summary>
    /// Общее количество элементов.
    /// </summary>
    [JsonPropertyName("total")]
    [JsonProperty("total")]
    public int Total { get; set; }

    /// <summary>
    /// Индекс начала выборки.
    /// </summary>
    [JsonPropertyName("from")]
    [JsonProperty("from")]
    public int From { get; set; }

    /// <summary>
    /// Индекс конца выборки.
    /// </summary>
    [JsonPropertyName("to")]
    [JsonProperty("to")]
    public int To { get; set; }

    /// <summary>
    /// Идентификатор следующей страницы
    /// </summary>
    [JsonPropertyName("nextPageToken")]
    [JsonProperty("nextPageToken")]
    public string? NextPageToken { get; set; }
  }

}
