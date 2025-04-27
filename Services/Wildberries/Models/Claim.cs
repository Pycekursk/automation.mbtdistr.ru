using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

using System.Runtime.Serialization;
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


  /// <summary>
  /// Источник подачи заявки на возврат.
  /// </summary>
  public enum ClaimSource
  {
    /// <summary>
    /// 1 — портал покупателей.
    /// </summary>
    [EnumMember(Value = "1")]
    Portal = 1,

    /// <summary>
    /// 3 — чат.
    /// </summary>
    [EnumMember(Value = "3")]
    Chat = 3
  }

  /// <summary>
  /// Решение по возврату покупателю.
  /// </summary>
  public enum ClaimDecision
  {
    /// <summary>
    /// 0 — на рассмотрении.
    /// </summary>
    [EnumMember(Value = "0")]
    UnderReview = 0,

    /// <summary>
    /// 1 — отказ.
    /// </summary>
    [EnumMember(Value = "1")]
    Rejected = 1,

    /// <summary>
    /// 2 — одобрено.
    /// </summary>
    [EnumMember(Value = "2")]
    Approved = 2
  }

  /// <summary>
  /// Статус товара в заявке на возврат.
  /// </summary>
  public enum ClaimProductStatus
  {
    /// <summary>
    /// 0 — заявка на рассмотрении.
    /// </summary>
    [EnumMember(Value = "0")]
    SubmissionUnderReview = 0,

    /// <summary>
    /// 1 — товар остаётся у покупателя (заявка отклонена).
    /// </summary>
    [EnumMember(Value = "1")]
    CustomerKeepsOnRejection = 1,

    /// <summary>
    /// 2 — покупатель сдаёт товар на WB, товар отправляется в утилизацию.
    /// </summary>
    [EnumMember(Value = "2")]
    SendToWBForDisposal = 2,

    /// <summary>
    /// 5 — товар остаётся у покупателя (заявка одобрена).
    /// </summary>
    [EnumMember(Value = "5")]
    CustomerKeepsOnApproval = 5,

    /// <summary>
    /// 8 — товар будет возвращён в реализацию после проверки WB.
    /// </summary>
    [EnumMember(Value = "8")]
    ReturnToSaleAfterWBInspection = 8,

    /// <summary>
    /// 10 — товар возвращается продавцу.
    /// </summary>
    [EnumMember(Value = "10")]
    ReturnToSeller = 10
  }


  /// <summary>
  /// Варианты ответа продавца на заявку.
  /// Если массив пуст — с заявкой работать нельзя.
  /// </summary>
  public enum ClaimAction
  {
    [EnumMember(Value = "approve1")]
    /// <summary>
    /// одобрить с проверкой брака.
    /// Деньги вернутся после возврата товара.
    /// Товар проверят на складе и, если подтвердится брак, отправят обратно продавцу.
    /// </summary>
    ApproveWithDefectCheck,

    [EnumMember(Value = "approve2")]
    /// <summary>
    /// одобрить и забрать товар.
    /// Деньги вернутся после возврата товара.
    /// Товар будет отправлен продавцу.
    /// </summary>
    ApproveAndPickup,

    [EnumMember(Value = "autorefund1")]
    /// <summary>
    /// одобрить без возврата товара.
    /// Товар остаётся у покупателя, деньги вернутся сразу.
    /// </summary>
    AutoRefundWithoutReturn,

    [EnumMember(Value = "reject1")]
    /// <summary>
    /// отклонить (шаблон «Брак не обнаружен»).
    /// </summary>
    RejectNoDefectFound,

    [EnumMember(Value = "reject2")]
    /// <summary>
    /// отклонить (шаблон «Добавить фото/видео»).
    /// </summary>
    RejectAddPhotoVideo,

    [EnumMember(Value = "reject3")]
    /// <summary>
    /// отклонить (шаблон «Направить в сервисный центр»).
    /// </summary>
    RejectSendToServiceCenter,

    [EnumMember(Value = "rejectcustom")]
    /// <summary>
    /// отклонить с собственным комментарием.
    /// Комментарий передаётся в параметре comment.
    /// </summary>
    RejectCustomComment,

    [EnumMember(Value = "approvecc1")]
    /// <summary>
    /// одобрить заявку с возвратом товара в магазин продавца.
    /// Применимо только при схеме «Самовывоз».
    /// </summary>
    ApproveReturnToSeller,

    [EnumMember(Value = "confirmreturngoodcc1")]
    /// <summary>
    /// подтвердить приёмку товара от покупателя.
    /// Применимо только при схеме «Самовывоз».
    /// </summary>
    ConfirmReceiptBySeller
  }
}



