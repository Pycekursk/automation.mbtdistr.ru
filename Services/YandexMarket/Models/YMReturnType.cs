using Newtonsoft.Json;

using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

/// <summary>
/// Тип заказа для фильтрации.
/// </summary>
public enum YMReturnType
{
  /// <summary>
  /// Невыкуп.
  /// </summary>
  [EnumMember(Value = "UNREDEEMED")]

  [JsonProperty("UNREDEEMED")]
  [Display(Name = "Невыкуп")]
  Unredeemed,

  /// <summary>
  /// Возврат.
  /// </summary>
  [EnumMember(Value = "RETURN")]

  [JsonProperty("RETURN")]
  [Display(Name = "Возврат")]
  Return,

  /// <summary>
  /// Неизвестный тип.
  /// </summary>
  [EnumMember(Value = "UNKNOWN")]

  [JsonProperty("UNKNOWN")]
  [Display(Name = "Неизвестный тип")]
  Unknown = 0
}
