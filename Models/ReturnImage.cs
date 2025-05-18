using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace automation.mbtdistr.ru.Models
{
  public class ReturnImage
  {
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [ForeignKey("ReturnProduct")]
    public int ReturnProductId { get; set; }

    [Newtonsoft.Json.JsonIgnore]
    public ReturnProduct ReturnProduct { get; set; }

    public string Url { get; set; } = string.Empty;
  }
}