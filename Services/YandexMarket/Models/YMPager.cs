using Newtonsoft.Json;

using System.Text.Json.Serialization;

namespace automation.mbtdistr.ru.Services.YandexMarket.Models
{
  /// <summary>
  /// Информация о страницах (пагинация).
  /// </summary>
  public class YMPager
  {
    /// <summary>
    /// Общее количество записей.
    /// </summary>
    [JsonPropertyName("total")]
    [JsonProperty("total")]
    public int Total { get; set; }

    /// <summary>
    /// Индекс начала текущей выборки.
    /// </summary>
    [JsonPropertyName("from")]
    [JsonProperty("from")]
    public int From { get; set; }

    /// <summary>
    /// Индекс конца текущей выборки.
    /// </summary>
    [JsonPropertyName("to")]
    [JsonProperty("to")]
    public int To { get; set; }

    /// <summary>
    /// Номер текущей страницы.
    /// </summary>
    [JsonPropertyName("currentPage")]
    [JsonProperty("currentPage")]
    public int CurrentPage { get; set; }

    /// <summary>
    /// Общее количество страниц.
    /// </summary>
    [JsonPropertyName("pagesCount")]
    [JsonProperty("pagesCount")]
    public int PagesCount { get; set; }

    /// <summary>
    /// Размер страницы (количество записей на странице).
    /// </summary>
    [JsonPropertyName("pageSize")]
    [JsonProperty("pageSize")]
    public int PageSize { get; set; }
  }

}
