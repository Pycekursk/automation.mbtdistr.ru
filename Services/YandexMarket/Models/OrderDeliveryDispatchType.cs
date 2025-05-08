using Newtonsoft.Json;

using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace automation.mbtdistr.ru.Services.YandexMarket.Models
{
  /// <summary>
  /// Способ отгрузки.
  /// </summary>
  public enum OrderDeliveryDispatchType
  {
    /// <summary>
    /// Доставка покупателю.
    /// </summary>
    [EnumMember(Value = "BUYER")]
    [JsonPropertyName("BUYER")]
    [JsonProperty("BUYER")]
    [Display(Name = "Доставка покупателю")]
    Buyer,

    /// <summary>
    /// Доставка в пункт выдачи заказов Маркета.
    /// </summary>
    [EnumMember(Value = "MARKET_BRANDED_OUTLET")]
    [JsonPropertyName("MARKET_BRANDED_OUTLET")]
    [JsonProperty("MARKET_BRANDED_OUTLET")]
    [Display(Name = "Доставка в пункт выдачи заказов Маркета")]
    MarketBrandedOutlet,

    /// <summary>
    /// Доставка в пункт выдачи заказов магазина.
    /// </summary>
    [EnumMember(Value = "SHOP_OUTLET")]
    [JsonPropertyName("SHOP_OUTLET")]
    [JsonProperty("SHOP_OUTLET")]
    [Display(Name = "Доставка в пункт выдачи заказов магазина")]
    ShopOutlet,

    /// <summary>
    /// Неизвестный тип.
    /// </summary>
    [EnumMember(Value = "UNKNOWN")]
    [JsonPropertyName("UNKNOWN")]
    [JsonProperty("UNKNOWN")]
    [Display(Name = "Неизвестный тип")]
    Unknown
  }

}
