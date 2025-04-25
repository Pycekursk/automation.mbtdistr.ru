
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace automation.mbtdistr.ru.Services.Ozon.Models
{
  public class DateRange
  {
    [JsonPropertyName("time_from"), Display(Name = "С")]
    public DateTime From { get; set; }

    [JsonPropertyName("time_to"), Display(Name = "По")]
    public DateTime To { get; set; }
  }
}
