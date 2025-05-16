using Newtonsoft.Json;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace automation.mbtdistr.ru.Services.YandexMarket.Models
{
  /// <summary>
  /// Валюта и ее значение.
  /// </summary>


  public class YMCurrencyValue
  {
    [JsonIgnore, Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }  // для EF Core

    /// <summary>
    /// Код валюты.
    /// </summary>
    [Display(Name = "Код валюты")]
    [JsonProperty("currencyId")]
    public YMCurrencyType CurrencyId { get; set; }

    /// <summary>
    /// Значение.
    /// </summary>
    [Display(Name = "Значение")]
    [JsonProperty("value")]
    public long Value { get; set; }

    // ─── Ссылка на товар ──────

    [ForeignKey(nameof(SupplyRequestItem))]
    [JsonIgnore]
    public int YMSupplyRequestItemId { get; set; }

    [JsonIgnore, System.Text.Json.Serialization.JsonIgnore]
    public YMSupplyRequestItem SupplyRequestItem { get; set; }
  }

}
