using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using System.Runtime.Serialization;

using JsonConverter = Newtonsoft.Json.JsonConverter;

namespace automation.mbtdistr.ru.Services.YandexMarket.Models
{
  /// <summary>
  /// Тип документа по заявке.
  /// </summary>
  [JsonConverter(typeof(StringEnumConverter))]
  public enum YMSupplyRequestDocumentType
  {
    // Документы, которые загружает магазин
    /// <summary>Список товаров.</summary>
    [EnumMember(Value = "SUPPLY")]
    SUPPLY,

    /// <summary>Список товаров в дополнительной поставке.</summary>
    [EnumMember(Value = "ADDITIONAL_SUPPLY")]
    ADDITIONAL_SUPPLY,

    /// <summary>Список товаров в мультипоставке.</summary>
    [EnumMember(Value = "VIRTUAL_DISTRIBUTION_CENTER_SUPPLY")]
    VIRTUAL_DISTRIBUTION_CENTER_SUPPLY,

    /// <summary>Список товаров для утилизации.</summary>
    [EnumMember(Value = "TRANSFER")]
    TRANSFER,

    /// <summary>Список товаров для вывоза.</summary>
    [EnumMember(Value = "WITHDRAW")]
    WITHDRAW,

    // Поставка товаров
    /// <summary>Ошибки по товарам в поставке.</summary>
    [EnumMember(Value = "VALIDATION_ERRORS")]
    VALIDATION_ERRORS,

    /// <summary>Ярлыки для грузомест.</summary>
    [EnumMember(Value = "CARGO_UNITS")]
    CARGO_UNITS,

    // Дополнительная поставка и непринятые товары
    /// <summary>Товары, которые подходят для дополнительной поставки.</summary>
    [EnumMember(Value = "ADDITIONAL_SUPPLY_ACCEPTABLE_GOODS")]
    ADDITIONAL_SUPPLY_ACCEPTABLE_GOODS,

    /// <summary>Вывоз непринятых товаров.</summary>
    [EnumMember(Value = "ADDITIONAL_SUPPLY_UNACCEPTABLE_GOODS")]
    ADDITIONAL_SUPPLY_UNACCEPTABLE_GOODS,

    // Маркировка товаров
    /// <summary>Входящий УПД.</summary>
    [EnumMember(Value = "INBOUND_UTD")]
    INBOUND_UTD,

    /// <summary>Исходящий УПД.</summary>
    [EnumMember(Value = "OUTBOUND_UTD")]
    OUTBOUND_UTD,

    /// <summary>Коды маркировки товаров.</summary>
    [EnumMember(Value = "IDENTIFIERS")]
    IDENTIFIERS,

    /// <summary>Принятые товары с кодами маркировки.</summary>
    [EnumMember(Value = "CIS_FACT")]
    CIS_FACT,

    /// <summary>Товары, для которых нужна маркировка.</summary>
    [EnumMember(Value = "ITEMS_WITH_CISES")]
    ITEMS_WITH_CISES,

    /// <summary>Отчет по маркированным товарам для вывоза со склада.</summary>
    [EnumMember(Value = "REPORT_OF_WITHDRAW_WITH_CISES")]
    REPORT_OF_WITHDRAW_WITH_CISES,

    /// <summary>Маркированные товары, которые приняты после вторичной приемки.</summary>
    [EnumMember(Value = "SECONDARY_ACCEPTANCE_CISES")]
    SECONDARY_ACCEPTANCE_CISES,

    /// <summary>Принятые товары с регистрационным номером партии (РНПТ).</summary>
    [EnumMember(Value = "RNPT_FACT")]
    RNPT_FACT,

    // Акты
    /// <summary>Акт возврата.</summary>
    [EnumMember(Value = "ACT_OF_WITHDRAW")]
    ACT_OF_WITHDRAW,

    /// <summary>Акт изъятия непринятого товара.</summary>
    [EnumMember(Value = "ANOMALY_CONTAINERS_WITHDRAW_ACT")]
    ANOMALY_CONTAINERS_WITHDRAW_ACT,

    /// <summary>Акт списания с ответственного хранения.</summary>
    [EnumMember(Value = "ACT_OF_WITHDRAW_FROM_STORAGE")]
    ACT_OF_WITHDRAW_FROM_STORAGE,

    /// <summary>Акт приема-передачи.</summary>
    [EnumMember(Value = "ACT_OF_RECEPTION_TRANSFER")]
    ACT_OF_RECEPTION_TRANSFER,

    /// <summary>Акт о расхождениях.</summary>
    [EnumMember(Value = "ACT_OF_DISCREPANCY")]
    ACT_OF_DISCREPANCY,

    /// <summary>Акт вторичной приемки.</summary>
    [EnumMember(Value = "SECONDARY_RECEPTION_ACT")]
    SECONDARY_RECEPTION_ACT
  }
  
}
