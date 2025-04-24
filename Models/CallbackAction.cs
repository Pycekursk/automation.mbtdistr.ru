namespace automation.mbtdistr.ru.Models
{
  public class CallbackAction
  {
    public int Id { get; set; }
    public string TelegramId { get; set; }    // от кого кнопка
    public string ActionType { get; set; }    // "SelectCabinet", "ProcessReturn" и т.п.
    public int TargetId { get; set; }    // id объекта, над которым действие
    public DateTime CreatedAt { get; set; }
  }
}
