using Newtonsoft.Json;

using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace automation.mbtdistr.ru.Services.YandexMarket.Models
{
  /// <summary>
  /// Тип заявки на поставку.
  /// </summary>
  public enum YMSupplyRequestType
  {
    /// <summary>
    /// Поставка товаров.
    /// </summary>
    [EnumMember(Value = "SUPPLY")]
    [JsonPropertyName("SUPPLY")]
    [JsonProperty("SUPPLY")]
    [Display(Name = "Поставка")]
    Supply,

    /// <summary>
    /// Вывоз товаров.
    /// </summary>
    [EnumMember(Value = "WITHDRAW")]
    [JsonPropertyName("WITHDRAW")]
    [JsonProperty("WITHDRAW")]
    [Display(Name = "Вывоз")]
    Withdraw,

    /// <summary>
    /// Утилизация товаров.
    /// </summary>
    [EnumMember(Value = "UTILIZATION")]
    [JsonPropertyName("UTILIZATION")]
    [JsonProperty("UTILIZATION")]
    [Display(Name = "Утилизация")]
    Utilization
  }
}
