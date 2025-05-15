using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using System.ComponentModel.DataAnnotations;

namespace automation.mbtdistr.ru.Services.YandexMarket.Models
{
  /// <summary>
  /// Решение магазина по конкретному товару в возврате (элемент массива <c>decisions</c> внутри <c>ReturnItemDTO</c>) :contentReference[oaicite:0]{index=0}:contentReference[oaicite:1]{index=1}.
  /// </summary>
  public class YMDecision
  {
    /// <summary>Сумма, которую магазин возвращает покупателю (товар + доставка).</summary>
    [Display(Name = "Сумма возврата")]
    [JsonProperty("amount")]
    public YMCurrencyValue? Amount { get; set; }



    /// <summary>Идентификатор товара в возврате.</summary>
    [Display(Name = "Идентификатор товара в возврате")]
    [JsonProperty("returnItemId")]
    [Required, Range(1, long.MaxValue)]
    public long ReturnItemId { get; set; }

    /// <summary>Количество единиц товара, на которые распространяется решение.</summary>
    [Display(Name = "Количество единиц товара")]
    [JsonProperty("count")]
    [Required, Range(1, int.MaxValue)]
    public int Count { get; set; }

    /// <summary>Тип решения магазина.</summary>
    [Display(Name = "Тип решения магазина")]
    [JsonProperty("decisionType")]
    [Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
    [Required]
    public YMReturnDecisionType DecisionType { get; set; }

    /// <summary>Причина возврата (группа).</summary>
    [Display(Name = "Причина возврата")]
    [JsonProperty("reasonType")]
    [Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
    [Required]
    public YMReturnDecisionReasonType ReasonType { get; set; }

    /// <summary>Детализация причины возврата.</summary>
    [Display(Name = "Детализация причины возврата")]
    [JsonProperty("subreasonType")]
    [Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
    public YMReturnDecisionSubreasonType? SubreasonType { get; set; }

    /// <summary>Хеш-коды фотографий, приложенных покупателем.</summary>
    [Display(Name = "Фотографии")]
    [JsonProperty("images")]
    [MinLength(1)]
    public List<string>? Images { get; set; }

    /// <summary>Компенсация за обратную доставку (новый формат).</summary>
    [Display(Name = "Компенсация за обратную доставку")]
    [JsonProperty("partnerCompensationAmount")]
    public YMCurrencyValue? PartnerCompensationAmount { get; set; }

    /// <summary>Комментарий к решению (обязателен для некоторых <see cref="DecisionType"/> значений) :contentReference[oaicite:2]{index=2}:contentReference[oaicite:3]{index=3}.</summary>
    [Display(Name = "Комментарий к решению")]
    [JsonProperty("comment")]
    [MaxLength(2000)]
    public string? Comment { get; set; }

  

    #region Устаревшие поля

    /// <summary>Старая сумма возврата (копейки). Используйте <see cref="Amount"/>.</summary>
    [Obsolete("Поле устарело. Используйте Amount")]
    [JsonProperty("refundAmount")]
    public long? RefundAmount { get; set; }

    /// <summary>Старая компенсация доставки (копейки). Используйте <see cref="PartnerCompensationAmount"/>.</summary>
    [Obsolete("Поле устарело. Используйте PartnerCompensationAmount")]
    [JsonProperty("partnerCompensation")]
    public long? PartnerCompensation { get; set; }

    #endregion
  }

}
