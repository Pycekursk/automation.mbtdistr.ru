namespace automation.mbtdistr.ru.Services.Wildberries.Models
{
  /// <summary>
  /// Ответ на запрос списка заказов Wildberries.
  /// </summary>
  public class WBOrdersListResponse
  {
    /// <summary>Список заказов Wildberries.</summary>
    public List<WBOrder> Orders { get; set; } = new List<WBOrder>();
  }
}
