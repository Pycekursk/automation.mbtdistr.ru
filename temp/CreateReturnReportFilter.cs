namespace automation.mbtdistr.ru.temp
{
  public class CreateReturnReportFilter
  {
    public string DeliverySchema { get; set; }       // "FBO" или "FBS"
    public DateTime DateFrom { get; set; }           // обязательный с v2
    public DateTime DateTo { get; set; }             // обязательный с v2
    public List<string> Status { get; set; }         // обязательный с v2: e.g. ["DELIVERED"]
  }

}
