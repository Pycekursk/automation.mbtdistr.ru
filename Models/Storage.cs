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

    /// <summary>
    /// Количество дней нахождения товара на складе.
    /// </summary>
    [Display(Name = "Дней")]
    [JsonProperty("days")]
    [Range(0, 365, ErrorMessage = "Количество дней должно быть от 0 до 365.")]
    public int Days { get; set; }

    /// <summary>
    /// Сумма к оплате за хранение товара на складе.
    /// </summary>
    [Display(Name = "Сумма")]
    [JsonProperty("price")]
    [Range(0, double.MaxValue, ErrorMessage = "Сумма должна быть неотрицательной.")]
    public double? Price { get; set; }
  }
}
