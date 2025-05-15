using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace automation.mbtdistr.ru.Services.YandexMarket.Models
{
  /// <summary>
  /// Изображение товара, полученное из возврата.
  /// Маркет отдаёт двоичный поток ( Content-Type: application/octet-stream ) :contentReference[oaicite:0]{index=0}:contentReference[oaicite:1]{index=1}.  
  /// Для удобства дальнейшей работы данные конвертируются в Base64-строку.
  /// </summary>
  public class YMReturnPhoto
  {
    /// <summary>Тип содержимого (по умолчанию image/jpeg или image/png).</summary>
    [Display(Name = "Тип содержимого")]
    [JsonProperty("contentType")]
    [Required]
    public string ContentType { get; set; }

    /// <summary>Данные изображения в кодировке Base64.</summary>
    [Display(Name = "Данные изображения (Base64)")]
    [JsonProperty("imageData")]
    [Required]
    public string ImageData { get; set; }
  }
}
