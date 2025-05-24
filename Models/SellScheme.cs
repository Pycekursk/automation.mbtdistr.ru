using Newtonsoft.Json;

using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace automation.mbtdistr.ru.Models
{
  /// <summary>
  /// Схема продажи товара.
  /// </summary>
  public enum SellScheme
  {
    /// <summary>
    /// Неизвестно.
    /// </summary>
    [EnumMember(Value = "unknown")]
    [Display(Name = "Неизвестно")]
    [JsonProperty("unknown")]
    [System.Text.Json.Serialization.JsonPropertyName("unknown")]
    Unknown,

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
    FBS // Продавец на своём складе
  }
}