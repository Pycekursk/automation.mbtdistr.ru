using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace automation.mbtdistr.ru.Services.Ozon.Models
{
  /// <summary>
  /// Детали статуса: ID, отображаемое и системное имя.
  /// </summary>
  public class StatusInfo
  {
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("display_name")]
    public string? DisplayName { get; set; }

    [JsonPropertyName("sys_name")]
    public OZVisualStatus? SysName { get; set; }
  }

  /// <summary>
  /// Визуальный статус возврата/заявки Ozon.
  /// Используется для отображения статуса возврата или заявки в пользовательском интерфейсе.
  /// </summary>
  public enum OZVisualStatus
  {
    /// <summary>Открыт спор с покупателем</summary>
    [Display(Name = "Открыт спор")]
    DisputeOpened,
    /// <summary>На согласовании у продавца</summary>
    [Display(Name = "На согласовании у продавца")]
    OnSellerApproval,
    /// <summary>Прибыл в пункт выдачи</summary>
    [Display(Name = "Прибыл в пункт выдачи")]
    ArrivedAtReturnPlace,
    /// <summary>На уточнении у продавца</summary>
    [Display(Name = "На уточнении у продавца")]
    OnSellerClarification,
    /// <summary>На уточнении у продавца после частичной компенсации</summary>
    [Display(Name = "На уточнении у продавца после частичной компенсации")]
    OnSellerClarificationAfterPartialCompensation,
    /// <summary>Предложена частичная компенсация</summary>
    [Display(Name = "Предложена частичная компенсация")]
    OfferedPartialCompensation,
    /// <summary>Одобрен возврат денег</summary>
    [Display(Name = "Одобрен возврат денег")]
    ReturnMoneyApproved,
    /// <summary>Вернули часть денег</summary>
    [Display(Name = "Вернули часть денег")]
    PartialCompensationReturned,
    /// <summary>Возврат отклонён, спор не открыт</summary>
    [Display(Name = "Возврат отклонён, спор не открыт")]
    CancelledDisputeNotOpen,
    /// <summary>Заявка отклонена</summary>
    [Display(Name = "Заявка отклонена")]
    Rejected,
    /// <summary>Заявка отклонена Ozon</summary>
    [Display(Name = "Заявка отклонена Ozon")]
    CrmRejected,
    /// <summary>Заявка отменена</summary>
    [Display(Name = "Заявка отменена")]
    Cancelled,
    /// <summary>Заявка одобрена продавцом</summary>
    [Display(Name = "Заявка одобрена продавцом")]
    Approved,
    /// <summary>Заявка одобрена Ozon</summary>
    [Display(Name = "Заявка одобрена Ozon")]
    ApprovedByOzon,
    /// <summary>Продавец получил возврат</summary>
    [Display(Name = "Продавец получил возврат")]
    ReceivedBySeller,
    /// <summary>Возврат на пути к продавцу</summary>
    [Display(Name = "Возврат на пути к продавцу")]
    MovingToSeller,
    /// <summary>Продавец получил компенсацию</summary>
    [Display(Name = "Продавец получил компенсацию")]
    ReturnCompensated,
    /// <summary>Курьер везёт возврат продавцу</summary>
    [Display(Name = "Курьер везёт возврат продавцу")]
    ReturningToSellerByCourier,
    /// <summary>На утилизации</summary>
    [Display(Name = "На утилизации")]
    Utilizing,
    /// <summary>Утилизирован</summary>
    [Display(Name = "Утилизирован")]
    Utilized,
    /// <summary>Покупателю вернули всю сумму</summary>
    [Display(Name = "Покупателю вернули всю сумму")]
    MoneyReturned,
    /// <summary>Одобрен частичный возврат денег</summary>
    [Display(Name = "Одобрен частичный возврат денег")]
    PartialCompensationInProcess,
    /// <summary>Продавец открыл спор</summary>
    [Display(Name = "Продавец открыл спор")]
    DisputeYouOpened,
    /// <summary>Отказано в компенсации</summary>
    [Display(Name = "Отказано в компенсации")]
    CompensationRejected,
    /// <summary>Обращение в поддержку отправлено</summary>
    [Display(Name = "Обращение в поддержку отправлено")]
    DisputeOpening,
    /// <summary>Ожидает вашего решения по компенсации</summary>
    [Display(Name = "Ожидает вашего решения по компенсации")]
    CompensationOffered,
    /// <summary>Ожидает компенсации</summary>
    [Display(Name = "Ожидает компенсации")]
    WaitingCompensation,
    /// <summary>Ошибка при отправке обращения в поддержку</summary>
    [Display(Name = "Ошибка при отправке обращения в поддержку")]
    SendingError,
    /// <summary>Истёк срок решения</summary>
    [Display(Name = "Истёк срок решения")]
    CompensationRejectedBySla,
    /// <summary>Продавец отказался от компенсации</summary>
    [Display(Name = "Продавец отказался от компенсации")]
    CompensationRejectedBySeller,
    /// <summary>Едет на склад Ozon</summary>
    [Display(Name = "Едет на склад Ozon")]
    MovingToOzon,
    /// <summary>На складе Ozon</summary>
    [Display(Name = "На складе Ozon")]
    ReturnedToOzon,
    /// <summary>Быстрый возврат</summary>
    [Display(Name = "Быстрый возврат")]
    MoneyReturnedBySystem,
    /// <summary>Ожидает отправки</summary>
    [Display(Name = "Ожидает отправки")]
    WaitingShipment
  }
}
