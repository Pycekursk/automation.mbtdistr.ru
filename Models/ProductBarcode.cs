using Newtonsoft.Json;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace automation.mbtdistr.ru.Models
{
  /// <summary>
  /// Штрихкод товара.
  /// </summary>
  public class ProductBarcode
  {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>
    /// Значение штрихкода.
    /// </summary>
    [Required]
    public string Barcode { get; set; } = string.Empty;

    /// <summary>
    /// Внешний ключ на товар.
    /// </summary>
    [ForeignKey(nameof(Product))]
    public int ProductId { get; set; }

    /// <summary>
    /// Навигационное свойство к товару.
    /// </summary>
    public virtual Product Product { get; set; } = null!;
  }
}
