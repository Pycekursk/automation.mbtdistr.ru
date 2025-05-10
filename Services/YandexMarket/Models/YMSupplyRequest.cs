using automation.mbtdistr.ru.Models;

using Newtonsoft.Json;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
//using System.Text.Json.Serialization;

//using System.Text.Json.Serialization;


namespace automation.mbtdistr.ru.Services.YandexMarket.Models
{
  /// <summary>
  /// Информация о заявке на поставку, вывоз или утилизацию.
  /// </summary>
  public class YMSupplyRequest
  {
    /// <summary>
    /// Счетчики: количество товаров, коробок и палет в заявке.
    /// </summary>
    [JsonProperty("counters")]
    [Display(Name = "Счетчики")]
    public YMSupplyRequestCounters? Counters { get; set; }

    [ForeignKey("ExternalId"), Key]
    public long? Id { get; set; }

    /// <summary>
    /// Идентификаторы заявки.
    /// </summary>
    [JsonProperty("id")]
    [Display(Name = "Идентификаторы заявки")]
    public YMSupplyRequestId? ExternalId { get; set; }

    /// <summary>
    /// Статус заявки.
    /// </summary>
    [JsonProperty("status")]
    [Display(Name = "Статус заявки")]
    public YMSupplyRequestStatusType Status { get; set; }

    /// <summary>
    /// Подтип заявки.
    /// </summary>
    [JsonProperty("subtype")]
    [Display(Name = "Подтип заявки")]
    public YMSupplyRequestSubType Subtype { get; set; }

    /// <summary>
    /// Информация о основном складе или ПВЗ.
    /// </summary>
    [JsonProperty("targetLocation")]
    [Display(Name = "Основной склад/ПВЗ")]
    public YMSupplyRequestLocation? TargetLocation { get; set; }

    /// <summary>
    /// Тип заявки.
    /// </summary>
    [JsonProperty("type")]
    [Display(Name = "Тип заявки")]
    public YMSupplyRequestType Type { get; set; }

    /// <summary>
    /// Дата и время последнего обновления заявки.
    /// </summary>
    [JsonProperty("updatedAt")]
    [Display(Name = "Дата и время последнего обновления заявки")]
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Ссылки на дочерние заявки.
    /// </summary>
    [JsonProperty("childrenLinks")]
    [Display(Name = "Ссылки на дочерние заявки"), NotMapped]
    public List<YMSupplyRequestReference>? ChildrenLinks { get; set; }

    /// <summary>
    /// Ссылка на родительскую заявку.
    /// </summary>
    [JsonProperty("parentLink")]
    [Display(Name = "Ссылка на родительскую заявку"), NotMapped]
    public YMSupplyRequestReference? ParentLink { get; set; }

    /// <summary>
    /// Информация о транзитном складе или ПВЗ.
    /// </summary>
    [JsonProperty("transitLocation")]
    [Display(Name = "Транзитный склад/ПВЗ")]
    public YMSupplyRequestLocation? TransitLocation { get; set; }

    /// <summary>
    /// Кабинет которому принадлежит заявка
    /// </summary>
    [JsonIgnore, ForeignKey("Cabinet")]
    public int? CabinetId { get; set; }

    /// <summary>
    /// Кабинет которому принадлежит заявка
    /// </summary>
    [JsonIgnore]
    public Cabinet? Cabinet { get; set; }
  }
}
