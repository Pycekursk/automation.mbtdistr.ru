using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace automation.mbtdistr.ru.Models
{
  public class ConnectionParameter
  {
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public int CabinetSettingsId { get; set; }

    [System.Text.Json.Serialization.JsonIgnore, Newtonsoft.Json.JsonIgnore]
    public CabinetSettings CabinetSettings { get; set; }

    [Display(Name = "Параметр")]
    public string? Key { get; set; }   // "Token", "ClientId", "ApiKey" и т.д.

    [Display(Name = "Значение")]
    public string? Value { get; set; }
  }
}