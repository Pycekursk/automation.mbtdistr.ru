using Newtonsoft.Json;

using System.ComponentModel.DataAnnotations;

namespace automation.mbtdistr.ru.Services.YandexMarket.Models
{
  #region Request Models

  #endregion


  /// <summary>
  /// Общий формат ошибки, возвращаемой Партнерским API Яндекс Маркета
  /// (массив <c>errors</c> в ответах со статусом <c>ERROR</c>) :contentReference[oaicite:0]{index=0}:contentReference[oaicite:1]{index=1}.
  /// </summary>
  public class YMApiError
  {
    /// <summary>Код ошибки.</summary>
    [Display(Name = "Код ошибки")]
    [JsonProperty("code")]
    [Required]
    public string Code { get; set; }

    /// <summary>Описание ошибки.</summary>
    [Display(Name = "Описание ошибки")]
    [JsonProperty("message")]
    [Required]
    public string Message { get; set; }
  }

}
