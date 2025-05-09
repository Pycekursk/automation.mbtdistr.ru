using automation.mbtdistr.ru.Models;
using automation.mbtdistr.ru.Services.Ozon;
using automation.mbtdistr.ru.Services.YandexMarket.Models;

using Microsoft.EntityFrameworkCore;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
//using System.Text.Json.Serialization;

//using System.Text.Json.Serialization;

using static automation.mbtdistr.ru.Services.YandexMarket.Models.DTOs;

namespace automation.mbtdistr.ru.Services.YandexMarket
{
  public class YMApiService
  {
    private readonly YMApiHttpClient _yMApiHttpClient;
    public YMApiService(YMApiHttpClient yMApiHttpClient)
    {
      _yMApiHttpClient = yMApiHttpClient;
    }

    public async Task<CampaignsResponse> GetCampaignsAsync(Cabinet cabinet)
    {
      try
      {
        var response = await _yMApiHttpClient.SendRequestAsync(
            MarketApiRequestType.Campaigns,
            cabinet
        );
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        var obj = json.FromJson<CampaignsResponse>();
        return obj;
      }
      catch (Exception)
      {
        throw;
      }
    }

    public async Task<ReturnsListResponse?> GetReturnsListAsync(Cabinet cabinet, Campaign campaign, YMFilter? filter = null, int limit = 500, long? lastId = null)
    {
      if (filter == null)
      {
        filter = new YMFilter
        {
          FromDate = DateTime.Now.AddDays(-20).ToString("yyyy-MM-dd"),
          ToDate = DateTime.Now.ToString("yyyy-MM-dd"),
          //Type = YMReturnType.Unredeemed,
          //Statuses = new List<YMRefundStatusType> { YMRefundStatusType.StartedByUser, YMRefundStatusType.RefundInProgress, YMRefundStatusType.RefundedWithBonuses, YMRefundStatusType.DecisionMade, YMRefundStatusType.RefundedByShop, YMRefundStatusType.WaitingForDecision }
        };
      }
      try
      {
        var response = await _yMApiHttpClient.SendRequestAsync(
            MarketApiRequestType.ReturnsList,
            cabinet,
            null,
            query: filter.ToQueryParams(),
            pathParams: new Dictionary<string, object>
            {
              { "campaignId", campaign.Id }
            }
        );
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        await Extensions.SendDebugMessage(json);
        var obj = Newtonsoft.Json.JsonConvert.DeserializeObject<ReturnsListResponse>(json, new JsonSerializerSettings()
        {
          Converters = new List<JsonConverter>
          {
            new StringEnumConverter()
          }
        });
        return obj;
      }
      catch (Exception ex)
      {
        await Extensions.SendDebugMessage($"Ошибка получения списка возвратов: {ex.Message}");
        return default;
      }
    }

    public async Task<YMSupplyRequestResponse> GetSupplyRequests(Cabinet cabinet, Campaign campaign)
    {
      try
      {
        var response = await _yMApiHttpClient.SendRequestAsync(
            MarketApiRequestType.SupplyRequests,
            cabinet,
            null,
            pathParams: new Dictionary<string, object>
            {
              { "campaignId", campaign.Id }
            }
        );
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        var obj = Newtonsoft.Json.JsonConvert.DeserializeObject<YMSupplyRequestResponse>(json, new JsonSerializerSettings()
        {
          Converters = new List<JsonConverter>
          {
            new StringEnumConverter()
          }
        });
        return obj;
      }
      catch (Exception ex)
      {
        await Extensions.SendDebugMessage($"Ошибка получения списка возвратов: {ex.Message}");
        return default;
      }
    }




