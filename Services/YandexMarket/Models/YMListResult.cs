namespace automation.mbtdistr.ru.Services.YandexMarket.Models
{
  /// <summary>
  /// Список обьектов от API Yandex Market.
  /// </summary>
  public class YMListResult<T>
  {
    /// <summary>
    /// Информация о пагинации.
    /// </summary>
    public YMPaging Paging { get; set; } = new();
    /// <summary>
    /// Список элементов.
    /// </summary>
    public List<T> Items { get; set; } = new();
  }

}
