namespace automation.mbtdistr.ru.temp
{
  // 1. Запрос создания отчёта возвратов (v2)
  public class CreateReturnReportRequest
  {
    public CreateReturnReportFilter Filter { get; set; }
    public string Language { get; set; } = "DEFAULT";
  }

}
