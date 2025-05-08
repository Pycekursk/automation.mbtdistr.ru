using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

using Newtonsoft.Json;

namespace automation.mbtdistr.ru.Services.YandexMarket.Models
{
  /// <summary>  
  /// Способ возврата товара покупателем.  
  /// </summary>  
  public enum YMShipmentRecipientType
  {
    /// <summary>  
    /// Возврат в магазин.  
    /// </summary>  
    [EnumMember(Value = "SHOP")]
    [JsonPropertyName("SHOP")]
    [JsonProperty("SHOP")]
    [Display(Name = "Магазин")]
    Shop,

    /// <summary>  
    /// Возврат через службу доставки.  
    /// </summary>  
    [EnumMember(Value = "DELIVERY_SERVICE")]
    [JsonPropertyName("DELIVERY_SERVICE")]
    [JsonProperty("DELIVERY_SERVICE")]
    [Display(Name = "Служба доставки")]
    DeliveryService,

    /// <summary>  
    /// Возврат через почту.  
    /// </summary>  
    [EnumMember(Value = "POST")]
    [JsonPropertyName("POST")]
    [JsonProperty("POST")]
    [Display(Name = "Почта")]
    Post
  }
}
