using Newtonsoft.Json;

using System.Text.Json.Serialization;

namespace automation.mbtdistr.ru.Services.YandexMarket.Models
{
  /// <summary>
  /// Ответ на запрос кампаний.
  /// </summary>
  public class YMCampaignsResponse
  {
    /// <summary>
    /// Список кампаний.
    /// </summary>
    [JsonPropertyName("campaigns")]
    [JsonProperty("campaigns")]
    public List<YMCampaign> Campaigns { get; set; } = new();

    /// <summary>
    /// Информация о пагинации.
    /// </summary>
    [JsonPropertyName("pager")]
    [JsonProperty("pager")]
    public YMPager Pager { get; set; } = new();
  }

}
