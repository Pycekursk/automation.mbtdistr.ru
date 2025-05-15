using Newtonsoft.Json;

using System.Text.Json.Serialization;

namespace automation.mbtdistr.ru.Services.YandexMarket.Models
{
  /// <summary>
  /// Логистический пункт выдачи.
  /// </summary>
  public class YMLogisticPickupPoint
  {
    /// <summary>
    /// Идентификатор пункта выдачи.
    /// </summary>
    [JsonPropertyName("id")]
    [JsonProperty("id")]
    public long Id { get; set; }

    /// <summary>
    /// Название пункта выдачи.
    /// </summary>
    [JsonPropertyName("name")]
    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Адрес пункта выдачи.
    /// </summary>
    [JsonPropertyName("address")]
    [JsonProperty("address")]
    public YMAddress Address { get; set; } = new();

    /// <summary>
    /// Инструкция по получению.
    /// </summary>
    [JsonPropertyName("instruction")]
    [JsonProperty("instruction")]
    public string Instruction { get; set; } = string.Empty;

    /// <summary>
    /// Тип пункта выдачи.
    /// </summary>
    [JsonPropertyName("type")]
    [JsonProperty("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Идентификатор логистического партнёра.
    /// </summary>
    [JsonPropertyName("logisticPartnerId")]
    [JsonProperty("logisticPartnerId")]
    public long LogisticPartnerId { get; set; }
  }

}
