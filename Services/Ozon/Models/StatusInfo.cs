using System.Text.Json.Serialization;

namespace automation.mbtdistr.ru.Services.Ozon.Models
{
  /// <summary>
  /// Детали статуса: ID, отображаемое и системное имя.
  /// </summary>
  public class StatusInfo
  {
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("display_name")]
    public string? DisplayName { get; set; }

    [JsonPropertyName("sys_name")]
    public string? SysName { get; set; }
  }

  
}
