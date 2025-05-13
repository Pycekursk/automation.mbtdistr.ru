using Newtonsoft.Json;

using System.Text.Json.Serialization;

namespace automation.mbtdistr.ru.Models
{
  public class CabinetSettings
  {
    public int Id { get; set; }
    public int CabinetId { get; set; }

    [System.Text.Json.Serialization.JsonIgnore, Newtonsoft.Json.JsonIgnore]
    public Cabinet Cabinet { get; set; }
    public ICollection<ConnectionParameter> ConnectionParameters { get; set; }
        = new List<ConnectionParameter>();
  }
}