    /// <summary>
    /// Получение списка товаров в заданной заявке на поставку.
    /// </summary>
    /// <param name="cabinet">Параметры авторизации кабинета.</param>
    /// <param name="campaign">Информация о кампании (магазине).</param>
    /// <param name="requestId">Идентификатор заявки.</param>
    /// <param name="limit">Максимальное число записей на странице.</param>
    /// <param name="pageToken">Токен следующей страницы для пагинации.</param>
    /// <returns>Десериализованный ответ с товарами в заявке.</returns>
    public async Task<YMGetSupplyRequestItemsResponse> GetSupplyRequestItemsAsync(
        Cabinet cabinet,
        Campaign campaign,
        long requestId,
        int? limit = null,
        string pageToken = null)
    {
      try
      {
        var query = new Dictionary<string, object>();
        if (limit.HasValue)
          query.Add("limit", limit.Value);
        if (!string.IsNullOrEmpty(pageToken))
          query.Add("page_token", pageToken);

        var response = await _yMApiHttpClient.SendRequestAsync(
            MarketApiRequestType.SupplyItems,
            cabinet,
            body: new YMSupplyRequestItemsRequest
            {
              RequestId = requestId
            },
            query: query,
            pathParams: new Dictionary<string, object>
            {
                        { "campaignId", campaign.Id }
            }
        );

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<YMGetSupplyRequestItemsResponse>(json, new JsonSerializerSettings
        {
          Converters = new List<JsonConverter>
                    {
                        new StringEnumConverter()
                    }
        });

        return result;
      }
      catch (Exception ex)
      {
        await Extensions.SendDebugMessage($"Ошибка получения товаров в заявке: {ex.Message}");
        return default;
      }
    }
    /// <summary>
    /// Получение списка документов по заявке на поставку, вывоз или утилизацию.
    /// </summary>
    /// <param name="cabinet">Параметры авторизации кабинета.</param>
    /// <param name="campaign">Информация о кампании (магазине).</param>
    /// <param name="requestId">Идентификатор заявки.</param>
    /// <returns>Десериализованный ответ с документами по заявке.</returns>
    public async Task<YMGetSupplyRequestDocumentsResponse> GetSupplyRequestDocumentsAsync(
        Cabinet cabinet,
        Campaign campaign,
        long requestId)
    {
      try
      {
        var response = await _yMApiHttpClient.SendRequestAsync(
            MarketApiRequestType.SupplyDocuments,
            cabinet,
            body: new YMSupplyRequestDocumentsRequest
            {
              RequestId = requestId
            },
            pathParams: new Dictionary<string, object>
            {
                        { "campaignId", campaign.Id }
            }
        );
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<YMGetSupplyRequestDocumentsResponse>(json, new JsonSerializerSettings
        {
          Converters = new List<JsonConverter>
                    {
                        new StringEnumConverter()
                    }
        });
        return result;
      }
      catch (Exception ex)
      {
        await Extensions.SendDebugMessage($"Ошибка получения документов в заявке: {ex.Message}");
        return default;
      }
    }
  }

  #region Request Models

  /// <summary>
  /// Модель запроса для получения документов по заявке.
  /// </summary>
  public class YMSupplyRequestDocumentsRequest
  {
    /// <summary>
    /// Идентификатор заявки.
    /// </summary>
    [Display(Name = "Идентификатор заявки")]
    [JsonProperty("requestId")]
    [Required]
    public long RequestId { get; set; }
  }

  #endregion

  #region Response Models

  /// <summary>
  /// Ответ API: список документов по заявке и информация по ним.
  /// </summary>
  public class YMGetSupplyRequestDocumentsResponse
  {
    /// <summary>
    /// Статус ответа.
    /// </summary>
    [Display(Name = "Статус ответа")]
    [JsonProperty("status")]
    public YMApiResponseStatusType Status { get; set; }

    /// <summary>
    /// Результат запроса.
    /// </summary>
    [Display(Name = "Результат запроса")]
    [JsonProperty("result")]
    public YMSupplyRequestDocumentsResult Result { get; set; }
  }

  /// <summary>
  /// Результат получения документов по заявке.
  /// </summary>
  public class YMSupplyRequestDocumentsResult
  {
    /// <summary>
    /// Список документов.
    /// </summary>
    [Display(Name = "Список документов")]
    [JsonProperty("documents")]
    public List<YMSupplyRequestDocument> Documents { get; set; }
  }

