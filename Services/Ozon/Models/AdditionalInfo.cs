using System.Text.Json.Serialization;

namespace automation.mbtdistr.ru.Services.Ozon.Models
{
  #region Response DTOs

  /// <summary>
  /// Дополнительные флаги: открыт ли возврат физически и специальный экономичный режим.
  /// </summary>
  public class AdditionalInfo
  {
    [JsonPropertyName("is_opened")]
    public bool IsOpened { get; set; }

    [JsonPropertyName("is_super_econom")]
    public bool IsSuperEconom { get; set; }
  }

  #endregion
}
