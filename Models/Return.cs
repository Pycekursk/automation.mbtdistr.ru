namespace automation.mbtdistr.ru.Models
{
  public class Return
  {
    public int Id { get; set; }
    public int UserId { get; set; } // кто оформил возврат
    public Worker User { get; set; }

    public int CabinetId { get; set; } // кабинет/бренд/ООО
    public Cabinet Cabinet { get; set; }

    public string Marketplace { get; set; } // Ozon, WB, Яндекс и т.д.
    public string ReturnStatus { get; set; } // Новый, В процессе, Завершен
    public DateTime CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }

    public Compensation Compensation { get; set; }
  }


}