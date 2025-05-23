using Newtonsoft.Json;

using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace automation.mbtdistr.ru.Models
{
  /// <summary>
  /// Схема продажи товара.
  /// </summary>
  [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter)), System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
  public enum SellScheme
  {
    /// <summary>
    /// Продавец на складе Ozon.
    /// </summary>
    [EnumMember(Value = "fbo")]
    [Display(Name = "ФБО")]
    [JsonProperty("fbo")]
    [System.Text.Json.Serialization.JsonPropertyName("fbo")]
    FBO, // Продавец на складе Ozon

    /// <summary>
    /// Продавец на своём складе.
    /// </summary>
    [EnumMember(Value = "fbs")]
    [Display(Name = "ФБС")]
    [JsonProperty("fbs")]
    [System.Text.Json.Serialization.JsonPropertyName("fbs")]
    FBS, // Продавец на своём складе

    /// <summary>
    /// Неизвестно.
    /// </summary>
    [EnumMember(Value = "unknown")]
    [Display(Name = "Неизвестно")]
    [JsonProperty("unknown")]
    [System.Text.Json.Serialization.JsonPropertyName("unknown")]
    Unknown = 0 // Неизвестно
  }
}