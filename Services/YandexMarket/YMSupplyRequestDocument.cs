using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using System.ComponentModel.DataAnnotations;

using JsonConverter = Newtonsoft.Json.JsonConverter;

namespace automation.mbtdistr.ru.Services.YandexMarket
{
  /// <summary>
  /// Информация о документе по заявке.
  /// </summary>
  public class YMSupplyRequestDocument
  {
    /// <summary>
    /// Дата и время создания документа.
    /// </summary>
    [Display(Name = "Дата и время создания документа")]
    [JsonProperty("createdAt")]
    [Required]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Тип документа.
    /// </summary>
    [Display(Name = "Тип документа")]
    [JsonProperty("type")]
    [JsonConverter(typeof(StringEnumConverter))]
    [Required]
    public YMSupplyRequestDocumentType Type { get; set; }

    /// <summary>
    /// Ссылка на документ.
    /// </summary>
    [Display(Name = "Ссылка на документ")]
    [JsonProperty("url")]
    [Required]
    public string Url { get; set; }
  }
  
}
