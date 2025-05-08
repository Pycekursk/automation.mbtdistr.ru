using System.Text.Json.Serialization;

namespace automation.mbtdistr.ru.Services.YandexMarket.Models
{
  /// <summary>
  /// Сумма и валюта возврата.
  /// </summary>
  public class Amount
  {
    /// <summary>
    /// Значение суммы возврата.
    /// </summary>
    [JsonPropertyName("value"), Newtonsoft.Json.JsonProperty("value")]
    public decimal Value { get; set; }

    /// <summary>
    /// Идентификатор валюты (например, RUB, USD).
    /// </summary>
    [JsonPropertyName("currencyId"), Newtonsoft.Json.JsonProperty("currencyId")]
    public YMCurrencyType CurrencyId { get; set; }
  }
}
