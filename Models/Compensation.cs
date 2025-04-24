namespace automation.mbtdistr.ru.Models
{
  public class Compensation
  {
    public int Id { get; set; }
    public int ReturnId { get; set; }
    public Return Return { get; set; }

    public string Type { get; set; }     // Компенсация, Продажа, Утилизация
    public decimal? Amount { get; set; }
    public DateTime ProcessedAt { get; set; }
  }


}