using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace automation.mbtdistr.ru.Services.YandexMarket.Models
{
  /// <summary>
  /// Тип валюты, используемый в системе.
  /// </summary>
  public enum YMCurrencyType
  {
    /// <summary>
    /// Российский рубль.
    /// </summary>
    [JsonPropertyName("RUR")]
    [JsonProperty("RUR")]
    RUR,

    /// <summary>
    /// Доллар США.
    /// </summary>
    [JsonPropertyName("USD")]
    [JsonProperty("USD")]
    USD,

    /// <summary>
    /// Евро.
    /// </summary>
    [JsonPropertyName("EUR")]
    [JsonProperty("EUR")]
    EUR,

    /// <summary>
    /// Украинская гривна.
    /// </summary>
    [JsonPropertyName("UAH")]
    [JsonProperty("UAH")]
    UAH,

    /// <summary>
    /// Австралийский доллар.
    /// </summary>
    [JsonPropertyName("AUD")]
    [JsonProperty("AUD")]
    AUD,

    /// <summary>
    /// Британский фунт стерлингов.
    /// </summary>
    [JsonPropertyName("GBP")]
    [JsonProperty("GBP")]
    GBP,

    /// <summary>
    /// Белорусский рубль (старый).
    /// </summary>
    [JsonPropertyName("BYR")]
    [JsonProperty("BYR")]
    BYR,

    /// <summary>
    /// Белорусский рубль (новый).
    /// </summary>
    [JsonPropertyName("BYN")]
    [JsonProperty("BYN")]
    BYN,

    /// <summary>
    /// Датская крона.
    /// </summary>
    [JsonPropertyName("DKK")]
    [JsonProperty("DKK")]
    DKK,

    /// <summary>
    /// Исландская крона.
    /// </summary>
    [JsonPropertyName("ISK")]
    [JsonProperty("ISK")]
    ISK,

    /// <summary>
    /// Казахстанский тенге.
    /// </summary>
    [JsonPropertyName("KZT")]
    [JsonProperty("KZT")]
    KZT,

    /// <summary>
    /// Канадский доллар.
    /// </summary>
    [JsonPropertyName("CAD")]
    [JsonProperty("CAD")]
    CAD,

    /// <summary>
    /// Китайский юань.
    /// </summary>
    [JsonPropertyName("CNY")]
    [JsonProperty("CNY")]
    CNY,

    /// <summary>
    /// Норвежская крона.
    /// </summary>
    [JsonPropertyName("NOK")]
    [JsonProperty("NOK")]
    NOK,

    /// <summary>
    /// Специальные права заимствования (SDR).
    /// </summary>
    [JsonPropertyName("XDR")]
    [JsonProperty("XDR")]
    XDR,

    /// <summary>
    /// Сингапурский доллар.
    /// </summary>
    [JsonPropertyName("SGD")]
    [JsonProperty("SGD")]
    SGD,

    /// <summary>
    /// Турецкая лира.
    /// </summary>
    [JsonPropertyName("TRY")]
    [JsonProperty("TRY")]
    TRY,

    /// <summary>
    /// Шведская крона.
    /// </summary>
    [JsonPropertyName("SEK")]
    [JsonProperty("SEK")]
    SEK,

    /// <summary>
    /// Швейцарский франк.
    /// </summary>
    [JsonPropertyName("CHF")]
    [JsonProperty("CHF")]
    CHF,

    /// <summary>
    /// Японская иена.
    /// </summary>
    [JsonPropertyName("JPY")]
    [JsonProperty("JPY")]
    JPY,

    /// <summary>
    /// Азербайджанский манат.
    /// </summary>
    [JsonPropertyName("AZN")]
    [JsonProperty("AZN")]
    AZN
  }
}
