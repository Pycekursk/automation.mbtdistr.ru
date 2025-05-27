using Newtonsoft.Json;

using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace automation.mbtdistr.ru.Models
{
  /// <summary>
  /// Тип возврата.
  /// </summary>
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
    /// Частичный возврат.
    /// </summary>
    [EnumMember(Value = "PARTIAL_RETURN")]
    [JsonProperty("PARTIAL_RETURN")]
    [System.Text.Json.Serialization.JsonPropertyName("PARTIAL_RETURN")]
    [Display(Name = "Частичный отказ при вручении")]
    PartialReturn,

    /// <summary>
    /// Неизвестный тип.
    /// </summary>
    [EnumMember(Value = "UNKNOWN")]
    [JsonProperty("UNKNOWN")]
    [System.Text.Json.Serialization.JsonPropertyName("UNKNOW")]
    [Display(Name = "Неизвестный")]
    Unknown,

    /// <summary>
    /// Полный отказ при вручении.
    /// </summary>
    [EnumMember(Value = "FULL_RETURN")]
    [JsonProperty("FULL_RETURN")]
    [System.Text.Json.Serialization.JsonPropertyName("FULL_RETURN")]
    [Display(Name = "Полный отказ при вручении")]
    FullReturn
  }
}