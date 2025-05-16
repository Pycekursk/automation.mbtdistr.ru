using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

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

    [System.Text.Json.Serialization.JsonIgnore, Newtonsoft.Json.JsonIgnore]
    public ICollection<Worker> AssignedWorkers { get; set; }
            = new List<Worker>();

    public ICollection<Return>? Returns { get; set; }

    public override string ToString()
    {
      StringBuilder sb = new StringBuilder();
      sb.AppendLine($"{Name}");
      sb.AppendLine($"Marketplace: {Marketplace}");
      return sb.ToString();
    }
  }
}