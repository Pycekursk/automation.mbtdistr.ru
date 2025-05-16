using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using System.Runtime.Serialization;

using JsonConverter = Newtonsoft.Json.JsonConverter;

namespace automation.mbtdistr.ru.Services.YandexMarket.Models
{
  /// <summary>
  /// Коды валют.
  /// </summary>
  [JsonConverter(typeof(StringEnumConverter))]
  public enum YMCurrencyType
  {
    /// <summary>
    /// Российский рубль.
    /// </summary>
    [EnumMember(Value = "RUR")]
    RUR,

    /// <summary>
    /// Украинская гривна.
    /// </summary>
    [EnumMember(Value = "UAH")]
    UAH,

    /// <summary>
    /// Белорусский рубль.
    /// </summary>
    [EnumMember(Value = "BYR")]
    BYR,

    /// <summary>
    /// Казахстанский тенге.
    /// </summary>
    [EnumMember(Value = "KZT")]
    KZT,

    /// <summary>
    /// Узбекский сум.
    /// </summary>
    [EnumMember(Value = "UZS")]
    UZS

    // при необходимости добавить другие валюты
  }
  
}