  /// <summary>
  /// Информация о документе по заявке.
  /// </summary>
  public class YMSupplyRequestDocument
  {
    /// <summary>
    /// Дата и время создания документа.
    /// </summary>
    [Display(Name = "Дата и время создания документа")]
    [JsonProperty("createdAt")]
    [Required]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Тип документа.
    /// </summary>
    [Display(Name = "Тип документа")]
    [JsonProperty("type")]
    [JsonConverter(typeof(StringEnumConverter))]
    [Required]
    public YMSupplyRequestDocumentType Type { get; set; }

    /// <summary>
    /// Ссылка на документ.
    /// </summary>
    [Display(Name = "Ссылка на документ")]
    [JsonProperty("url")]
    [Required]
    public string Url { get; set; }
  }

  #endregion

  #region Enums

  /// <summary>
  /// Тип документа по заявке.
  /// </summary>
  [JsonConverter(typeof(StringEnumConverter))]
  public enum YMSupplyRequestDocumentType
  {
    // Документы, которые загружает магазин
    /// <summary>Список товаров.</summary>
    [EnumMember(Value = "SUPPLY")]
    SUPPLY,

    /// <summary>Список товаров в дополнительной поставке.</summary>
    [EnumMember(Value = "ADDITIONAL_SUPPLY")]
    ADDITIONAL_SUPPLY,

    /// <summary>Список товаров в мультипоставке.</summary>
    [EnumMember(Value = "VIRTUAL_DISTRIBUTION_CENTER_SUPPLY")]
    VIRTUAL_DISTRIBUTION_CENTER_SUPPLY,

    /// <summary>Список товаров для утилизации.</summary>
    [EnumMember(Value = "TRANSFER")]
    TRANSFER,

    /// <summary>Список товаров для вывоза.</summary>
    [EnumMember(Value = "WITHDRAW")]
    WITHDRAW,

    // Поставка товаров
    /// <summary>Ошибки по товарам в поставке.</summary>
    [EnumMember(Value = "VALIDATION_ERRORS")]
    VALIDATION_ERRORS,

    /// <summary>Ярлыки для грузомест.</summary>
    [EnumMember(Value = "CARGO_UNITS")]
    CARGO_UNITS,

    // Дополнительная поставка и непринятые товары
    /// <summary>Товары, которые подходят для дополнительной поставки.</summary>
    [EnumMember(Value = "ADDITIONAL_SUPPLY_ACCEPTABLE_GOODS")]
    ADDITIONAL_SUPPLY_ACCEPTABLE_GOODS,

    /// <summary>Вывоз непринятых товаров.</summary>
    [EnumMember(Value = "ADDITIONAL_SUPPLY_UNACCEPTABLE_GOODS")]
    ADDITIONAL_SUPPLY_UNACCEPTABLE_GOODS,

    // Маркировка товаров
    /// <summary>Входящий УПД.</summary>
    [EnumMember(Value = "INBOUND_UTD")]
    INBOUND_UTD,

    /// <summary>Исходящий УПД.</summary>
    [EnumMember(Value = "OUTBOUND_UTD")]
    OUTBOUND_UTD,

    /// <summary>Коды маркировки товаров.</summary>
    [EnumMember(Value = "IDENTIFIERS")]
    IDENTIFIERS,

    /// <summary>Принятые товары с кодами маркировки.</summary>
    [EnumMember(Value = "CIS_FACT")]
    CIS_FACT,

    /// <summary>Товары, для которых нужна маркировка.</summary>
    [EnumMember(Value = "ITEMS_WITH_CISES")]
    ITEMS_WITH_CISES,

    /// <summary>Отчет по маркированным товарам для вывоза со склада.</summary>
    [EnumMember(Value = "REPORT_OF_WITHDRAW_WITH_CISES")]
    REPORT_OF_WITHDRAW_WITH_CISES,

    /// <summary>Маркированные товары, которые приняты после вторичной приемки.</summary>
    [EnumMember(Value = "SECONDARY_ACCEPTANCE_CISES")]
    SECONDARY_ACCEPTANCE_CISES,

