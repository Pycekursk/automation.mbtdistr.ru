using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace automation.mbtdistr.ru.Services.Wildberries.Models
{
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



