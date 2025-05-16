using Newtonsoft.Json;

using System.ComponentModel.DataAnnotations;

namespace automation.mbtdistr.ru.Services.YandexMarket.Models
{
  /// <summary>
  /// Пейджинг с прокруткой вперед.
  /// </summary>
  public class YMForwardScrollingPager
  {
    /// <summary>
    /// Идентификатор следующей страницы результатов.
    /// </summary>
    [Display(Name = "Идентификатор следующей страницы результатов")]
    [JsonProperty("nextPageToken")]
    public string NextPageToken { get; set; }
  }
  
}
