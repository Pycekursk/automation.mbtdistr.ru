using System.Text.Json.Serialization;

namespace automation.mbtdistr.ru.Services.Ozon.Models
{
  #region Response DTOs

  /// <summary>
  /// Минимальная информация об экземпляре возврата.
  /// </summary>
  public class Exemplar
  {
    [JsonPropertyName("id")]
    public long Id { get; set; }
  }

  #endregion
}
