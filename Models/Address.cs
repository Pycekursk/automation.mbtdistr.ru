using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace automation.mbtdistr.ru.Models
{
  public class Address
  {
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public string? Street { get; set; }
    public string? House { get; set; }
    public string? Office { get; set; }
    public string? ZipCode { get; set; }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }

    [ForeignKey("Warehouse")]
    public int WarehouseId { get; set; }

    [System.Text.Json.Serialization.JsonIgnore, Newtonsoft.Json.JsonIgnore]
    public Warehouse Warehouse { get; set; }
  }
}