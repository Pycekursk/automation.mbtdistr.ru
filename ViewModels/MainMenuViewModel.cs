using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace automation.mbtdistr.ru.ViewModels
{
  public class MainMenuViewModel
  {
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public string GreetingMessage { get; set; }
    public int? WorkerId { get; set; }
    public List<MenuItem> Menu { get; set; } = new List<MenuItem>();
    public string HtmlContent { get; set; } = string.Empty; //HtmlContent для отображения на странице
  }
}
