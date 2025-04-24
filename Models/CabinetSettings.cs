namespace automation.mbtdistr.ru.Models
{
  public class CabinetSettings
  {
    public int Id { get; set; }
    public int CabinetId { get; set; }
    public Cabinet Cabinet { get; set; }
    public ICollection<ConnectionParameter> ConnectionParameters { get; set; }
        = new List<ConnectionParameter>();
  }
}