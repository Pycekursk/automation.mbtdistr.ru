using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace automation.mbtdistr.ru.Models
{
  /// <summary>
  /// Класс обьекта склада/пвз
  /// </summary>
  public class Warehouse
  {
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public string? ExternalId { get; set; }
    public string? Name { get; set; }
    public Address? Address { get; set; }
    public string? Phone { get; set; }

    public string? Service { get; set; }

    // Возвраты, которые сейчас на этом складе
    [Newtonsoft.Json.JsonIgnore, System.Text.Json.Serialization.JsonIgnore]
    public ICollection<Return> CurrentReturns { get; set; } = new List<Return>();

    // Возвраты, направленные на этот склад
    [Newtonsoft.Json.JsonIgnore, System.Text.Json.Serialization.JsonIgnore]
    public ICollection<Return> DestinationReturns { get; set; } = new List<Return>();

    [ForeignKey(nameof(Cabinet))]
    public int? CabinetId { get; set; }

    [Newtonsoft.Json.JsonIgnore]
    public Cabinet? Cabinet { get; set; }
  }
}