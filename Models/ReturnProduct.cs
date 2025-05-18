using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace automation.mbtdistr.ru.Models
{
  public class ReturnProduct
  {
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Sku { get; set; }

    public string? OfferId { get; set; }

    public List<ReturnImage>? Images { get; set; }

    public int Count { get; set; }

    [ForeignKey("Return")]
    public int ReturnId { get; set; }

    [Newtonsoft.Json.JsonIgnore]
    public Return Return { get; set; }

    public string? Url { get; set; }
  }
}