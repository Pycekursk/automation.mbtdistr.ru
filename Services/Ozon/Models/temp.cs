using DevExpress.XtraEditors.Controls;

using System.Text.Json.Serialization;

namespace automation.mbtdistr.ru.Services.Ozon.Models
{
  public class temp
  {
  }
  #region Response DTOs

  /// <summary>
  /// Обёртка для тела POST-запроса /v1/returns/list
  /// </summary>
  public class ReturnsListRequest
  {
    [JsonPropertyName("filter")]
    public Filter Filter { get; set; }

    [JsonPropertyName("limit")]
    public int Limit { get; set; }

    [JsonPropertyName("last_id")]
    public long? LastId { get; set; }
  }
  /// <summary>
  /// Параметры фильтрации возвратов (по датам, статусам, идентификаторам и т.д.).
  /// </summary>
  public class Filter
  {
    [JsonPropertyName("logistic_return_date")]
    public DateRange LogisticReturnDate { get; set; }

    [JsonPropertyName("storage_tariffication_start_date")]
    public DateRange StorageTarifficationStartDate { get; set; }

    [JsonPropertyName("visual_status_change_moment")]
    public DateRange VisualStatusChangeMoment { get; set; }

    [JsonPropertyName("order_id")]
    public long? OrderId { get; set; }

    [JsonPropertyName("posting_numbers")]
    public List<string>? PostingNumbers { get; set; }

    [JsonPropertyName("product_name")]
    public string? ProductName { get; set; }

    [JsonPropertyName("offer_id")]
    public string? OfferId { get; set; }

    [JsonPropertyName("visual_status_name")]
    public string? VisualStatusName { get; set; }

    [JsonPropertyName("warehouse_id")]
    public int? WarehouseId { get; set; }

    [JsonPropertyName("barcode")]
    public string? Barcode { get; set; }

    [JsonPropertyName("return_schema")]
    public string? ReturnSchema { get; set; }
  }

  /// <summary>
  /// Модель ответа от API с коллекцией возвратов и флагом постраничности.
  /// </summary>
  public class ReturnsListResponse
  {
    [JsonPropertyName("returns")]
    public List<ReturnInfo> Returns { get; set; }

    [JsonPropertyName("has_next")]
    public bool HasNext { get; set; }
  }

  /// <summary>
  /// Детальная информация о конкретном возврате.
  /// </summary>
  public class ReturnInfo
  {
    [JsonPropertyName("exemplars")]
    public List<Exemplar>? Exemplars { get; set; }

    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("company_id")]
    public long CompanyId { get; set; }

    [JsonPropertyName("return_reason_name")]
    public string? ReturnReasonName { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("schema")]
    public string? Schema { get; set; }

    [JsonPropertyName("order_id")]
    public long OrderId { get; set; }

    [JsonPropertyName("order_number")]
    public string? OrderNumber { get; set; }

    [JsonPropertyName("place")]
    public Place? Place { get; set; }

    [JsonPropertyName("target_place")]
    public Place? TargetPlace { get; set; }

    [JsonPropertyName("storage")]
    public StorageInfo? Storage { get; set; }

    [JsonPropertyName("product")]
    public ProductInfo? Product { get; set; }

    [JsonPropertyName("logistic")]
    public LogisticInfo? Logistic { get; set; }

    [JsonPropertyName("visual")]
    public VisualInfo? Visual { get; set; }

    [JsonPropertyName("additional_info")]
    public AdditionalInfo? AdditionalInfo { get; set; }

    [JsonPropertyName("source_id")]
    public long SourceId { get; set; }

    [JsonPropertyName("posting_number")]
    public string? PostingNumber { get; set; }

    [JsonPropertyName("clearing_id")]
    public long? ClearingId { get; set; }

    [JsonPropertyName("return_clearing_id")]
    public long? ReturnClearingId { get; set; }
  }

  /// <summary>
  /// Минимальная информация об экземпляре возврата.
  /// </summary>
  public class Exemplar
  {
    [JsonPropertyName("id")]
    public long Id { get; set; }
  }

  /// <summary>
  /// Описание склада или пункта, где происходит возврат.
  /// </summary>
  public class Place
  {
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("address")]
    public string? Address { get; set; }
  }

  /// <summary>
  /// Информация о хранении возврата: суммы, даты и прогнозы использования склада.
  /// </summary>
  public class StorageInfo
  {
    [JsonPropertyName("sum")]
    public Money Sum { get; set; }

    [JsonPropertyName("tariffication_first_date")]
    public DateTime? TarifficationFirstDate { get; set; }

    [JsonPropertyName("tariffication_start_date")]
    public DateTime? TarifficationStartDate { get; set; }

    [JsonPropertyName("arrived_moment")]
    public DateTime? ArrivedMoment { get; set; }

    [JsonPropertyName("days")]
    public int Days { get; set; }

    [JsonPropertyName("utilization_sum")]
    public Money UtilizationSum { get; set; }

    [JsonPropertyName("utilization_forecast_date")]
    public DateTime? UtilizationForecastDate { get; set; }
  }

  /// <summary>
  /// Модель валюты и суммы для денежных полей.
  /// </summary>
  public class Money
  {
    [JsonPropertyName("currency_code")]
    public string CurrencyCode { get; set; }

    [JsonPropertyName("price")]
    public float Price { get; set; }
  }

  /// <summary>
  /// Информация о товаре в возврате: SKU, наименование, цены и комиссия.
  /// </summary>
  public class ProductInfo
  {
    [JsonPropertyName("sku")]
    public long Sku { get; set; }

    [JsonPropertyName("offer_id")]
    public string? OfferId { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("price")]
    public Money Price { get; set; }

    [JsonPropertyName("price_without_commission")]
    public Money PriceWithoutCommission { get; set; }

    [JsonPropertyName("commission_percent")]
    public float CommissionPercent { get; set; }

    [JsonPropertyName("commission")]
    public Money Commission { get; set; }

    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }
  }

  /// <summary>
  /// Логистические метки и времена обработки возврата.
  /// </summary>
  public class LogisticInfo
  {
    [JsonPropertyName("technical_return_moment")]
    public DateTime? TechnicalReturnMoment { get; set; }

    [JsonPropertyName("final_moment")]
    public DateTime? FinalMoment { get; set; }

    [JsonPropertyName("cancelled_with_compensation_moment")]
    public DateTime? CancelledWithCompensationMoment { get; set; }

    [JsonPropertyName("return_date")]
    public DateTime? ReturnDate { get; set; }

    [JsonPropertyName("barcode")]
    public string? Barcode { get; set; }
  }

  /// <summary>
  /// Состояние возврата в визуальном представлении: идентификатор, отображаемое имя и системное имя.
  /// </summary>
  public class VisualInfo
  {
    [JsonPropertyName("status")]
    public StatusInfo Status { get; set; }

    [JsonPropertyName("change_moment")]
    public DateTime? ChangeMoment { get; set; }
  }

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
    public string? SysName { get; set; }
  }

  /// <summary>
  /// Дополнительные флаги: открыт ли возврат физически и специальный экономичный режим.
  /// </summary>
  public class AdditionalInfo
  {
    [JsonPropertyName("is_opened")]
    public bool IsOpened { get; set; }

    [JsonPropertyName("is_super_econom")]
    public bool IsSuperEconom { get; set; }
  }

  #endregion
}
