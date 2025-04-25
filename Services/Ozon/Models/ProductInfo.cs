using System.Text.Json.Serialization;

namespace automation.mbtdistr.ru.Services.Ozon.Models
{
  /// <summary>
  /// Информация о товаре в возврате: SKU, наименование, цены и комиссия.
  /// </summary>
  public class ProductInfo
  {
    [JsonPropertyName("sku")]
    public long Sku { get; set; }

    [JsonPropertyName("offer_id")]
    public string? OfferId { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("price")]
    public Money Price { get; set; }

    [JsonPropertyName("price_without_commission")]
    public Money PriceWithoutCommission { get; set; }

    [JsonPropertyName("commission_percent")]
    public float CommissionPercent { get; set; }

    [JsonPropertyName("commission")]
    public Money Commission { get; set; }

    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }
  }

  
}
