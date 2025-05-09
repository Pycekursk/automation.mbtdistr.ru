using Newtonsoft.Json;

using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace automation.mbtdistr.ru.Services.YandexMarket.Models
{
  /// <summary>
  /// Направление сортировки.
  /// </summary>
  public enum YMSortOrderType
  {
    /// <summary>
    /// Сортировка по возрастанию.
    /// </summary>
    [EnumMember(Value = "ASC")]
    [JsonPropertyName("ASC")]
    [JsonProperty("ASC")]
    [Display(Name = "По возрастанию")]
    Asc,

    /// <summary>
    /// Сортировка по убыванию.
    /// </summary>
    [EnumMember(Value = "DESC")]
    [JsonPropertyName("DESC")]
    [JsonProperty("DESC")]
    [Display(Name = "По убыванию")]
    Desc
  }
}
