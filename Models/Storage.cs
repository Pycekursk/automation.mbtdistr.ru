using Microsoft.EntityFrameworkCore;

namespace automation.mbtdistr.ru.Models
{
  [Owned]
  public class Storage
  {
    public DateTime? UtilizationForecastDate { get; set; }

    public DateTime? ArrivedDate { get; set; }


  }
}
