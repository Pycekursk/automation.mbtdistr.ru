using Microsoft.EntityFrameworkCore;

using Newtonsoft.Json;

using System.ComponentModel.DataAnnotations;

namespace automation.mbtdistr.ru.Models
{
  [Owned]
  public class Storage
  {
    [Display(Name = "Прогноз утилизации")]
    [JsonProperty("utilizationForecastDate")]
    public DateTime? UtilizationForecastDate { get; set; }

    [Display(Name = "Дата прибытия")]
    [JsonProperty("arrivedDate")]
    public DateTime? ArrivedDate { get; set; }
  }
}
