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

    [Newtonsoft.Json.JsonIgnore]
    public ICollection<Return>? Returns { get; set; } = new List<Return>();
  }
}