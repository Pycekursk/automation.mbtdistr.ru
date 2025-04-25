using System.Text.Json.Serialization;

namespace automation.mbtdistr.ru.Services.Ozon.Models
{
  /// <summary>
  /// Модель валюты и суммы для денежных полей.
  /// </summary>
  public class Money
  {
    [JsonPropertyName("currency_code")]
    public string CurrencyCode { get; set; }

    [JsonPropertyName("price")]
    public float Price { get; set; }
  }

  
}