    /// <summary>Принятые товары с регистрационным номером партии (РНПТ).</summary>
    [EnumMember(Value = "RNPT_FACT")]
    RNPT_FACT,

    // Акты
    /// <summary>Акт возврата.</summary>
    [EnumMember(Value = "ACT_OF_WITHDRAW")]
    ACT_OF_WITHDRAW,

    /// <summary>Акт изъятия непринятого товара.</summary>
    [EnumMember(Value = "ANOMALY_CONTAINERS_WITHDRAW_ACT")]
    ANOMALY_CONTAINERS_WITHDRAW_ACT,

    /// <summary>Акт списания с ответственного хранения.</summary>
    [EnumMember(Value = "ACT_OF_WITHDRAW_FROM_STORAGE")]
    ACT_OF_WITHDRAW_FROM_STORAGE,

    /// <summary>Акт приема-передачи.</summary>
    [EnumMember(Value = "ACT_OF_RECEPTION_TRANSFER")]
    ACT_OF_RECEPTION_TRANSFER,

    /// <summary>Акт о расхождениях.</summary>
    [EnumMember(Value = "ACT_OF_DISCREPANCY")]
    ACT_OF_DISCREPANCY,

    /// <summary>Акт вторичной приемки.</summary>
    [EnumMember(Value = "SECONDARY_RECEPTION_ACT")]
    SECONDARY_RECEPTION_ACT
  }

  #endregion

  #region Request Models

  /// <summary>
  /// Модель запроса для получения товаров в заявке.
  /// </summary>
  public class YMSupplyRequestItemsRequest
  {
    /// <summary>
    /// Идентификатор заявки.
    /// </summary>
    [Display(Name = "Идентификатор заявки")]
    [JsonProperty("requestId")]
    [Required]
    public long RequestId { get; set; }
  }

  #endregion

  #region Response Models

  /// <summary>
  /// Ответ API: список товаров в заявке и информация по ним.
  /// </summary>
  public class YMGetSupplyRequestItemsResponse
  {
    /// <summary>
    /// Статус ответа.
    /// </summary>
    [Display(Name = "Статус ответа")]
    [JsonProperty("status")]
    public YMApiResponseStatusType Status { get; set; }

    /// <summary>
    /// Результат запроса.
    /// </summary>
    [Display(Name = "Результат запроса")]
    [JsonProperty("result")]
    public YMSupplyRequestItemsResult Result { get; set; }
  }

  /// <summary>
  /// Результат получения товаров в заявке.
  /// </summary>
  public class YMSupplyRequestItemsResult
  {
    /// <summary>
    /// Список товаров.
    /// </summary>
    [Display(Name = "Список товаров")]
    [JsonProperty("items")]
    public List<YMSupplyRequestItem> Items { get; set; }

    /// <summary>
    /// Пейджинг по результатам.
    /// </summary>
    [Display(Name = "Пейджинг по результатам")]
    [JsonProperty("paging")]
    public YMForwardScrollingPager Paging { get; set; }

    /// <summary>
    /// Количество товаров в заявке.
    /// </summary>
    [Display(Name = "Количество товаров в заявке")]
    [JsonProperty("counters")]
    public YMSupplyRequestItemCounters Counters { get; set; }
  }

  /// <summary>
  /// Информация о товаре в заявке.
  /// </summary>
  public class YMSupplyRequestItem
  {
    /// <summary>
    /// Название товара.
    /// </summary>
    [Display(Name = "Название товара")]
    [JsonProperty("name")]
    [Required]
    [MaxLength(255)]
    public string Name { get; set; }

    /// <summary>
    /// Ваш SKU — идентификатор товара в вашей системе.
    /// </summary>
    [Display(Name = "Ваш SKU — идентификатор товара в вашей системе")]
    [JsonProperty("offerId")]
    [Required]
    [MaxLength(255)]
    public string OfferId { get; set; }

