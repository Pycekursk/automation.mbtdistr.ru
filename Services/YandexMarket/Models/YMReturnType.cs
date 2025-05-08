using Newtonsoft.Json;

using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

using System.Text.Json.Serialization;

/// <summary>
/// Тип заказа для фильтрации.
/// </summary>
public enum YMReturnType
{
  /// <summary>
  /// Невыкуп.
  /// </summary>
  [EnumMember(Value = "UNREDEEMED")]
  [JsonPropertyName("UNREDEEMED")]
  [JsonProperty("UNREDEEMED")]
  [Display(Name = "Невыкуп")]
  Unredeemed,

  /// <summary>
  /// Возврат.
  /// </summary>
  [EnumMember(Value = "RETURN")]
  [JsonPropertyName("RETURN")]
  [JsonProperty("RETURN")]
  [Display(Name = "Возврат")]
  Return
}
