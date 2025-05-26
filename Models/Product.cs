using Newtonsoft.Json;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace automation.mbtdistr.ru.Models
{
  /// <summary>
  /// Товар.
  /// </summary>
  public class Product
  {
    /// <summary>
    /// Идентификатор товара в базе данных.
    /// </summary>
    [JsonProperty("id"), Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>
    /// Внешний идентификатор предложения.
    /// </summary>
    public string OfferId { get; set; } = string.Empty;

    /// <summary>
    /// Наименование товара.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Связь к таблице штрихкодов.
    /// </summary>
    public virtual ICollection<ProductBarcode> Barcodes { get; set; } = new List<ProductBarcode>();
  }
}