    /// <summary>
    /// Цена за единицу товара.
    /// </summary>
    [Display(Name = "Цена за единицу товара")]
    [JsonProperty("price")]
    [Required]
    public YMCurrencyValue Price { get; set; }
  }

  /// <summary>
  /// Количество товаров в заявке.
  /// </summary>
  public class YMSupplyRequestItemCounters
  {
    /// <summary>
    /// Количество товаров с браком.
    /// </summary>
    [Display(Name = "Количество товаров с браком")]
    [JsonProperty("defectCount")]
    public int DefectCount { get; set; }

    /// <summary>
    /// Количество товаров, принятых на складе.
    /// </summary>
    [Display(Name = "Количество товаров, принятых на складе")]
    [JsonProperty("factCount")]
    public int FactCount { get; set; }

    /// <summary>
    /// Количество товаров в заявке на поставку.
    /// </summary>
    [Display(Name = "Количество товаров в заявке на поставку")]
    [JsonProperty("planCount")]
    public int PlanCount { get; set; }

    /// <summary>
    /// Количество товаров с недостатками.
    /// </summary>
    [Display(Name = "Количество товаров с недостатками")]
    [JsonProperty("shortageCount")]
    public int ShortageCount { get; set; }

    /// <summary>
    /// Количество лишних товаров.
    /// </summary>
    [Display(Name = "Количество лишних товаров")]
    [JsonProperty("surplusCount")]
    public int SurplusCount { get; set; }
  }

  /// <summary>
  /// Валюта и ее значение.
  /// </summary>
  public class YMCurrencyValue
  {
    /// <summary>
    /// Код валюты.
    /// </summary>
    [Display(Name = "Код валюты")]
    [JsonProperty("currencyId")]
    public YMCurrencyType CurrencyId { get; set; }

    /// <summary>
    /// Значение.
    /// </summary>
    [Display(Name = "Значение")]
    [JsonProperty("value")]
    public decimal Value { get; set; }
  }

  #endregion

  #region Enums

  /// <summary>
  /// Тип ответа API. Возможные значения: OK — ошибок нет, ERROR — при обработке запроса произошла ошибка.
  /// </summary>
  [JsonConverter(typeof(StringEnumConverter))]
  public enum YMApiResponseStatusType
  {
    /// <summary>
    /// Ошибок нет.
    /// </summary>
    [EnumMember(Value = "OK")]
    OK,

    /// <summary>
    /// При обработке запроса произошла ошибка.
    /// </summary>
    [EnumMember(Value = "ERROR")]
    ERROR
  }

  /// <summary>
  /// Пейджинг с прокруткой вперед.
  /// </summary>
  public class YMForwardScrollingPager
  {
    /// <summary>
    /// Идентификатор следующей страницы результатов.
    /// </summary>
    [Display(Name = "Идентификатор следующей страницы результатов")]
    [JsonProperty("nextPageToken")]
    public string NextPageToken { get; set; }
  }

  /// <summary>
  /// Коды валют.
  /// </summary>
  [JsonConverter(typeof(StringEnumConverter))]
  public enum YMCurrencyType
  {
    /// <summary>
    /// Российский рубль.
    /// </summary>
    [EnumMember(Value = "RUR")]
    RUR,

    /// <summary>
    /// Украинская гривна.
    /// </summary>
    [EnumMember(Value = "UAH")]
    UAH,

    /// <summary>
    /// Белорусский рубль.
    /// </summary>
    [EnumMember(Value = "BYR")]
    BYR,

    /// <summary>
    /// Казахстанский тенге.
    /// </summary>
    [EnumMember(Value = "KZT")]
    KZT,

    /// <summary>
    /// Узбекский сум.
    /// </summary>
    [EnumMember(Value = "UZS")]
    UZS

    // при необходимости добавить другие валюты
  }

  #endregion

  #region Yandex Market supplies DTOs

  public class YMSupplyRequestResponse
  {
    [Newtonsoft.Json.JsonProperty("status")]
    public string Status { get; set; } = string.Empty;
    [Newtonsoft.Json.JsonProperty("result")]
    public YMGetSupplyRequests Result { get; set; } = new();
  }


