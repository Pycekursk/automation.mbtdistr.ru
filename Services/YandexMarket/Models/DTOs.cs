using Newtonsoft.Json;

using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace automation.mbtdistr.ru.Services.YandexMarket.Models
{
  public partial class DTOs
  {



  }

  public class ReturnsListResponse
  {
    [JsonPropertyName("status"), Newtonsoft.Json.JsonProperty("status")]
    public string Status { get; set; } = string.Empty;
    [JsonPropertyName("result"), Newtonsoft.Json.JsonProperty("result")]
    public ReturnsListResult Result { get; set; } = new();
  }

  /// <summary>
  /// Результат запроса списка возвратов Yandex Market.
  /// </summary>
  public class ReturnsListResult
  {
    /// <summary>
    /// Информация о пагинации.
    /// </summary>
    [JsonPropertyName("paging")]
    [JsonProperty("paging")]
    public Paging Paging { get; set; } = new();

    /// <summary>
    /// Список возвратов.
    /// </summary>
    [JsonPropertyName("returns")]
    [JsonProperty("returns")]
    public List<YMReturn> Returns { get; set; } = new();
  }

  /// <summary>
  /// Данные пагинации.
  /// </summary>
  public class Paging
  {
    /// <summary>
    /// Общее количество элементов.
    /// </summary>
    [JsonPropertyName("total")]
    [JsonProperty("total")]
    public int Total { get; set; }

    /// <summary>
    /// Индекс начала выборки.
    /// </summary>
    [JsonPropertyName("from")]
    [JsonProperty("from")]
    public int From { get; set; }

    /// <summary>
    /// Индекс конца выборки.
    /// </summary>
    [JsonPropertyName("to")]
    [JsonProperty("to")]
    public int To { get; set; }

    /// <summary>
    /// Идентификатор следующей страницы
    /// </summary>
    [JsonPropertyName("nextPageToken")]
    [JsonProperty("nextPageToken")]
    public string? NextPageToken { get; set; }
  }


  /// <summary>
  /// Возврат товара.
  /// </summary>
  public class YMReturn
  {
    /// <summary>
    /// Уникальный идентификатор возврата.
    /// </summary>
    [JsonPropertyName("id")]
    [JsonProperty("id")]
    public long Id { get; set; }

    /// <summary>
    /// Идентификатор заказа, связанного с возвратом.
    /// </summary>
    [JsonPropertyName("orderId")]
    [JsonProperty("orderId")]
    public long OrderId { get; set; }

    /// <summary>
    /// Дата создания возврата.
    /// </summary>
    [JsonPropertyName("creationDate")]
    [JsonProperty("creationDate")]
    public DateTime CreationDate { get; set; }

    /// <summary>
    /// Дата последнего обновления возврата.
    /// </summary>
    [JsonPropertyName("updateDate")]
    [JsonProperty("updateDate")]
    public DateTime UpdateDate { get; set; }

    /// <summary>
    /// Логистический пункт выдачи, связанный с возвратом.
    /// </summary>
    [JsonPropertyName("logisticPickupPoint")]
    [JsonProperty("logisticPickupPoint")]
    public LogisticPickupPoint? LogisticPickupPoint { get; set; }

    /// <summary>
    /// Тип получателя отправления.
    /// </summary>
    [JsonPropertyName("shipmentRecipientType")]
    [JsonProperty("shipmentRecipientType")]
    public YMShipmentRecipientType ShipmentRecipientType { get; set; }

    /// <summary>
    /// Статус отправления.
    /// </summary>
    [JsonPropertyName("shipmentStatus")]
    [JsonProperty("shipmentStatus")]
    public YMReturnShipmentStatusType ShipmentStatus { get; set; }

    /// <summary>
    /// Статус возврата денег
    /// </summary>
    [JsonPropertyName("refundStatus")]
    [JsonProperty("refundStatus")]
    public YMRefundStatusType RefundStatus { get; set; }

    /// <summary>
    /// Сумма и валюта возврата.
    /// </summary>
    [JsonPropertyName("amount")]
    [JsonProperty("amount")]
    public Amount? Amount { get; set; }

    /// <summary>
    /// Список товаров в возврате.
    /// </summary>
    [JsonPropertyName("items")]
    [JsonProperty("items")]
    public List<Item> Items { get; set; } = new();

    /// <summary>
    /// Список изображений, связанных с возвратом.
    /// </summary>
    [JsonPropertyName("images")]
    [JsonProperty("images")]
    public List<string> Images { get; set; } = new();

    /// <summary>
    /// Комментарий к возврату.
    /// </summary>
    [JsonPropertyName("comment")]
    [JsonProperty("comment")]
    public string Comment { get; set; } = string.Empty;

    /// <summary>
    /// Решение по возврату.
    /// </summary>
    [JsonPropertyName("decisionType")]
    [JsonProperty("decisionType")]
    public YMReturnDecisionType DecisionType { get; set; }

    /// <summary>
    /// Причина возврата.
    /// </summary>
    [JsonPropertyName("decisionReason")]
    [JsonProperty("decisionReason")]
    public YMReturnDecisionReasonType DecisionReason { get; set; }

    /// <summary>
    /// Подробности причины возврата.
    /// </summary>
    [JsonPropertyName("decisionSubreason")]
    [JsonProperty("decisionSubreason")]
    public YMReturnDecisionSubreasonType DecisionSubreason { get; set; }
  }




  /// <summary>
  /// Логистический пункт выдачи.
  /// </summary>
  public class LogisticPickupPoint
  {
    /// <summary>
    /// Идентификатор пункта выдачи.
    /// </summary>
    [JsonPropertyName("id")]
    [JsonProperty("id")]
    public long Id { get; set; }

    /// <summary>
    /// Название пункта выдачи.
    /// </summary>
    [JsonPropertyName("name")]
    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Адрес пункта выдачи.
    /// </summary>
    [JsonPropertyName("address")]
    [JsonProperty("address")]
    public Address Address { get; set; } = new();

    /// <summary>
    /// Инструкция по получению.
    /// </summary>
    [JsonPropertyName("instruction")]
    [JsonProperty("instruction")]
    public string Instruction { get; set; } = string.Empty;

    /// <summary>
    /// Тип пункта выдачи.
    /// </summary>
    [JsonPropertyName("type")]
    [JsonProperty("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Идентификатор логистического партнёра.
    /// </summary>
    [JsonPropertyName("logisticPartnerId")]
    [JsonProperty("logisticPartnerId")]
    public long LogisticPartnerId { get; set; }
  }

  /// <summary>
  /// Адрес.
  /// </summary>
  public class Address
  {
    /// <summary>
    /// Страна.
    /// </summary>
    [JsonPropertyName("country")]
    [JsonProperty("country")]
    public string Country { get; set; } = string.Empty;

    /// <summary>
    /// Город.
    /// </summary>
    [JsonPropertyName("city")]
    [JsonProperty("city")]
    public string City { get; set; } = string.Empty;

    /// <summary>
    /// Улица.
    /// </summary>
    [JsonPropertyName("street")]
    [JsonProperty("street")]
    public string Street { get; set; } = string.Empty;

    /// <summary>
    /// Дом.
    /// </summary>
    [JsonPropertyName("house")]
    [JsonProperty("house")]
    public string House { get; set; } = string.Empty;

    /// <summary>
    /// Почтовый индекс.
    /// </summary>
    [JsonPropertyName("postcode")]
    [JsonProperty("postcode")]
    public string Postcode { get; set; } = string.Empty;
  }

  /// <summary>
  /// Товар в возврате.
  /// </summary>
  public class Item
  {
    /// <summary>
    /// Маркет SKU.
    /// </summary>
    [JsonPropertyName("marketSku")]
    [JsonProperty("marketSku")]
    public long MarketSku { get; set; }

    /// <summary>
    /// Магазинный SKU.
    /// </summary>
    [JsonPropertyName("shopSku")]
    [JsonProperty("shopSku")]
    public string ShopSku { get; set; } = string.Empty;

    /// <summary>
    /// Количество.
    /// </summary>
    [JsonPropertyName("count")]
    [JsonProperty("count")]
    public int Count { get; set; }

    /// <summary>
    /// Экземпляры товара.
    /// </summary>
    [JsonPropertyName("instances")]
    [JsonProperty("instances")]
    public List<Instance> Instances { get; set; } = new();

    /// <summary>
    /// Треки доставки.
    /// </summary>
    [JsonPropertyName("tracks")]
    [JsonProperty("tracks")]
    public List<Track> Tracks { get; set; } = new();
  }

  /// <summary>
  /// Экземпляр товара.
  /// </summary>
  public class Instance
  {
    /// <summary>
    /// Статус экземпляра.
    /// </summary>
    [JsonPropertyName("status")]
    [JsonProperty("status")]
    public string Status { get; set; } = string.Empty;
  }

  /// <summary>
  /// Трек доставки.
  /// </summary>
  public class Track
  {
    /// <summary>
    /// Трек-код.
    /// </summary>
    [JsonPropertyName("trackCode")]
    [JsonProperty("trackCode")]
    public string TrackCode { get; set; } = string.Empty;
  }

  /// <summary>
  /// Запрос на получение списка возвратов.
  /// </summary>
  public class ReturnsListRequest
  {
    /// <summary>
    /// Фильтр запроса.
    /// </summary>
    [JsonPropertyName("filter")]
    [JsonProperty("filter")]
    public YMFilter Filter { get; set; } = new();

    /// <summary>
    /// Ограничение количества результатов.
    /// </summary>
    [JsonPropertyName("limit")]
    [JsonProperty("limit")]
    public int Limit { get; set; }

    /// <summary>
    /// Последний идентификатор возврата (для пагинации).
    /// </summary>
    [JsonPropertyName("lastId")]
    [JsonProperty("lastId")]
    public long? LastId { get; set; }
  }


  /// <summary>
  /// Фильтр для запроса возвратов или невыкупов.
  /// </summary>
  public class YMFilter
  {
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
      return queryParams;
    }
  }

  /// <summary>
  /// Ответ на запрос кампаний.
  /// </summary>
  public class CampaignsResponse
  {
    /// <summary>
    /// Список кампаний.
    /// </summary>
    [JsonPropertyName("campaigns")]
    [JsonProperty("campaigns")]
    public List<Campaign> Campaigns { get; set; } = new();

    /// <summary>
    /// Информация о пагинации.
    /// </summary>
    [JsonPropertyName("pager")]
    [JsonProperty("pager")]
    public Pager Pager { get; set; } = new();
  }

  /// <summary>
  /// Кампания.
  /// </summary>
  public class Campaign
  {
    /// <summary>
    /// Домен кампании.
    /// </summary>
    [JsonPropertyName("domain")]
    [JsonProperty("domain")]
    public string Domain { get; set; } = string.Empty;

    /// <summary>
    /// Идентификатор кампании.
    /// </summary>
    [JsonPropertyName("id")]
    [JsonProperty("id")]
    public long Id { get; set; }

    /// <summary>
    /// Идентификатор клиента.
    /// </summary>
    [JsonPropertyName("clientId")]
    [JsonProperty("clientId")]
    public long ClientId { get; set; }

    /// <summary>
    /// Информация о бизнесе.
    /// </summary>
    [JsonPropertyName("business")]
    [JsonProperty("business")]
    public Business Business { get; set; } = new();

    /// <summary>
    /// Тип размещения.
    /// </summary>
    [JsonPropertyName("placementType")]
    [JsonProperty("placementType")]
    public string PlacementType { get; set; } = string.Empty;
  }

  /// <summary>
  /// Бизнес.
  /// </summary>
  public class Business
  {
    /// <summary>
    /// Идентификатор бизнеса.
    /// </summary>
    [JsonPropertyName("id")]
    [JsonProperty("id")]
    public long Id { get; set; }

    /// <summary>
    /// Название бизнеса.
    /// </summary>
    [JsonPropertyName("name")]
    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;
  }

  /// <summary>
  /// Информация о страницах (пагинация).
  /// </summary>
  public class Pager
  {
    /// <summary>
    /// Общее количество записей.
    /// </summary>
    [JsonPropertyName("total")]
    [JsonProperty("total")]
    public int Total { get; set; }

    /// <summary>
    /// Индекс начала текущей выборки.
    /// </summary>
    [JsonPropertyName("from")]
    [JsonProperty("from")]
    public int From { get; set; }

    /// <summary>
    /// Индекс конца текущей выборки.
    /// </summary>
    [JsonPropertyName("to")]
    [JsonProperty("to")]
    public int To { get; set; }

    /// <summary>
    /// Номер текущей страницы.
    /// </summary>
    [JsonPropertyName("currentPage")]
    [JsonProperty("currentPage")]
    public int CurrentPage { get; set; }

    /// <summary>
    /// Общее количество страниц.
    /// </summary>
    [JsonPropertyName("pagesCount")]
    [JsonProperty("pagesCount")]
    public int PagesCount { get; set; }

    /// <summary>
    /// Размер страницы (количество записей на странице).
    /// </summary>
    [JsonPropertyName("pageSize")]
    [JsonProperty("pageSize")]
    public int PageSize { get; set; }
  }

}
