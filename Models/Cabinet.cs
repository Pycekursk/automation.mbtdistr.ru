using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Telegram.Bot.Types;

namespace automation.mbtdistr.ru.Models
{
  public class Cabinet
  {
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    private string marketplace;
    public string Marketplace { get => marketplace.ToUpper(); set => marketplace = value.ToUpper(); }  // "Wildberries", "Ozon" и т.д.

    public string Name { get; set; }         // например, "Основной", "Резервный" и т.п.
    public CabinetSettings Settings { get; set; } = new CabinetSettings();

    public ICollection<Worker> AssignedWorkers { get; set; }
            = new List<Worker>();
  }
}