  /// <summary>
  /// Параметры сортировки заявок.
  /// </summary>
  public class YMSupplyRequestSorting
  {
    /// <summary>
    /// По какому параметру сортировать заявки.
    /// </summary>
    [JsonProperty("attribute")]
    [Display(Name = "По какому параметру сортировать заявки")]
    public YMSupplyRequestSortAttributeType Attribute { get; set; }

    /// <summary>
    /// Направление сортировки.
    /// </summary>
    [JsonProperty("direction")]
    [Display(Name = "Направление сортировки")]
    public YMSortOrderType Direction { get; set; }
  }

  /// <summary>
  /// Модель ответа с данными заявок.
  /// </summary>
  public class YMGetSupplyRequests
  {
    /// <summary>
    /// Список заявок.
    /// </summary>
    [JsonProperty("requests")]
    [Display(Name = "Список заявок")]
    public List<YMSupplyRequest> Requests { get; set; }

    /// <summary>
    /// Пагинация — идентификатор следующей страницы.
    /// </summary>
    [JsonProperty("paging")]
    [Display(Name = "Идентификатор следующей страницы")]
    public YMForwardScrollingPager Paging { get; set; }
  }

  /// <summary>
  /// Информация о заявке на поставку, вывоз или утилизацию.
  /// </summary>
  public class YMSupplyRequest
  {
    [JsonIgnore, Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>
    /// Счетчики: количество товаров, коробок и палет в заявке.
    /// </summary>
    [JsonProperty("counters")]
    [Display(Name = "Счетчики")]
    public YMSupplyRequestCounters? Counters { get; set; }

    /// <summary>
    /// Идентификаторы заявки.
    /// </summary>
    [JsonProperty("id")]
    [Display(Name = "Идентификаторы заявки")]
    public YMSupplyRequestId? ExternalId { get; set; }

    /// <summary>
    /// Статус заявки.
    /// </summary>
    [JsonProperty("status")]
    [Display(Name = "Статус заявки")]
    public YMSupplyRequestStatusType Status { get; set; }

    /// <summary>
    /// Подтип заявки.
    /// </summary>
    [JsonProperty("subtype")]
    [Display(Name = "Подтип заявки")]
    public YMSupplyRequestSubType Subtype { get; set; }

    /// <summary>
    /// Информация о основном складе или ПВЗ.
    /// </summary>
    [JsonProperty("targetLocation")]
    [Display(Name = "Основной склад/ПВЗ")]
    public YMSupplyRequestLocation? TargetLocation { get; set; }

    /// <summary>
    /// Тип заявки.
    /// </summary>
    [JsonProperty("type")]
    [Display(Name = "Тип заявки")]
    public YMSupplyRequestType Type { get; set; }

    /// <summary>
    /// Дата и время последнего обновления заявки.
    /// </summary>
    [JsonProperty("updatedAt")]
    [Display(Name = "Дата и время последнего обновления заявки")]
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Ссылки на дочерние заявки.
    /// </summary>
    [JsonProperty("childrenLinks")]
    [Display(Name = "Ссылки на дочерние заявки"), NotMapped]
    public List<YMSupplyRequestReference>? ChildrenLinks { get; set; }

    /// <summary>
    /// Ссылка на родительскую заявку.
    /// </summary>
    [JsonProperty("parentLink")]
    [Display(Name = "Ссылка на родительскую заявку"), NotMapped]
    public YMSupplyRequestReference? ParentLink { get; set; }

    /// <summary>
    /// Информация о транзитном складе или ПВЗ.
    /// </summary>
    [JsonProperty("transitLocation")]
    [Display(Name = "Транзитный склад/ПВЗ")]
    public YMSupplyRequestLocation? TransitLocation { get; set; }
  }

  /// <summary>
  /// Счетчики товаров, коробок и палет в заявке.
  /// </summary>
  public class YMSupplyRequestCounters
  {
    [JsonIgnore, Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }  // для EF Core

