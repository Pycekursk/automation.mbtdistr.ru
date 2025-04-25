﻿using System.Text.Json.Serialization;

namespace automation.mbtdistr.ru.Services.Ozon.Models
{
  /// <summary>
  /// Логистические метки и времена обработки возврата.
  /// </summary>
  public class LogisticInfo
  {
    [JsonPropertyName("technical_return_moment")]
    public DateTime? TechnicalReturnMoment { get; set; }

    [JsonPropertyName("final_moment")]
    public DateTime? FinalMoment { get; set; }

    [JsonPropertyName("cancelled_with_compensation_moment")]
    public DateTime? CancelledWithCompensationMoment { get; set; }

    [JsonPropertyName("return_date")]
    public DateTime? ReturnDate { get; set; }

    [JsonPropertyName("barcode")]
    public string? Barcode { get; set; }
  }
}
