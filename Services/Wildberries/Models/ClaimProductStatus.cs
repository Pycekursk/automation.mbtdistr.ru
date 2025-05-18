using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace automation.mbtdistr.ru.Services.Wildberries.Models
{
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
}



