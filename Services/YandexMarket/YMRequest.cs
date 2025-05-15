using Newtonsoft.Json;

using System.ComponentModel.DataAnnotations;

namespace automation.mbtdistr.ru.Services.YandexMarket
{
  /// <summary>
  /// Модель запроса для получения документов по заявке.
  /// </summary>
  public class YMRequest
  {
    /// <summary>
    /// Идентификатор заявки.
    /// </summary>
    [Display(Name = "Идентификатор заявки")]
    [JsonProperty("requestId")]
    [Required]
    public long RequestId { get; set; }
  }
  
}
