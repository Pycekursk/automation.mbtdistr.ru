using automation.mbtdistr.ru.Models;

using Newtonsoft.Json;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace automation.mbtdistr.ru.Services.YandexMarket.Models
{
  /// <summary>
  /// Информация о заявке на поставку, вывоз или утилизацию.
  /// </summary>
  public class YMSupplyRequest
  {
    [Key, DataGrid(false)]
    public long Id { get; set; }

    [ForeignKey(nameof(ExternalId)), JsonIgnore, DataGrid(false)]
    public long ExternalIdId { get; set; }

    /// <summary>Идентификаторы заявки.</summary>
    [JsonProperty("id")]
    [Display(Name = "Идентификаторы заявки"), DataGrid(false)]
    public YMSupplyRequestId? ExternalId { get; set; }

    [JsonProperty("status")]
    [Display(Name = "Статус заявки")]
    public YMSupplyRequestStatusType Status { get; set; }

    [JsonProperty("subtype")]
    [Display(Name = "Подтип заявки")]
    public YMSupplyRequestSubType Subtype { get; set; }

    [JsonProperty("type")]
    [Display(Name = "Тип заявки")]
    public YMSupplyRequestType Type { get; set; }

    [JsonProperty("updatedAt")]
    [Display(Name = "Обновление заявки")]
    public DateTime UpdatedAt { get; set; }

    [JsonProperty("counters")]
    [Display(Name = "Счетчики")]
    public YMSupplyRequestCounters? Counters { get; set; }

    /// <summary>Продукты</summary>
    [JsonProperty("items")]
    [Display(Name = "Продукты")]
    public ICollection<YMSupplyRequestItem>? Items { get; set; }

    // ─── Target Location ───────────────────────────────

    [JsonProperty("targetLocation")]
    [Display(Name = "Основной склад/ПВЗ")]
    public YMSupplyRequestLocation TargetLocation { get; set; }

    [ForeignKey(nameof(TargetLocation)), DataGrid(false)]
    public long TargetLocationServiceId { get; set; }

    // ─── Transit Location ──────────────────────────────

    [JsonProperty("transitLocation")]
    [Display(Name = "Транзитный склад/ПВЗ")]
    public YMSupplyRequestLocation? TransitLocation { get; set; }

    [ForeignKey(nameof(TransitLocation)), DataGrid(false)]
    public long? TransitLocationServiceId { get; set; }

    // ─── Кабинет ───────────────────────────────────────

    [DataGrid(false)]
    public int? CabinetId { get; set; }

    [Display(Name = "Кабинет")]
    public Cabinet? Cabinet { get; set; }

    // ─── Виртуальные связи (не сохраняются) ────────────

    [JsonProperty("childrenLinks")]
    [Display(Name = "Ссылки на дочерние заявки"), InverseProperty(nameof(YMSupplyRequestReference.Request))]
    public List<YMSupplyRequestReference>? ChildrenLinks { get; set; }

    [JsonProperty("parentLink")]
    [Display(Name = "Ссылка на родительскую заявку"), InverseProperty(nameof(YMSupplyRequestReference.RelatedRequest))]
    public YMSupplyRequestReference? ParentLink { get; set; }
  }
}
