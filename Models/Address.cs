using Microsoft.EntityFrameworkCore;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace automation.mbtdistr.ru.Models
{
  [Owned]
  public class Address
  {
    public string? Country { get; set; }
    public string? City { get; set; }
    public string? Street { get; set; }
    public string? House { get; set; }
    public string? Office { get; set; }
    public string? ZipCode { get; set; }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }

    public string? FullAddress { get; set; }
  }
}