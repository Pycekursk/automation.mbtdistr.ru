using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace automation.mbtdistr.ru.Models
{
  public class Compensation
  {
    public Compensation(int returnId)
    {
      ProcessedAt = DateTime.UtcNow;
      ReturnId = returnId;
    }

    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int ReturnId { get; set; }
    public Return Return { get; set; }

    public string Type { get; set; } = string.Empty; // Тип компенсации: Возврат, Замена, Отказ от возврата
    public decimal? Amount { get; set; }
    public DateTime? ProcessedAt { get; set; } // Дата обработки
  }


}