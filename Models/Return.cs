using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace automation.mbtdistr.ru.Models
{
  public class Return
  {
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int? UserId { get; set; } // кто оформил возврат

    public Worker? User { get; set; }

    public int CabinetId { get; set; } // кабинет/бренд/ООО
    public Cabinet Cabinet { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public DateTime? ChangedAt { get; set; }

    public Compensation? Compensation { get; set; }

    public ReturnMainInfo Info { get; set; } = new ReturnMainInfo(); // информация о возврате
    public bool IsOpened { get; set; }
    public bool IsSuperEconom { get; set; }
    public DateTime OrderedAt { get; internal set; }
  }

  public class ReturnMainInfo
  {
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int ReturnId { get; set; }
    public Return Return { get; set; }

    public ReturnStatus ReturnStatus { get; set; } = ReturnStatus.Unknown; // статус возврата

    public string? ReturnReasonName { get; set; } // причина возврата

    public long ReturnInfoId { get; set; } // ID возврата в системе

    public long OrderId { get; set; } // ID заказа в системе

    public string? ClaimId { get; set; } // ID обращения в системе
  }

  public enum ReturnStatus
  {
    [Display(Name = "Открыт спор с покупателем")]
    DisputeOpened,

    [Display(Name = "На согласовании у продавца")]
    OnSellerApproval,

    [Display(Name = "В пункте выдачи")]
    ArrivedAtReturnPlace,

    [Display(Name = "На уточнении у продавца")]
    OnSellerClarification,

    [Display(Name = "На уточнении у продавца после частичной компенсации")]
    OnSellerClarificationAfterPartialCompensation,

    [Display(Name = "Предложена частичная компенсация")]
    OfferedPartialCompensation,

    [Display(Name = "Одобрен возврат денег")]
    ReturnMoneyApproved,

    [Display(Name = "Вернули часть денег")]
    PartialCompensationReturned,

    [Display(Name = "Возврат отклонён, спор не открыт")]
    CancelledDisputeNotOpen,

    [Display(Name = "Заявка отклонена")]
    Rejected,

    [Display(Name = "Заявка отклонена Ozon")]
    CrmRejected,

    [Display(Name = "Заявка отменена")]
    Cancelled,

    [Display(Name = "Заявка одобрена продавцом")]
    Approved,

    [Display(Name = "Заявка одобрена Ozon")]
    ApprovedByOzon,

    [Display(Name = "Продавец получил возврат")]
    ReceivedBySeller,

    [Display(Name = "Возврат на пути к продавцу")]
    MovingToSeller,

    [Display(Name = "Продавец получил компенсацию")]
    ReturnCompensated,

    [Display(Name = "Курьер везёт возврат продавцу")]
    ReturningToSellerByCourier,

    [Display(Name = "На утилизации")]
    Utilizing,

    [Display(Name = "Утилизирован")]
    Utilized,

    [Display(Name = "Покупателю вернули всю сумму")]
    MoneyReturned,

    [Display(Name = "Одобрен частичный возврат денег")]
    PartialCompensationInProcess,

    [Display(Name = "Продавец открыл спор")]
    DisputeYouOpened,

    [Display(Name = "Отказано в компенсации")]
    CompensationRejected,

    [Display(Name = "Обращение в поддержку отправлено")]
    DisputeOpening,

    [Display(Name = "Ожидает вашего решения по компенсации")]
    CompensationOffered,

    [Display(Name = "Ожидает компенсации")]
    WaitingCompensation,

    [Display(Name = "Ошибка при отправке обращения в поддержку")]
    SendingError,

    [Display(Name = "Истёк срок решения")]
    CompensationRejectedBySla,

    [Display(Name = "Продавец отказался от компенсации")]
    CompensationRejectedBySeller,

    [Display(Name = "Едет на склад Ozon")]
    MovingToOzon,

    [Display(Name = "На складе Ozon")]
    ReturnedToOzon,

    [Display(Name = "Быстрый возврат")]
    MoneyReturnedBySystem,

    [Display(Name = "Ожидает отправки")]
    WaitingShipment,

    [Display(Name = "Неизвестен")]
    Unknown
  }
}