using Newtonsoft.Json;

using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace automation.mbtdistr.ru.Services.YandexMarket.Models
{

  /// <summary>
  /// Статус заявки на поставку.
  /// </summary>
  public enum YMSupplyRequestStatusType
  {
    /// <summary>
    /// Создан черновик заявки.
    /// </summary>
    [EnumMember(Value = "CREATED")]
    [JsonPropertyName("CREATED")]
    [JsonProperty("CREATED")]
    [Display(Name = "Черновик")]
    Created,

    /// <summary>
    /// Заявка завершена, товары приняты / перемещены / выданы / утилизированы.
    /// </summary>
    [EnumMember(Value = "FINISHED")]
    [JsonPropertyName("FINISHED")]
    [JsonProperty("FINISHED")]
    [Display(Name = "Завершена")]
    Finished,

    /// <summary>
    /// Заявка отменена.
    /// </summary>
    [EnumMember(Value = "CANCELLED")]
    [JsonPropertyName("CANCELLED")]
    [JsonProperty("CANCELLED")]
    [Display(Name = "Отменена")]
    Cancelled,

    /// <summary>
    /// Ошибка обработки заявки.
    /// </summary>
    [EnumMember(Value = "INVALID")]
    [JsonPropertyName("INVALID")]
    [JsonProperty("INVALID")]
    [Display(Name = "Ошибка обработки")]
    Invalid,

    /// <summary>
    /// Заявка в обработке.
    /// </summary>
    [EnumMember(Value = "VALIDATED")]
    [JsonPropertyName("VALIDATED")]
    [JsonProperty("VALIDATED")]
    [Display(Name = "В обработке")]
    Validated,

    /// <summary>
    /// Создана заявка.
    /// </summary>
    [EnumMember(Value = "PUBLISHED")]
    [JsonPropertyName("PUBLISHED")]
    [JsonProperty("PUBLISHED")]
    [Display(Name = "Создана заявка")]
    Published,

    /// <summary>
    /// Поставка прибыла на склад хранения.
    /// </summary>
    [EnumMember(Value = "ARRIVED_TO_SERVICE")]
    [JsonPropertyName("ARRIVED_TO_SERVICE")]
    [JsonProperty("ARRIVED_TO_SERVICE")]
    [Display(Name = "Прибыла на склад хранения")]
    ArrivedToService,

    /// <summary>
    /// Поставка прибыла на транзитный склад.
    /// </summary>
    [EnumMember(Value = "ARRIVED_TO_XDOC_SERVICE")]
    [JsonPropertyName("ARRIVED_TO_XDOC_SERVICE")]
    [JsonProperty("ARRIVED_TO_XDOC_SERVICE")]
    [Display(Name = "Прибыла на транзитный склад")]
    ArrivedToXDocService,

    /// <summary>
    /// Поставка отправлена с транзитного склада на склад хранения.
    /// </summary>
    [EnumMember(Value = "SHIPPED_TO_SERVICE")]
    [JsonPropertyName("SHIPPED_TO_SERVICE")]
    [JsonProperty("SHIPPED_TO_SERVICE")]
    [Display(Name = "Отправлена на склад хранения")]
    ShippedToService,

    /// <summary>
    /// Запрошена отмена заявки.
    /// </summary>
    [EnumMember(Value = "CANCELLATION_REQUESTED")]
    [JsonPropertyName("CANCELLATION_REQUESTED")]
    [JsonProperty("CANCELLATION_REQUESTED")]
    [Display(Name = "Запрошена отмена")]
    CancellationRequested,

    /// <summary>
    /// Отмена заявки отклонена.
    /// </summary>
    [EnumMember(Value = "CANCELLATION_REJECTED")]
    [JsonPropertyName("CANCELLATION_REJECTED")]
    [JsonProperty("CANCELLATION_REJECTED")]
    [Display(Name = "Отмена отклонена")]
    CancellationRejected,

    /// <summary>
    /// Поставка зарегистрирована в электронной очереди.
    /// </summary>
    [EnumMember(Value = "REGISTERED_IN_ELECTRONIC_QUEUE")]
    [JsonPropertyName("REGISTERED_IN_ELECTRONIC_QUEUE")]
    [JsonProperty("REGISTERED_IN_ELECTRONIC_QUEUE")]
    [Display(Name = "В электронной очереди")]
    RegisteredInElectronicQueue,

    /// <summary>
    /// Товары готовы к утилизации.
    /// </summary>
    [EnumMember(Value = "READY_FOR_UTILIZATION")]
    [JsonPropertyName("READY_FOR_UTILIZATION")]
    [JsonProperty("READY_FOR_UTILIZATION")]
    [Display(Name = "Готово к утилизации")]
    
    ReadyForUtilization,

    /// <summary>
    /// Перемещение товаров на склад вывоза.
    /// </summary>
    [EnumMember(Value = "TRANSIT_MOVING")]
    [JsonPropertyName("TRANSIT_MOVING")]
    [JsonProperty("TRANSIT_MOVING")]
    [Display(Name = "Перемещение на склад вывоза")]
    TransitMoving,

    /// <summary>
    /// Вторичная приемка, сборка на утилизацию или выдачу.
    /// </summary>
    [EnumMember(Value = "WAREHOUSE_HANDLING")]
    [JsonPropertyName("WAREHOUSE_HANDLING")]
    [JsonProperty("WAREHOUSE_HANDLING")]
    [Display(Name = "Обработка на складе")]
    WarehouseHandling,

    /// <summary>
    /// Информация о заявке направлена на склад.
    /// </summary>
    [EnumMember(Value = "ACCEPTED_BY_WAREHOUSE_SYSTEM")]
    [JsonPropertyName("ACCEPTED_BY_WAREHOUSE_SYSTEM")]
    [JsonProperty("ACCEPTED_BY_WAREHOUSE_SYSTEM")]
    [Display(Name = "Отправлена на склад")]
    AcceptedByWarehouseSystem,

    /// <summary>
    /// Товары готовы к выдаче.
    /// </summary>
    [EnumMember(Value = "READY_TO_WITHDRAW")]
    [JsonPropertyName("READY_TO_WITHDRAW")]
    [JsonProperty("READY_TO_WITHDRAW")]
    [Display(Name = "Готово к выдаче")]
    ReadyToWithdraw
  }
}
