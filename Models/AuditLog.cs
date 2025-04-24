namespace automation.mbtdistr.ru.Models
{
  public class AuditLog
  {
    public int Id { get; set; }
    public int? UserId { get; set; }
    public Worker User { get; set; }

    public string Action { get; set; }
    public DateTime Timestamp { get; set; }
  }


}