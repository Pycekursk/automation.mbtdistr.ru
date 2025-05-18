using Newtonsoft.Json;

using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace automation.mbtdistr.ru.Models
{
  [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
  public enum ReturnStatus
  {
    [EnumMember(Value = "DisputeOpened")]
    [Display(Name = "Открыт спор с покупателем")]
    DisputeOpened,

    [EnumMember(Value = "OnSellerApproval")]
    [Display(Name = "На согласовании у продавца")]
    OnSellerApproval,

    [EnumMember(Value = "ArrivedAtReturnPlace")]
    [Display(Name = "В пункте выдачи")]
    ArrivedAtReturnPlace,

    [EnumMember(Value = "OnSellerClarification")]
    [Display(Name = "На уточнении у продавца")]
    OnSellerClarification,

    [EnumMember(Value = "OnSellerClarificationAfterPartialCompensation")]
    [Display(Name = "На уточнении у продавца после частичной компенсации")]
    OnSellerClarificationAfterPartialCompensation,

    [EnumMember(Value = "OfferedPartialCompensation")]
    [Display(Name = "Предложена частичная компенсация")]
    OfferedPartialCompensation,

    [EnumMember(Value = "ReturnMoneyApproved")]
    [Display(Name = "Одобрен возврат денег")]
    ReturnMoneyApproved,

    [EnumMember(Value = "PartialCompensationReturned")]
    [Display(Name = "Вернули часть денег")]
    PartialCompensationReturned,

    [EnumMember(Value = "CancelledDisputeNotOpen")]
    [Display(Name = "Возврат отклонён, спор не открыт")]
    CancelledDisputeNotOpen,

    [EnumMember(Value = "Rejected")]
    [Display(Name = "Заявка отклонена")]
    Rejected,

    [EnumMember(Value = "CrmRejected")]
    [Display(Name = "Заявка отклонена Ozon")]
    CrmRejected,

    [EnumMember(Value = "Cancelled")]
    [Display(Name = "Заявка отменена")]
    Cancelled,

    [EnumMember(Value = "Approved")]
    [Display(Name = "Заявка одобрена продавцом")]
    Approved,

    [EnumMember(Value = "ApprovedByOzon")]
    [Display(Name = "Заявка одобрена Ozon")]
    ApprovedByOzon,

    [EnumMember(Value = "ReceivedBySeller")]
    [Display(Name = "Продавец получил возврат")]
    ReceivedBySeller,

    [EnumMember(Value = "MovingToSeller")]
    [Display(Name = "Возврат на пути к продавцу")]
    MovingToSeller,

    [EnumMember(Value = "ReturnCompensated")]
    [Display(Name = "Продавец получил компенсацию")]
    ReturnCompensated,

    [EnumMember(Value = "ReturningToSellerByCourier")]
    [Display(Name = "Курьер везёт возврат продавцу")]
    ReturningToSellerByCourier,

    [EnumMember(Value = "Utilizing")]
    [Display(Name = "На утилизации")]
    Utilizing,

    [EnumMember(Value = "Utilized")]
    [Display(Name = "Утилизирован")]
    Utilized,

    [EnumMember(Value = "MoneyReturned")]
    [Display(Name = "Покупателю вернули всю сумму")]
    MoneyReturned,

    [EnumMember(Value = "PartialCompensationInProcess")]
    [Display(Name = "Одобрен частичный возврат денег")]
    PartialCompensationInProcess,

    [EnumMember(Value = "DisputeYouOpened")]
    [Display(Name = "Продавец открыл спор")]
    DisputeYouOpened,

    [EnumMember(Value = "CompensationRejected")]
    [Display(Name = "Отказано в компенсации")]
    CompensationRejected,

    [EnumMember(Value = "DisputeOpening")]
    [Display(Name = "Обращение в поддержку отправлено")]
    DisputeOpening,

    [EnumMember(Value = "CompensationOffered")]
    [Display(Name = "Ожидает вашего решения по компенсации")]
    CompensationOffered,

    [EnumMember(Value = "WaitingCompensation")]
    [Display(Name = "Ожидает компенсации")]
    WaitingCompensation,

    [EnumMember(Value = "SendingError")]
    [Display(Name = "Ошибка при отправке обращения в поддержку")]
    SendingError,

    [EnumMember(Value = "CompensationRejectedBySla")]
    [Display(Name = "Истёк срок решения")]
    CompensationRejectedBySla,

    [EnumMember(Value = "CompensationRejectedBySeller")]
    [Display(Name = "Продавец отказался от компенсации")]
    CompensationRejectedBySeller,

    [EnumMember(Value = "MovingToOzon")]
    [Display(Name = "Едет на склад Ozon")]
    MovingToOzon,

    [EnumMember(Value = "ReturnedToOzon")]
    [Display(Name = "На складе Ozon")]
    ReturnedToOzon,

    [EnumMember(Value = "MoneyReturnedBySystem")]
    [Display(Name = "Быстрый возврат")]
    MoneyReturnedBySystem,

    [EnumMember(Value = "WaitingShipment")]
    [Display(Name = "Ожидает отправки")]
    WaitingShipment,

    #region yandex market statuses

    [EnumMember(Value = "CREATED")]
    [JsonProperty("CREATED")]
    [Display(Name = "Возврат создан")]
    Created,

    [EnumMember(Value = "RECEIVED")]
    [JsonProperty("RECEIVED")]
    [Display(Name = "Возврат принят у отправителя")]
    Received,

    [EnumMember(Value = "IN_TRANSIT")]
    [JsonProperty("IN_TRANSIT")]
    [Display(Name = "На пути к продавцу")]
    InTransit,

    [EnumMember(Value = "READY_FOR_PICKUP")]
    [JsonProperty("READY_FOR_PICKUP")]
    [Display(Name = "Возврат готов к выдаче магазину")]
    ReadyForPickup,

    [EnumMember(Value = "PICKED")]
    [JsonProperty("PICKED")]
    [Display(Name = "Возврат выдан магазину")]
    Picked,

    [EnumMember(Value = "RECEIVED_ON_FULFILLMENT")]
    [JsonProperty("RECEIVED_ON_FULFILLMENT")]
    [Display(Name = "Возврат принят на складе Маркета")]
    ReceivedOnFulfillment,

    [EnumMember(Value = "CANCELLED")]

    [JsonProperty("CANCELLED")]
    [Display(Name = "Возврат отменен")]
    YMCancelled,

    [EnumMember(Value = "LOST")]

    [JsonProperty("LOST")]
    [Display(Name = "Возврат утерян")]
    Lost,

    [EnumMember(Value = "UTILIZED")]

    [JsonProperty("UTILIZED")]
    [Display(Name = "Возврат утилизирован")]
    YMUtilized,

    [EnumMember(Value = "PREPARED_FOR_UTILIZATION")]
    [JsonProperty("PREPARED_FOR_UTILIZATION")]
    [Display(Name = "Возврат готов к утилизации")]
    PreparedForUtilization,

    [EnumMember(Value = "EXPROPRIATED")]
    [JsonProperty("EXPROPRIATED")]
    [Display(Name = "Товары в возврате направлены на перепродажу")]
    Expropriated,

    [EnumMember(Value = "NOT_IN_DEMAND")]
    [JsonProperty("NOT_IN_DEMAND")]
    [Display(Name = "Возврат не забрали с почты")]
    NotInDemand,

    #endregion

    [EnumMember(Value = "Unknown")]
    [Display(Name = "Неизвестен")]
    Unknown = 0
  }
}