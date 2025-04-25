using System.ComponentModel.DataAnnotations;

namespace automation.mbtdistr.ru.Models
{
  public class Internal
  {
    public static string GetEnumDisplayName(Enum enumValue)
    {
      var display = enumValue.GetType()
          .GetField(enumValue.ToString())
          ?.GetCustomAttributes(typeof(DisplayAttribute), false)
          .FirstOrDefault() as DisplayAttribute;

      return display?.Name ?? enumValue.ToString();
    }
  }
}
