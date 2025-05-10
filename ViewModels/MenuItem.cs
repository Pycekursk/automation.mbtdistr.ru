using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using automation.mbtdistr.ru.Models;

namespace automation.mbtdistr.ru.ViewModels
{
  public class MenuItem
  {
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public Worker Worker { get; set; }
    public string Icon { get; set; }

    public string CSS { get; set; } = "btn btn-primary";

    public string Action { get; set; }
    public string Title { get; set; }
    public string EntityId { get; set; } = string.Empty;
  }
}
