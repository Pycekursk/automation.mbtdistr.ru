using Newtonsoft.Json;

using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace automation.mbtdistr.ru.Services.YandexMarket.Models
{
  /// <summary>
  /// Подтип заявки на поставку.
  /// </summary>
  public enum YMSupplyRequestSubType
  {
    /// <summary>
    /// Поставка товаров на склад хранения или вывоз с него.
    /// </summary>
    [EnumMember(Value = "DEFAULT")]
    [JsonPropertyName("DEFAULT")]
    [JsonProperty("DEFAULT")]
    [Display(Name = "Обычная поставка")]
    Default,

    /// <summary>
    /// Поставка через транзитный склад или вывоз с него.
    /// </summary>
    [EnumMember(Value = "XDOC")]
    [JsonPropertyName("XDOC")]
    [JsonProperty("XDOC")]
    [Display(Name = "Транзитная поставка")]
    XDoc,

    /// <summary>
    /// Инвентаризация на складе по запросу магазина.
    /// </summary>
    [EnumMember(Value = "INVENTORYING_SUPPLY")]
    [JsonPropertyName("INVENTORYING_SUPPLY")]
    [JsonProperty("INVENTORYING_SUPPLY")]
    [Display(Name = "Инвентаризация по запросу магазина")]
    InventoryingSupply,

    /// <summary>
    /// Инвентаризация на складе по запросу склада.
    /// </summary>
    [EnumMember(Value = "INVENTORYING_SUPPLY_WAREHOUSE_BASED_PER_SUPPLIER")]
    [JsonPropertyName("INVENTORYING_SUPPLY_WAREHOUSE_BASED_PER_SUPPLIER")]
    [JsonProperty("INVENTORYING_SUPPLY_WAREHOUSE_BASED_PER_SUPPLIER")]
    [Display(Name = "Инвентаризация по запросу склада")]
    InventoryingSupplyWarehouseBasedPerSupplier,

    /// <summary>
    /// Входящее перемещение между складами.
    /// </summary>
    [EnumMember(Value = "MOVEMENT_SUPPLY")]
    [JsonPropertyName("MOVEMENT_SUPPLY")]
    [JsonProperty("MOVEMENT_SUPPLY")]
    [Display(Name = "Перемещение (входящее)")]
    MovementSupply,

    /// <summary>
    /// Дополнительная поставка непринятых товаров.
    /// </summary>
    [EnumMember(Value = "ADDITIONAL_SUPPLY")]
    [JsonPropertyName("ADDITIONAL_SUPPLY")]
    [JsonProperty("ADDITIONAL_SUPPLY")]
    [Display(Name = "Дополнительная поставка")]
    AdditionalSupply,

    /// <summary>
    /// Родительская заявка при мультипоставке.
    /// </summary>
    [EnumMember(Value = "VIRTUAL_DISTRIBUTION_CENTER")]
    [JsonPropertyName("VIRTUAL_DISTRIBUTION_CENTER")]
    [JsonProperty("VIRTUAL_DISTRIBUTION_CENTER")]
    [Display(Name = "Род. заявка мультипоставки")]
    VirtualDistributionCenter,

    /// <summary>
    /// Дочерняя заявка при мультипоставке.
    /// </summary>
    [EnumMember(Value = "VIRTUAL_DISTRIBUTION_CENTER_CHILD")]
    [JsonPropertyName("VIRTUAL_DISTRIBUTION_CENTER_CHILD")]
    [JsonProperty("VIRTUAL_DISTRIBUTION_CENTER_CHILD")]
    [Display(Name = "Доч. заявка мультипоставки")]
    VirtualDistributionCenterChild,

    /// <summary>
    /// Автоматическая утилизация по запросу склада.
    /// </summary>
    [EnumMember(Value = "FORCE_PLAN")]
    [JsonPropertyName("FORCE_PLAN")]
    [JsonProperty("FORCE_PLAN")]
    [Display(Name = "Автоутилизация по запросу склада")]
    ForcePlan,

    /// <summary>
    /// Утилизация непринятых товаров.
    /// </summary>
    [EnumMember(Value = "FORCE_PLAN_ANOMALY_PER_SUPPLY")]
    [JsonPropertyName("FORCE_PLAN_ANOMALY_PER_SUPPLY")]
    [JsonProperty("FORCE_PLAN_ANOMALY_PER_SUPPLY")]
    [Display(Name = "Утилизация непринятых товаров")]
    ForcePlanAnomalyPerSupply,

    /// <summary>
    /// Утилизация по запросу магазина.
    /// </summary>
    [EnumMember(Value = "PLAN_BY_SUPPLIER")]
    [JsonPropertyName("PLAN_BY_SUPPLIER")]
    [JsonProperty("PLAN_BY_SUPPLIER")]
    [Display(Name = "Утилизация по запросу магазина")]
    PlanBySupplier,

    /// <summary>
    /// Вывоз непринятых товаров.
    /// </summary>
    [EnumMember(Value = "ANOMALY_WITHDRAW")]
    [JsonPropertyName("ANOMALY_WITHDRAW")]
    [JsonProperty("ANOMALY_WITHDRAW")]
    [Display(Name = "Вывоз непринятых товаров")]
    AnomalyWithdraw,

    /// <summary>
    /// Товары, не найденные после второй инвентаризации.
    /// </summary>
    [EnumMember(Value = "FIX_LOST_INVENTORYING")]
    [JsonPropertyName("FIX_LOST_INVENTORYING")]
    [JsonProperty("FIX_LOST_INVENTORYING")]
    [Display(Name = "Потеря после 2-й инвентаризации")]
    FixLostInventorying,

    /// <summary>
    /// Товары, не найденные после первой инвентаризации.
    /// </summary>
    [EnumMember(Value = "OPER_LOST_INVENTORYING")]
    [JsonPropertyName("OPER_LOST_INVENTORYING")]
    [JsonProperty("OPER_LOST_INVENTORYING")]
    [Display(Name = "Потеря после 1-й инвентаризации")]
    OperLostInventorying,

    /// <summary>
    /// Исходящее перемещение между складами.
    /// </summary>
    [EnumMember(Value = "MOVEMENT_WITHDRAW")]
    [JsonPropertyName("MOVEMENT_WITHDRAW")]
    [JsonProperty("MOVEMENT_WITHDRAW")]
    [Display(Name = "Перемещение (исходящее)")]
    MovementWithdraw,

    /// <summary>
    /// Пересортица в большую сторону.
    /// </summary>
    [EnumMember(Value = "MISGRADING_SUPPLY")]
    [JsonPropertyName("MISGRADING_SUPPLY")]
    [JsonProperty("MISGRADING_SUPPLY")]
    [Display(Name = "Пересортица: излишек")]
    MisgradingSupply,

    /// <summary>
    /// Пересортица в меньшую сторону.
    /// </summary>
    [EnumMember(Value = "MISGRADING_WITHDRAW")]
    [JsonPropertyName("MISGRADING_WITHDRAW")]
    [JsonProperty("MISGRADING_WITHDRAW")]
    [Display(Name = "Пересортица: недостача")]
    MisgradingWithdraw,

    /// <summary>
    /// Ручная утилизация по запросу склада.
    /// </summary>
    [EnumMember(Value = "MAN_UTIL")]
    [JsonPropertyName("MAN_UTIL")]
    [JsonProperty("MAN_UTIL")]
    [Display(Name = "Ручная утилизация")]
    ManUtil
  }
}
