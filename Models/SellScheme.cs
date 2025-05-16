using Newtonsoft.Json;

using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace automation.mbtdistr.ru.Models
{
  /// <summary>
  /// Схема продажи товара.
  /// </summary>
  [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
  public enum SellScheme
  {
    /// <summary>
    /// Продавец на складе Ozon.
    /// </summary>
    [EnumMember(Value = "fbo")]
    [Display(Name = "ФБО")]
    [JsonProperty("fbo")]
    FBO, // Продавец на складе Ozon

    /// <summary>
    /// Продавец на своём складе.
    /// </summary>
    [EnumMember(Value = "fbs")]
    [Display(Name = "ФБС")]
    [JsonProperty("fbs")]
    FBS, // Продавец на своём складе

    /// <summary>
    /// Неизвестно.
    /// </summary>
    [EnumMember(Value = "unknown")]
    [Display(Name = "Неизвестно")]
    [JsonProperty("unknown")]
    Unknown = 0 // Неизвестно
  }
}