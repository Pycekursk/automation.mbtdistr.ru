using Newtonsoft.Json;

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace automation.mbtdistr.ru.Services.YandexMarket.Models
{
  /// <summary>
  /// Фильтр для запроса возвратов или невыкупов.
  /// </summary>
  public class YMFilter
  {
    public YMFilter(YMFilter yMFilter)
    {
      FromDate = yMFilter.FromDate;
      ToDate = yMFilter.ToDate;
      Type = yMFilter.Type;
      Statuses = yMFilter.Statuses;
      OrderIds = yMFilter.OrderIds;
      Limit = yMFilter.Limit;
      PageToken = yMFilter.PageToken;
    }

    public YMFilter()
    {
    }


    /// <summary>
    /// Начальная дата для фильтрации невыкупов или возвратов по дате обновления.
    /// Формат: ГГГГ-ММ-ДД.
    /// Example: 2022-10-31
    /// </summary>
    [JsonPropertyName("fromDate")]
    [JsonProperty("fromDate")]
    [DataType(DataType.Date)]
    public string? FromDate { get; set; }

    /// <summary>  
    /// Конечная дата для фильтрации невыкупов или возвратов по дате обновления.  
    /// Формат: ГГГГ-ММ-ДД.  
    /// Example: 2022-11-30  
    /// </summary>  
    [JsonPropertyName("toDate")]
    [JsonProperty("toDate")]
    [DataType(DataType.Date)]
    public string? ToDate { get; set; }

    /// <summary>
    /// Тип возврата.
    /// </summary>
    [JsonPropertyName("type")]
    [JsonProperty("type")]
    public YMReturnType? Type { get; set; }

    /// <summary>  
    /// Статусы невыкупов или возвратов — для фильтрации результатов.  
    /// Несколько статусов перечисляются через запятую.  
    /// Example: STARTED_BY_USER,WAITING_FOR_DECISION  
    /// </summary>  
    [JsonPropertyName("statuses")]
    [JsonProperty("statuses")]
    public List<YMRefundStatusType>? Statuses { get; set; }

    /// <summary>  
    /// Идентификаторы заказов — для фильтрации результатов.  
    /// Несколько идентификаторов перечисляются через запятую без пробела.  
    /// Example: 123543  
    /// Max items: 50  
    /// Unique items  
    /// </summary>  
    [JsonPropertyName("orderIds")]
    [JsonProperty("orderIds")]
    public List<long>? OrderIds { get; set; }

    /// <summary>
    /// Количество результатов в ответе.
    /// </summary>
    public int Limit { get; internal set; }

    /// <summary>
    /// Идентификатор следующей страницы (для пагинации).
    /// </summary>
    [JsonPropertyName("page_token")]
    [JsonProperty("page_token")]
    public string? PageToken { get; set; }

    /// <summary>
    /// Метод преобразования фильтра в словарь для строки запроса.
    /// </summary>
    public Dictionary<string, object> ToQueryParams()
    {
      //получаем значение аттрибута enummember

      var queryParams = new Dictionary<string, object>();
      if (FromDate != null) queryParams.Add("fromDate", FromDate);
      if (ToDate != null) queryParams.Add("toDate", ToDate);
      if (Type != null) queryParams.Add("type", Type.GetEnumMemberValue());
      if (Statuses != null) queryParams.Add("statuses", string.Join(",", Statuses.Select(x => x.GetEnumMemberValue())));
      if (OrderIds != null) queryParams.Add("orderIds", string.Join(",", OrderIds));
      if (Limit > 0) queryParams.Add("limit", Limit);
      if (PageToken != null) queryParams.Add("page_token", PageToken);
      return queryParams;
    }
  }

}
