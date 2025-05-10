namespace automation.mbtdistr.ru.ViewModels
{
  /// <summary>
  /// Универсальная модель страницы/partial, которая возвращает список сущностей T
  /// и набор выбранных фильтров (для восстановления состояния при серверной отрисовке).
  /// </summary>
  /// <typeparam name="T">Тип элемента списка.</typeparam>
  public class ListViewModel<T>
  {
    /// <summary>
    /// Название модели
    /// </summary>
    public string? Title { get; set; } // заголовок страницы/раздела

    /// <summary>
    /// Сами объекты, которые рендерим в таблице (или любом другом компоненте).
    /// </summary>
    public List<T> Items { get; set; } = new();

    /// <summary>
    /// Словарь: ключ — имя фильтра (FilterName), 
    /// значение — массив выбранных строковых значений.
    /// </summary>
    public Dictionary<string, string[]> SelectedFilters { get; set; } = new();

    /// <summary>
    /// Дополнительный слой: заранее подготовленные ViewModels для кнопок-фильтров.
    /// Ключ — FilterName, значение — соответствующий VM с Options.
    /// </summary>
    public Dictionary<string, FilterButtonsViewModel>? FilterButtonModels { get; set; }
  }
}
