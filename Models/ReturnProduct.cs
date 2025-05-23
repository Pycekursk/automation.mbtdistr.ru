using automation.mbtdistr.ru.Services.YandexMarket.Models;

using Microsoft.EntityFrameworkCore;

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

    public Price Price { get; set; }

    public string? OfferId { get; set; }

    public List<ReturnImage>? Images { get; set; }

    public int Count { get; set; }

    [ForeignKey("Return")]
    public int ReturnId { get; set; }

    [Newtonsoft.Json.JsonIgnore, System.Text.Json.Serialization.JsonIgnore]
    public Return Return { get; set; }

    public string? Url { get; set; }
  }

  public class Price
  {
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [ForeignKey("ReturnProduct")]
    public int ReturnProductId { get; set; }

    [Newtonsoft.Json.JsonIgnore]
    public ReturnProduct ReturnProduct { get; set; }


    public decimal? Amount { get; set; }
    public string? Currency { get; set; }
  }
}