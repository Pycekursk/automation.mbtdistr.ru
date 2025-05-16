using Newtonsoft.Json;

using System.ComponentModel.DataAnnotations;

namespace automation.mbtdistr.ru.Services.YandexMarket.Models
{
  /// <summary>
  /// Параметры запроса для получения изображения товара в возврате
  /// (GET /campaigns/{campaignId}/orders/{orderId}/returns/{returnId}/decision/{itemId}/image/{imageHash}). 
  /// </summary>
  public class YMReturnPhotoRequest
  {
    /// <summary>Идентификатор кампании.</summary>
    [Display(Name = "Идентификатор кампании")]
    [JsonProperty("campaignId")]
    [Range(1, long.MaxValue)]
    public long CampaignId { get; set; }

    /// <summary>Идентификатор заказа.</summary>
    [Display(Name = "Идентификатор заказа")]
    [JsonProperty("orderId")]
    [Range(1, long.MaxValue)]
    public long OrderId { get; set; }

    /// <summary>Идентификатор невыкупа или возврата.</summary>
    [Display(Name = "Идентификатор возврата")]
    [JsonProperty("returnId")]
    [Range(1, long.MaxValue)]
    public long ReturnId { get; set; }

    /// <summary>Идентификатор товара в возврате.</summary>
    [Display(Name = "Идентификатор товара в возврате")]
    [JsonProperty("itemId")]
    [Range(1, long.MaxValue)]
    public long ItemId { get; set; }

    /// <summary>Хеш-код изображения, полученный из массива images решения по возврату.</summary>
    [Display(Name = "Хеш-код изображения")]
    [JsonProperty("imageHash")]
    [Required, MaxLength(256)]
    public string ImageHash { get; set; }
  }
  
}
