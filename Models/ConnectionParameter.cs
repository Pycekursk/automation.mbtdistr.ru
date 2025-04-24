using System.ComponentModel.DataAnnotations;

namespace automation.mbtdistr.ru.Models
{
  public class ConnectionParameter
  {
    public int Id { get; set; }
    public int CabinetSettingsId { get; set; }
    public CabinetSettings CabinetSettings { get; set; }

    [Display(Name = "Параметр")]
    public string Key { get; set; }   // "Token", "ClientId", "ApiKey" и т.д.

    [Display(Name = "Значение")]
    public string Value { get; set; }
  }
}