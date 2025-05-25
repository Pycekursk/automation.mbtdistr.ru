using Newtonsoft.Json;

using System.ComponentModel.DataAnnotations;

namespace automation.mbtdistr.ru.Services.YandexMarket.Models
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
    [System.Text.Json.Serialization.JsonPropertyName("requestId")]
    [Required]
    public long RequestId { get; set; }
  }

}
