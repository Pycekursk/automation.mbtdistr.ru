using Newtonsoft.Json;

using System.ComponentModel.DataAnnotations;

namespace automation.mbtdistr.ru.Services.YandexMarket.Models
{
  /// <summary>
  /// Обобщённый класс-ответ API Яндекс.Маркета.
  /// </summary>
  public class YMApiResponse<TResult>
  {
    /// <summary>
    /// Статус ответа.
    /// </summary>
    [Display(Name = "Статус ответа")]
    [JsonProperty("status")]
    public YMApiResponseStatusType Status { get; set; }

    /// <summary>
    /// Результат запроса.
    /// </summary>
    [Display(Name = "Результат запроса")]
    [JsonProperty("result")]
    public TResult Result { get; set; }

    /// <summary>
    /// Ошибки
    /// </summary>
    public List<YMApiError>? Errors { get; set; }
  }
}
