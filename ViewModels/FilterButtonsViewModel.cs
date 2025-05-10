namespace automation.mbtdistr.ru.ViewModels
{
  /// <summary>
  /// Набор кнопок-фильтров для одного enum-свойства.
  /// </summary>
  public class FilterButtonsViewModel
  {
    /// <summary>
    /// Имя свойства/параметра в query-string (e.g. "SupplyRequestStatus").
    /// </summary>
    public string FilterName { get; set; } = null!;

    /// <summary>
    /// Список всех опций (по одному элементу enum) для отрисовки кнопок.
    /// </summary>
    public List<FilterButtonOption> Options { get; set; } = new();

    /// <summary>
    /// Если у вас не один фильтр на странице, можно хранить порядок или явный id, 
    /// но в нашем AJAX-сценарии он не нужен, т.к. используется только FilterName.
    /// </summary>
    // public string? FormId { get; set; }
  }
}
