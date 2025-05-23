using Newtonsoft.Json;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace automation.mbtdistr.ru.Models
{
  public class Product
  {
    [JsonProperty("id"), Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public string OfferId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    /// <summary>Связь к таблице штрихкодов</summary>
    public virtual ICollection<ProductBarcode> Barcodes { get; set; } = new List<ProductBarcode>();

  }

  /// <summary>Штрихкод товара</summary>
  public class ProductBarcode
  {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public string Barcode { get; set; } = string.Empty;

    // Внешний ключ на Product
    [ForeignKey(nameof(Product))]
    public int ProductId { get; set; }
    public virtual Product Product { get; set; } = null!;
  }
}
