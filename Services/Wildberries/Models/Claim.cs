using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

using System.ComponentModel.DataAnnotations;
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
    [Display(Name = "Портал покупателей")]
    Portal = 1,

    /// <summary>
    /// 3 — чат.
    /// </summary>
    [EnumMember(Value = "3")]
    [Display(Name = "Чат")]
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
    [Display(Name = "На рассмотрении")]
    UnderReview = 0,

    /// <summary>
    /// 1 — отказ.
    /// </summary>
    [EnumMember(Value = "1")]
    [Display(Name = "Отказ")]
    Rejected = 1,

    /// <summary>
    /// 2 — одобрено.
    /// </summary>
    [EnumMember(Value = "2")]
    [Display(Name = "Одобрено")]
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
    [Display(Name = "На рассмотрении")]
    SubmissionUnderReview = 0,

    /// <summary>
    /// 1 — товар остаётся у покупателя (заявка отклонена).
    /// </summary>
    [EnumMember(Value = "1")]
    [Display(Name = "Товар остаётся у покупателя (заявка отклонена)")]
    CustomerKeepsOnRejection = 1,

    /// <summary>
    /// 2 — покупатель сдаёт товар на WB, товар отправляется в утилизацию.
    /// </summary>
    [EnumMember(Value = "2")]
    [Display(Name = "Покупатель сдаёт товар на WB, товар отправляется в утилизацию")]
    SendToWBForDisposal = 2,

    /// <summary>
    /// 5 — товар остаётся у покупателя (заявка одобрена).
    /// </summary>
    [EnumMember(Value = "5")]
    [Display(Name = "Товар остаётся у покупателя (заявка одобрена)")]
    CustomerKeepsOnApproval = 5,

    /// <summary>
    /// 8 — товар будет возвращён в реализацию после проверки WB.
    /// </summary>
    [EnumMember(Value = "8")]
    [Display(Name = "Товар будет возвращён в реализацию после проверки WB")]
    ReturnToSaleAfterWBInspection = 8,

    /// <summary>
    /// 10 — товар возвращается продавцу.
    /// </summary>
    [EnumMember(Value = "10")]
    [Display(Name = "Товар возвращается продавцу")]
    ReturnToSeller = 10
  }


  /// <summary>
  /// Варианты ответа продавца на заявку.
  /// Если массив пуст — с заявкой работать нельзя.
  /// </summary>
  public enum ClaimAction
  {
    /// <summary>
    /// одобрить с проверкой брака.
    /// Деньги вернутся после возврата товара.
    /// Товар проверят на складе и, если подтвердится брак, отправят обратно продавцу.
    /// </summary>
    [EnumMember(Value = "approve1")]
    [Display(Name = "Одобрить с проверкой брака")]
    ApproveWithDefectCheck,

    /// <summary>
    /// одобрить и забрать товар.
    /// Деньги вернутся после возврата товара.
    /// Товар будет отправлен продавцу.
    /// </summary>
    [EnumMember(Value = "approve2")]
    [Display(Name = "Одобрить и забрать товар")]
    ApproveAndPickup,

    /// <summary>
    /// одобрить без возврата товара.
    /// Товар остаётся у покупателя, деньги вернутся сразу.
    /// </summary>
    [EnumMember(Value = "autorefund1")]
    [Display(Name = "Одобрить без возврата товара")]
    AutoRefundWithoutReturn,

    /// <summary>
    /// отклонить (шаблон «Брак не обнаружен»).
    /// </summary>
    [EnumMember(Value = "reject1")]
    [Display(Name = "Отклонить (шаблон «Брак не обнаружен»)")]
    RejectNoDefectFound,

    /// <summary>
    /// отклонить (шаблон «Добавить фото/видео»).
    /// </summary>
    [EnumMember(Value = "reject2")]
    [Display(Name = "Отклонить (шаблон «Добавить фото/видео»)")]
    RejectAddPhotoVideo,

    /// <summary>
    /// отклонить (шаблон «Направить в сервисный центр»).
    /// </summary>
    [EnumMember(Value = "reject3")]
    [Display(Name = "Отклонить (шаблон «Направить в сервисный центр»)")]
    RejectSendToServiceCenter,

    /// <summary>
    /// отклонить с собственным комментарием.
    /// Комментарий передаётся в параметре comment.
    /// </summary>
    [EnumMember(Value = "rejectcustom")]
    [Display(Name = "Отклонить с собственным комментарием")]
    RejectCustomComment,

    /// <summary>
    /// одобрить заявку с возвратом товара в магазин продавца.
    /// Применимо только при схеме «Самовывоз».
    /// </summary>
    [EnumMember(Value = "approvecc1")]
    [Display(Name = "Одобрить заявку с возвратом товара в магазин продавца")]
    ApproveReturnToSeller,

    /// <summary>
    /// подтвердить приёмку товара от покупателя.
    /// Применимо только при схеме «Самовывоз».
    /// </summary>
    [EnumMember(Value = "confirmreturngoodcc1")]
    [Display(Name = "Подтвердить приёмку товара от покупателя")]
    ConfirmReceiptBySeller
  }
}



