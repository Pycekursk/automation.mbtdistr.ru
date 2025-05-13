using Newtonsoft.Json;

using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace automation.mbtdistr.ru.Models
{
  /// <summary>
  /// Схема продажи товара.
  /// </summary>
  [Newtonsoft.Json.JsonConverter(typeof(JsonStringEnumConverter)), System.Text.Json.Serialization.JsonConverter(typeof(JsonStringEnumConverter))]
  public enum SellScheme
  {
    /// <summary>
    /// Продавец на складе Ozon.
    /// </summary>
    [EnumMember(Value = "fbo")]
    [Display(Name = "ФБО")]
    [JsonPropertyName("fbo")]
    [JsonProperty("fbo")]
    FBO, // Продавец на складе Ozon

    /// <summary>
    /// Продавец на своём складе.
    /// </summary>
    [EnumMember(Value = "fbs")]
    [Display(Name = "ФБС")]
    [JsonPropertyName("fbs")]
    [JsonProperty("fbs")]
    FBS, // Продавец на своём складе

    /// <summary>
    /// Неизвестно.
    /// </summary>
    [EnumMember(Value = "unknown")]
    [Display(Name = "Неизвестно")]
    [JsonPropertyName("unknown")]
    [JsonProperty("unknown")]
    Unknown = 0 // Неизвестно
  }
}