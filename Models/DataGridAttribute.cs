using automation.mbtdistr.ru.Data;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace automation.mbtdistr.ru.Models
{
  public class DataGridAttribute : Attribute
  {
    bool _isVisible = true;

    public bool IsVisible
    {
      get => _isVisible;
      set => _isVisible = value;
    }

    public DataGridAttribute(bool isVisible)
    {
      IsVisible = isVisible;
    }
  }
}
