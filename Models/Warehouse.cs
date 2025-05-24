using Newtonsoft.Json;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace automation.mbtdistr.ru.Models
{
  /// <summary>
  /// Класс обьекта склада/пвз
  /// </summary>
  public class Warehouse
  {
    /// <summary>
    /// Идентификатор склада в базе данных.
    /// </summary>
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity), DataGrid(false)]
    public int Id { get; set; }

    /// <summary>
    /// Идентификатор склада или ПВЗ в системе площадки.
    /// </summary>
    [Display(Name = "Внешний ID")]
    public string? ExternalId { get; set; }

    /// <summary>
    /// Название склада или ПВЗ.
    /// </summary>
    [Display(Name = "Название")]
    public string? Name { get; set; }

    /// <summary>
    /// Адрес склада или ПВЗ.
    /// </summary>
    [Display(Name = "Адрес")]
    [JsonProperty("address")]
    public Address? Address { get; set; }

    /// <summary>
    /// Телефон для связи с ПВЗ или складом
    /// </summary>
    [Display(Name = "Телефон")]
    [JsonProperty("phone")]
    public string? Phone { get; set; }

    /// <summary>
    /// Площадка, на которой находится склад (например, Wildberries, Ozon и т.д.)
    /// </summary>
    [Display(Name = "Площадка")]
    [JsonProperty("platform")]
    public string? Service { get; set; }


    // Возвраты, которые сейчас на этом складе
    [Newtonsoft.Json.JsonIgnore, System.Text.Json.Serialization.JsonIgnore]
    public ICollection<Return> CurrentReturns { get; set; } = new List<Return>();

    // Возвраты, направленные на этот склад
    [Newtonsoft.Json.JsonIgnore, System.Text.Json.Serialization.JsonIgnore]
    public ICollection<Return> DestinationReturns { get; set; } = new List<Return>();

    /// <summary>
    /// Внешний ключ на кабинет, к которому относится склад.
    /// </summary>
    [ForeignKey(nameof(Cabinet)), DataGrid(false)]
    public int? CabinetId { get; set; }

    /// <summary>
    /// Навигационное свойство для кабинета, к которому относится склад.
    /// </summary>
    [Newtonsoft.Json.JsonIgnore, System.Text.Json.Serialization.JsonIgnore, DataGrid(false)]
    public Cabinet? Cabinet { get; set; }
  }
}