using Newtonsoft.Json;

using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace automation.mbtdistr.ru.Models
{
  /// <summary>
  /// Тип возврата.
  /// </summary>
  [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter)), System.Text.Json.Serialization.JsonConverter(typeof(JsonStringEnumConverter))]
  public enum ReturnType
  {
    /// <summary>
    /// Невыкуп.
    /// </summary>
    [EnumMember(Value = "UNREDEEMED")]
    [JsonProperty("UNREDEEMED")]
    [System.Text.Json.Serialization.JsonPropertyName("UNREDEEMED")]
    [Display(Name = "Невыкуп")]
    Unredeemed,

    /// <summary>
    /// Возврат.
    /// </summary>
    [EnumMember(Value = "RETURN")]
    [System.Text.Json.Serialization.JsonPropertyName("RETURN")]
    [JsonProperty("RETURN")]
    [Display(Name = "Возврат")]
    Return,

    /// <summary>
    /// Неизвестный тип.
    /// </summary>
    [EnumMember(Value = "UNKNOWN")]
    [JsonProperty("UNKNOWN")]
    [System.Text.Json.Serialization.JsonPropertyName("UNKNOW")]
    [Display(Name = "Неизвестный")]
    Unknown
  }
}