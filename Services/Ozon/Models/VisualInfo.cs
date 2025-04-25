using System.Text.Json.Serialization;

namespace automation.mbtdistr.ru.Services.Ozon.Models
{
  /// <summary>
  /// Состояние возврата в визуальном представлении: идентификатор, отображаемое имя и системное имя.
  /// </summary>
  public class VisualInfo
  {
    [JsonPropertyName("status")]
    public StatusInfo Status { get; set; }

    [JsonPropertyName("change_moment")]
    public DateTime? ChangeMoment { get; set; }
  }

  
}