    /// <summary>Количество товаров в заявке.</summary>
    [JsonProperty("planCount")]
    [Display(Name = "Количество товаров в заявке")]
    public int PlanCount { get; set; }

    /// <summary>Количество товаров, принятых на складе.</summary>
    [JsonProperty("factCount")]
    [Display(Name = "Количество фактических товаров")]
    public int FactCount { get; set; }

    /// <summary>Количество бракованных товаров.</summary>
    [JsonProperty("defectCount")]
    [Display(Name = "Количество брака")]
    public int DefectCount { get; set; }

    /// <summary>Количество непринятых товаров.</summary>
    [JsonProperty("undefinedCount")]
    [Display(Name = "Количество непринятых товаров")]
    public int UndefinedCount { get; set; }

    /// <summary>Количество коробок.</summary>
    [JsonProperty("actualBoxCount")]
    [Display(Name = "Количество коробок")]
    public int ActualBoxCount { get; set; }

    /// <summary>Количество палет.</summary>
    [JsonProperty("actualPalletsCount")]
    [Display(Name = "Количество палет")]
    public int ActualPalletsCount { get; set; }
  }

  /// <summary>
  /// Идентификаторы заявки: внутренний, marketplace и складской номера.
  /// </summary>
  public class YMSupplyRequestId
  {
    [JsonIgnore, Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }  // для EF Core

    /// <summary>Внутренний идентификатор заявки.</summary>
    [JsonProperty("id")]
    [Display(Name = "Внутренний ID заявки")]
    public long ExternalId { get; set; }

    /// <summary>Номер заявки на маркетплейсе.</summary>
    [JsonProperty("marketplaceRequestId")]
    [Display(Name = "Номер заявки на маркетплейсе")]
    public string? MarketplaceRequestId { get; set; }

    /// <summary>Номер заявки на складе.</summary>
    [JsonProperty("warehouseRequestId")]
    [Display(Name = "Номер заявки на складе")]
    public string? WarehouseRequestId { get; set; }
  }

  /// <summary>
  /// Адрес склада или пункта выдачи.
  /// </summary>
  public class YMSupplyRequestLocationAddress
  {
    [JsonIgnore, Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }  // для EF Core

    /// <summary>Полный адрес склада или ПВЗ.</summary>
    [JsonProperty("fullAddress")]
    [Display(Name = "Полный адрес")]
    public string? FullAddress { get; set; }

    /// <summary>GPS-координаты склада или ПВЗ.</summary>
    [JsonProperty("gps")]
    [Display(Name = "GPS-координаты")]
    public YMGps Gps { get; set; }
  }

  /// <summary>
  /// Информация о складе хранения или ПВЗ.
  /// </summary>
  public class YMSupplyRequestLocation
  {
    //коллекция для связи с YMSupplyRequest в EF Core как один ко многим
    [JsonIgnore]
    public ICollection<YMSupplyRequestLocation>? SupplyRequests { get; set; }


    [JsonIgnore, Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }  // для EF Core

    /// <summary>Адрес склада или ПВЗ.</summary>
    [JsonProperty("address")]
    [Display(Name = "Адрес")]
    public YMSupplyRequestLocationAddress Address { get; set; }

    /// <summary>Название склада или ПВЗ.</summary>
    [JsonProperty("name")]
    [Display(Name = "Название")]
    public string? Name { get; set; }

    /// <summary>Идентификатор склада или логистического партнёра.</summary>
    [JsonProperty("serviceId")]
    [Display(Name = "Идентификатор склада/партнёра")]
    public long ServiceId { get; set; }

    /// <summary>Тип склада или ПВЗ.</summary>
    [JsonProperty("type")]
    [Display(Name = "Тип склада/ПВЗ")]
    public YMSupplyRequestLocationType Type { get; set; }

    /// <summary>Дата и время поставки на склад или в ПВЗ.</summary>
    [JsonProperty("requestedDate")]
    [Display(Name = "Дата и время поставки")]
    public DateTime? RequestedDate { get; set; }
  }

