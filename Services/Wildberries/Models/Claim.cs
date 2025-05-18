using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

using System.Text.Json.Serialization;

namespace automation.mbtdistr.ru.Services.Wildberries.Models
{
  public class Claim
  {
    /// <summary>
    /// Идентификатор заявки
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; }

    /// <summary>
    /// Источник заявки
    /// </summary>
    [JsonPropertyName("claim_type")]
    public ClaimSource ClaimType { get; set; }

    /// <summary>
    /// Решение по возврату покупателю
    /// </summary>
    [JsonPropertyName("status")]
    public ClaimDecision Status { get; set; }

    /// <summary>
    /// Статус товара
    /// </summary>
    [JsonPropertyName("status_ex")]
    public ClaimProductStatus StatusEx { get; set; }

    /// <summary>
    /// Артикул WB
    /// </summary>
    [JsonPropertyName("nm_id")]
    public long NmId { get; set; }

    /// <summary>
    /// Комментарий от покупателя
    /// </summary>
    [JsonPropertyName("user_comment")]
    public string UserComment { get; set; } = string.Empty;

    /// <summary>
    /// Комментарий WB
    /// </summary>
    [JsonPropertyName("wb_comment")]
    public string WbComment { get; set; } = string.Empty;

    /// <summary>
    /// Дата/время создания заявки (UTC)
    /// </summary>
    [JsonPropertyName("dt")]
    public DateTime Dt { get; set; }

    /// <summary>
    /// Название товара
    /// </summary>
    [JsonPropertyName("imt_name")]
    public string ImtName { get; set; } = string.Empty;

    /// <summary>
    /// Дата/время заказа (UTC)
    /// </summary>
    [JsonPropertyName("order_dt")]
    public DateTime OrderDt { get; set; }

    /// <summary>
    /// Дата/время последнего обновления (UTC)
    /// </summary>
    [JsonPropertyName("dt_update")]
    public DateTime DtUpdate { get; set; }

    /// <summary>
    /// Фото (массив URL или путей)
    /// </summary>
    [JsonPropertyName("photos")]
    public List<string> Photos { get; set; } = new();

    /// <summary>
    /// Видео (массив URL или путей)
    /// </summary>
    [JsonPropertyName("video_paths")]
    public List<string> VideoPaths { get; set; } = new();

    /// <summary>
    /// Варианты ответа продавца
    /// </summary>
    [JsonPropertyName("actions")]
    public List<ClaimAction> Actions { get; set; } = new();

    /// <summary>
    /// Фактическая цена с учётом всех скидок
    /// </summary>
    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    /// <summary>
    /// Код валюты (ISO-код)
    /// </summary>
    [JsonPropertyName("currency_code")]
    public string CurrencyCode { get; set; } = string.Empty;

    /// <summary>
    /// Внутренний идентификатор SRID
    /// </summary>
    [JsonPropertyName("srid")]
    public string Srid { get; set; } = string.Empty;
  }
}



