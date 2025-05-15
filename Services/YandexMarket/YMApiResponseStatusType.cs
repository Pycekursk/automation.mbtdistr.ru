using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using System.Runtime.Serialization;

using JsonConverter = Newtonsoft.Json.JsonConverter;

namespace automation.mbtdistr.ru.Services.YandexMarket
{
  /// <summary>
  /// Тип ответа API. Возможные значения: OK — ошибок нет, ERROR — при обработке запроса произошла ошибка.
  /// </summary>
  [JsonConverter(typeof(StringEnumConverter))]
  public enum YMApiResponseStatusType
  {
    /// <summary>
    /// Ошибок нет.
    /// </summary>
    [EnumMember(Value = "OK")]
    OK,

    /// <summary>
    /// При обработке запроса произошла ошибка.
    /// </summary>
    [EnumMember(Value = "ERROR")]
    ERROR
  }
  
}