  /// <summary>
  /// Ссылка на связанную заявку.
  /// </summary>
  public class YMSupplyRequestReference
  {
    [JsonIgnore, Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }  // для EF Core

    /// <summary>Идентификаторы связанной заявки.</summary>
    [JsonProperty("id")]
    [Display(Name = "Идентификаторы связанной заявки")]
    public YMSupplyRequestId ExternalId { get; set; }

    /// <summary>Тип связи между заявками.</summary>
    [JsonProperty("type")]
    [Display(Name = "Тип связи")]
    public YMSupplyRequestReferenceType Type { get; set; }
  }

  /// <summary>
  /// GPS-координаты: широта и долгота.
  /// </summary>
  [Owned]
  public class YMGps
  {
    /// <summary>Широта.</summary>
    [JsonProperty("latitude")]
    [Display(Name = "Широта")]
    public decimal Latitude { get; set; }

    /// <summary>Долгота.</summary>
    [JsonProperty("longitude")]
    [Display(Name = "Долгота")]
    public decimal Longitude { get; set; }
  }

  /// <summary>
  /// Тип склада или пункта выдачи.
  /// </summary>
  [JsonConverter(typeof(StringEnumConverter))]
  public enum YMSupplyRequestLocationType
  {
    /// <summary>
    /// Склад хранения.
    /// </summary>
    [EnumMember(Value = "FULFILLMENT")]
    [Display(Name = "Склад хранения")]
    Fulfillment,

    /// <summary>
    /// Транзитный склад.
    /// </summary>
    [EnumMember(Value = "XDOC")]
    [Display(Name = "Транзитный склад")]
    Xdoc,

    /// <summary>
    /// Пункт выдачи (ПВЗ).
    /// </summary>
    [EnumMember(Value = "PICKUP_POINT")]
    [Display(Name = "Пункт выдачи (ПВЗ)")]
    PickupPoint
  }

  /// <summary>
  /// Тип связи между заявками.
  /// </summary>
  [JsonConverter(typeof(StringEnumConverter))]
  public enum YMSupplyRequestReferenceType
  {
    /// <summary>
    /// Мультипоставка.
    /// </summary>
    [EnumMember(Value = "VIRTUAL_DISTRIBUTION")]
    [Display(Name = "Мультипоставка")]
    VirtualDistribution,

    /// <summary>
    /// Вывоз непринятых товаров.
    /// </summary>
    [EnumMember(Value = "WITHDRAW")]
    [Display(Name = "Вывоз непринятых товаров")]
    Withdraw,

    /// <summary>
    /// Утилизация непринятых товаров.
    /// </summary>
    [EnumMember(Value = "UTILIZATION")]
    [Display(Name = "Утилизация непринятых товаров")]
    Utilization,

    /// <summary>
    /// Дополнительная поставка непринятых товаров.
    /// </summary>
    [EnumMember(Value = "ADDITIONAL_SUPPLY")]
    [Display(Name = "Дополнительная поставка непринятых товаров")]
    AdditionalSupply
  }

  /// <summary>
  /// По какому параметру сортировать заявки.
  /// </summary>
  [JsonConverter(typeof(StringEnumConverter))]
  public enum YMSupplyRequestSortAttributeType
  {
    /// <summary>
    /// Идентификатор заявки.
    /// </summary>
    [EnumMember(Value = "ID")]
    [Display(Name = "Идентификатор заявки")]
    Id,

    /// <summary>
    /// Дата и время поставки заявки.
    /// </summary>
    [EnumMember(Value = "REQUESTED_DATE")]
    [Display(Name = "Дата и время поставки заявки")]
    RequestedDate,

    /// <summary>
    /// Дата и время последнего обновления заявки.
    /// </summary>
    [EnumMember(Value = "UPDATED_AT")]
    [Display(Name = "Дата и время последнего обновления заявки")]
    UpdatedAt,

    /// <summary>
    /// Статус заявки.
    /// </summary>
    [EnumMember(Value = "STATUS")]
    [Display(Name = "Статус заявки")]
    Status
  }
  #endregion
}
