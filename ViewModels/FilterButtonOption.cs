namespace automation.mbtdistr.ru.ViewModels
{
  /// <summary>
  /// Одна кнопка-опция фильтра: её строковое значение, подпись и состояние.
  /// </summary>
  public class FilterButtonOption
  {
    /// <summary>Строковое представление значения enum (ToString())</summary>
    public string Value { get; set; } = null!;

    /// <summary>Подпись на кнопке (например DisplayNameAttribute или ToString())</summary>
    public string DisplayName { get; set; } = null!;

    /// <summary>Выбран ли сейчас этот фильтр</summary>
    public bool Selected { get; set; }
  }
}
