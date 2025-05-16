using automation.mbtdistr.ru.Data;
using automation.mbtdistr.ru.Models;
using automation.mbtdistr.ru.Services.Ozon;
using automation.mbtdistr.ru.Services.YandexMarket.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Text;

using Telegram.Bot.Types;

using static automation.mbtdistr.ru.Extensions;

using JsonConverter = Newtonsoft.Json.JsonConverter;

namespace automation.mbtdistr.ru.Services.YandexMarket
{
  public class YMApiService
  {
    private readonly YMApiHttpClient _yMApiHttpClient;



    public YMApiService(YMApiHttpClient yMApiHttpClient)
    {

      _yMApiHttpClient = yMApiHttpClient;
    }

    public async Task<YMCampaignsResponse> GetCampaignsAsync(Cabinet cabinet)
    {
      try
      {
        var response = await _yMApiHttpClient.SendRequestAsync(
            MarketApiRequestType.Campaigns,
            cabinet
        );
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        var obj = json.FromJson<YMCampaignsResponse>();
        return obj;
      }
      catch (Exception)
      {
        throw;
      }
    }

    /// <summary>
    /// Получает полный список возвратов, рекурсивно проходя все страницы.
    /// </summary>
    public async Task<YMApiResponse<YMListResult<YMReturn>>?> GetReturnsListAsync(
        Cabinet cabinet,
        YMCampaign campaign,
        YMFilter? filter = null,
        string? pageToken = null)
    {
      // ─── Фильтр по умолчанию ──────────────────────────────────────────────────
      filter ??= new YMFilter
      {
        FromDate = DateTime.UtcNow.AddDays(-20).ToString("yyyy-MM-dd"),
        ToDate = DateTime.UtcNow.ToString("yyyy-MM-dd"),
        PageToken = pageToken,
        Limit = 100
      };



      // На каждой итерации явно прописываем PageToken в копию фильтра,
      // чтобы ToQueryParams() отправил его в запрос.
      if (!string.IsNullOrEmpty(pageToken))
        filter.PageToken = pageToken;
      try
      {
        var response = await _yMApiHttpClient.SendRequestAsync(
            MarketApiRequestType.ReturnsList,
            cabinet,
            query: filter.ToQueryParams(),
            pathParams: new Dictionary<string, object> { { "campaignId", campaign.Id } });

        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        SerializerSettings.Converters.Add(new YMListResultConverter<YMReturn>("returns"));

        var pageResult = JsonConvert.DeserializeObject<YMApiResponse<YMListResult<YMReturn>>>(json, SerializerSettings);

        // ─── Есть ли следующая страница? ──────────────────────────────────────
        var nextToken = pageResult?.Result?.Paging?.NextPageToken;
        if (!string.IsNullOrEmpty(nextToken))
        {
          var nextPage = await GetReturnsListAsync(
                             cabinet,
                             campaign,
                             filter,   // исходный диапазон дат, лимит и т.д.
                             nextToken);

          if (nextPage?.Result is { } np)
          {
            pageResult!.Result.Items.AddRange(np.Items);
            pageResult.Result.Paging.NextPageToken = np.Paging.NextPageToken;
          }
        }

        return pageResult;
      }
      catch (Exception ex)
      {
        await SendDebugMessage($"Ошибка получения списка возвратов: {ex.Message}");

        return new YMApiResponse<YMListResult<YMReturn>>
        {
          Status = YMApiResponseStatusType.ERROR,
          Errors = new List<YMApiError>
            {
                new() { Code = "LOCAL_EXCEPTION", Message = ex.Message }
            }
        };
      }
    }

    // Единые настройки сериализации
    public static readonly JsonSerializerSettings SerializerSettings = new()
    {
      StringEscapeHandling = StringEscapeHandling.Default,
      Culture = System.Globalization.CultureInfo.CurrentCulture,
      Converters = { new StringEnumConverter() }
    };

    /// <summary>
    /// Получение информации о невыкупе или возврате
    /// https://api.partner.market.yandex.ru/campaigns/{campaignId}/orders/{orderId}/returns/{returnId}
    /// </summary> 
    public async Task<YMApiResponse<YMReturn>> GetReturnInfoAsync(Cabinet cabinet, YMCampaign campaign, long orderId, long returnId)
    {
      try
      {
        var response = await _yMApiHttpClient.SendRequestAsync(
            MarketApiRequestType.ReturnInfo,
            cabinet,
            null,
            pathParams: new Dictionary<string, object>
            {
              { "campaignId", campaign.Id },
              { "orderId", orderId },
              { "returnId", returnId }
            }
        );
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        var obj = JsonConvert.DeserializeObject<YMApiResponse<YMReturn>>(json, new JsonSerializerSettings()
        {
          StringEscapeHandling = StringEscapeHandling.Default,
          Culture = System.Globalization.CultureInfo.CurrentCulture,
          Converters = new List<JsonConverter>
          {
            new StringEnumConverter()
          }
        });
        return obj;
      }
      catch (Exception ex)
      {
        await SendDebugMessage($"Ошибка получения информации о возврате: {ex.Message}");
        return default;
      }
    }


    /// <summary>
    /// Метод получения фотографии товара в возврате.
    /// <para/>
    /// GET /campaigns/{campaignId}/orders/{orderId}/returns/{returnId}/decision/{itemId}/image/{imageHash}
    /// </summary>
    /// <param name="cabinet">Кабинет продавца.</param>
    /// <param name="campaign">Кампания Маркета.</param>
    /// <param name="orderId">Идентификатор заказа.</param>
    /// <param name="returnId">Идентификатор возврата / невыкупа.</param>
    /// <param name="itemId">Идентификатор товара в возврате.</param>
    /// <param name="imageHash">Хеш-код фотографии.</param>
    /// <returns>Фотография в Base-64, обёрнутая в <see cref="YMApiResponse{T}"/>.</returns>
    public async Task<YMApiResponse<YMReturnPhoto>> GetReturnImageAsync(
        Cabinet cabinet,
        YMCampaign campaign,
        long orderId,
        long returnId,
        long itemId,
        string imageHash)
    {
      try
      {
        var response = await _yMApiHttpClient.SendRequestAsync(
            MarketApiRequestType.Image,
            cabinet,
            null,
            pathParams: new Dictionary<string, object>
            {
          { "campaignId", campaign.Id },
          { "orderId",    orderId     },
          { "returnId",   returnId    },
          { "itemId",     itemId      },
          { "imageHash",  imageHash   }
            });

        response.EnsureSuccessStatusCode();

        // ─── Бинарный поток ► Base64 ────────────────────────────────────────────────
        var rawBytes = await response.Content.ReadAsByteArrayAsync();
        var photoDto = new YMReturnPhoto
        {
          ContentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream",
          ImageData = Convert.ToBase64String(rawBytes)
        };

        return new YMApiResponse<YMReturnPhoto>
        {
          Status = YMApiResponseStatusType.OK,
          Result = photoDto
        };
      }
      catch (Exception ex)
      {
        await SendDebugMessage($"Ошибка получения фотографии возврата: {ex.Message}");

        return new YMApiResponse<YMReturnPhoto>
        {
          Status = YMApiResponseStatusType.ERROR,
          Errors = new List<YMApiError>
      {
        new YMApiError
        {
          Code    = "LOCAL_EXCEPTION",
          Message = ex.Message
        }
      }
        };
      }
    }

    /// <summary>
    /// Метод получения информации о заказах
    /// </summary>
    public async Task<YMListResult<YMOrder>> GetOrdersAsync(Cabinet cabinet, YMCampaign campaign, long[] ordersId, YMFilter? filter = null, int limit = 100, string? pageToken = null)
    {
      if (filter == null)
      {
        filter = new YMFilter
        {
          FromDate = DateTime.Now.AddDays(-20).ToString("yyyy-MM-dd"),
          ToDate = DateTime.Now.ToString("yyyy-MM-dd"),
          Limit = limit,
          OrderIds = ordersId.ToList(),
          //Statuses = new List<YMOrderStatusType> { YMOrderStatusType.Unpaid }
        };
      }
      try
      {
        var response = await _yMApiHttpClient.SendRequestAsync(
            MarketApiRequestType.Orders,
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
        var obj = JsonConvert.DeserializeObject<YMListResult<YMOrder>>(json, new JsonSerializerSettings()
        {
          StringEscapeHandling = StringEscapeHandling.Default,
          Culture = System.Globalization.CultureInfo.CurrentCulture,
          Converters = new List<JsonConverter>
          {
            new YMListResultConverter<YMOrder>("orders"),
            new StringEnumConverter()
          }
        });
        return obj;
      }
      catch (Exception ex)
      {
        await SendDebugMessage($"Ошибка получения списка заказов: {ex.Message}");
        return default;
      }
    }

    /// <summary>
    /// Получение списка заявок на поставку.
    /// </summary>
    public async Task<YMApiResponse<YMListResult<YMSupplyRequest>>> GetSupplyRequests(Cabinet cabinet, YMCampaign campaign, int limit = 100, YMFilter? filter = null, string? pageToken = null)
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
            },
            query: new Dictionary<string, object>
            {
              { "limit", limit }
            }
        );
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        var obj = JsonConvert.DeserializeObject<YMApiResponse<YMListResult<YMSupplyRequest>>>(json, new JsonSerializerSettings()
        {
          StringEscapeHandling = StringEscapeHandling.Default,
          Culture = System.Globalization.CultureInfo.CurrentCulture,
          Converters = new List<JsonConverter>
          {
            new YMListResultConverter<YMSupplyRequest>("requests"),
            new StringEnumConverter()
          }
        });
        if (obj?.Result?.Paging.NextPageToken is string s && !string.IsNullOrEmpty(s))
          await SendDebugMessage($"В компании {campaign.Domain} ({campaign.Id}) нужна обработка постаничной загрузки");
        return obj;
      }
      catch (Exception ex)
      {
        await SendDebugMessage($"Ошибка получения списка возвратов: {ex.Message}");
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
    public async Task<YMApiResponse<YMListResult<YMSupplyRequestItem>>> GetSupplyRequestItemsAsync(
        Cabinet cabinet,
        YMCampaign campaign,
        long requestId,
        int limit = 100,
        string? pageToken = null)
    {
      try
      {
        var query = new Dictionary<string, object>();
        query.Add("limit", limit);
        if (!string.IsNullOrEmpty(pageToken))
          query.Add("page_token", pageToken);

        var response = await _yMApiHttpClient.SendRequestAsync(
            MarketApiRequestType.SupplyItems,
            cabinet,
            body: new YMRequest
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
        var result = JsonConvert.DeserializeObject<YMApiResponse<YMListResult<YMSupplyRequestItem>>>(json, new JsonSerializerSettings
        {
          StringEscapeHandling = StringEscapeHandling.Default,
          Culture = System.Globalization.CultureInfo.CurrentCulture,
          Converters = new List<JsonConverter>()
                    {
                       new YMListResultConverter<YMSupplyRequestItem>("items"),
                       new StringEnumConverter()
                    }
        });

        return result;
      }
      catch (Exception ex)
      {
        await SendDebugMessage($"Ошибка получения товаров в заявке: {ex.Message}");
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
    public async Task<YMApiResponse<YMSupplyRequestDocumentsResult>> GetSupplyRequestDocumentsAsync(
        Cabinet cabinet,
        YMCampaign campaign,
        long requestId)
    {
      try
      {
        var response = await _yMApiHttpClient.SendRequestAsync(
            MarketApiRequestType.SupplyDocuments,
            cabinet,
            body: new YMRequest
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
        var result = JsonConvert.DeserializeObject<YMApiResponse<YMSupplyRequestDocumentsResult>>(json, new JsonSerializerSettings
        {
          StringEscapeHandling = StringEscapeHandling.Default,
          Culture = System.Globalization.CultureInfo.CurrentCulture,
          Converters = new List<JsonConverter>
                    {
                        new StringEnumConverter()
                    }
        });
        return result;
      }
      catch (Exception ex)
      {
        await SendDebugMessage($"Ошибка получения документов в заявке: {ex.Message}");
        return default;
      }
    }

    /// <summary>
    /// Добавление или обновление заявки в БД на поставку.
    /// </summary>
    public async Task<YMSupplyRequest> AddOrUpdateSupplyRequestAsync(
    YMSupplyRequest incoming,
    ApplicationDbContext context)
    {
      // 1) Подгружаем существующую заявку без ссылок
      var existing = await context.YMSupplyRequests
          .Include(r => r.TargetLocation).ThenInclude(l => l.Address)
          .Include(r => r.TransitLocation).ThenInclude(l => l.Address)
          .Include(r => r.Items).ThenInclude(i => i.Price)
          .Include(r => r.Items).ThenInclude(i => i.Counters)
          .FirstOrDefaultAsync(r => r.ExternalIdId == incoming.ExternalId.Id);

      // 2.1) Локальная функция для Location
      async Task<YMSupplyRequestLocation> ResolveLocationAsync(YMSupplyRequestLocation loc)
      {
        var dbLoc = await context.YMSupplyRequestLocations
            .Include(l => l.Address)
            .FirstOrDefaultAsync(l => l.ServiceId == loc.ServiceId);

        if (dbLoc != null)
        {
          dbLoc.Name = loc.Name;
          dbLoc.Type = loc.Type;
          dbLoc.RequestedDate = loc.RequestedDate;

          if (loc.Address != null)
          {
            var dbAddr = await context.YMLocationAddresses
                .FirstOrDefaultAsync(a => a.Id == loc.Address.Id);

            if (dbAddr != null)
            {
              dbAddr.FullAddress = loc.Address.FullAddress;
              dbAddr.Gps = loc.Address.Gps;
            }
            else
            {
              context.YMLocationAddresses.Add(loc.Address);
              dbLoc.Address = loc.Address;
            }
          }

          return dbLoc;
        }
        else
        {
          if (loc.Address != null)
            context.YMLocationAddresses.Add(loc.Address);

          context.YMSupplyRequestLocations.Add(loc);
          return loc;
        }
      }

      // 2.2) Локальная функция для Item (+ Counters + Price)
      async Task<YMSupplyRequestItem> ResolveItemAsync(YMSupplyRequestItem item, YMSupplyRequest parent)
      {
        var dbItem = existing?.Items?
            .FirstOrDefault(i => i.OfferId == item.OfferId);

        if (dbItem != null)
        {
          dbItem.Name = item.Name;
          if (dbItem.Price != null)
          {
            dbItem.Price.CurrencyId = item.Price.CurrencyId;
            dbItem.Price.Value = item.Price.Value;
          }
          dbItem.Counters.DefectCount = item.Counters.DefectCount;
          dbItem.Counters.FactCount = item.Counters.FactCount;
          dbItem.Counters.PlanCount = item.Counters.PlanCount;
          dbItem.Counters.ShortageCount = item.Counters.ShortageCount;
          dbItem.Counters.SurplusCount = item.Counters.SurplusCount;
          return dbItem;
        }
        else
        {
          item.SupplyRequest = parent;
          context.YMSupplyRequestItems.Add(item);
          return item;
        }
      }

      // 3) Сохраняем заявку + вложенные Locations и Items
      if (existing == null)
      {
        // Новая заявка
        var targetLoc = await ResolveLocationAsync(incoming.TargetLocation);
        var transitLoc = incoming.TransitLocation != null
            ? await ResolveLocationAsync(incoming.TransitLocation)
            : null;

        incoming.TargetLocation = targetLoc;
        incoming.TargetLocationServiceId = targetLoc.ServiceId;

        if (transitLoc != null)
        {
          incoming.TransitLocation = transitLoc;
          incoming.TransitLocationServiceId = transitLoc.ServiceId;
        }

        if (incoming.Items != null)
          foreach (var it in incoming.Items.ToList())
            await ResolveItemAsync(it, incoming);

        context.YMSupplyRequests.Add(incoming);
        await context.SaveChangesAsync();  // чтобы получить incoming.Id
        existing = incoming;
      }
      else
      {
        // Обновление
        existing.Status = incoming.Status;
        existing.Subtype = incoming.Subtype;
        existing.Type = incoming.Type;
        existing.UpdatedAt = DateTime.UtcNow;

        var targetLoc = await ResolveLocationAsync(incoming.TargetLocation);
        var transitLoc = incoming.TransitLocation != null
            ? await ResolveLocationAsync(incoming.TransitLocation)
            : null;

        existing.TargetLocation = targetLoc;
        existing.TargetLocationServiceId = targetLoc.ServiceId;

        if (transitLoc != null)
        {
          existing.TransitLocation = transitLoc;
          existing.TransitLocationServiceId = transitLoc.ServiceId;
        }
        else
        {
          existing.TransitLocation = null;
          existing.TransitLocationServiceId = null;
        }

        if (incoming.Items != null)
        {
          // удаляем отсутствующие
          var toRemove = existing.Items
              .Where(i => !incoming.Items.Any(ii => ii.OfferId == i.OfferId))
              .ToList();
          context.YMSupplyRequestItems.RemoveRange(toRemove);

          // добавляем/обновляем
          foreach (var it in incoming.Items)
            await ResolveItemAsync(it, existing);
        }

        await context.SaveChangesAsync();
      }

      // 4) Удаляем все старые ссылки — теперь это приведёт к физическому DELETE
      var oldRefs = await context.YMSupplyRequestReferences
          .Where(rf => rf.RequestId == existing.Id || rf.RelatedRequestId == existing.Id)
          .ToListAsync();
      context.YMSupplyRequestReferences.RemoveRange(oldRefs);
      await context.SaveChangesAsync();


      // 5) Локальная функция для добавления одной ссылки, пропуская отсутствующие
      async Task AddReferenceAsync(YMSupplyRequestReference link, bool isParentLink)
      {
        var related = await context.YMSupplyRequests
            .FirstOrDefaultAsync(r => r.ExternalIdId == link.YMSupplyRequestId!.Id);
        if (related == null)
          return; // пропускаем, чтобы не нарушить FK

        if (isParentLink)
        {
          link.RequestId = related.Id;
          link.RelatedRequestId = existing.Id;
        }
        else
        {
          link.RequestId = existing.Id;
          link.RelatedRequestId = related.Id;
        }
        context.YMSupplyRequestReferences.Add(link);
      }

      // 6) Добавляем новые ссылки
      if (incoming.ChildrenLinks != null)
        foreach (var link in incoming.ChildrenLinks)
          await AddReferenceAsync(link, isParentLink: false);

      if (incoming.ParentLink != null)
        await AddReferenceAsync(incoming.ParentLink, isParentLink: true);

      await context.SaveChangesAsync();

      // 7) Возвращаем полностью загруженную заявку
      return await context.YMSupplyRequests
          .Include(r => r.TargetLocation).ThenInclude(l => l.Address)
          .Include(r => r.TransitLocation).ThenInclude(l => l.Address)
          .Include(r => r.Items).ThenInclude(i => i.Price)
          .Include(r => r.Items).ThenInclude(i => i.Counters)
          .Include(r => r.ChildrenLinks)
          .Include(r => r.ParentLink)
          .FirstAsync(r => r.Id == existing.Id);
    }

    /// <summary>
    /// Получение информации о заполненности карточек магазина.
    /// </summary>
    /// <param name="cabinet">Параметры авторизации кабинета.</param>
    /// <param name="businessId">Идентификатор кабинета.</param>
    /// <param name="request">Параметры запроса: фильтры по статусам карточек, категориям и SKU.</param>
    /// <param name="limit">Количество значений на одной странице.</param>
    /// <param name="pageToken">Токен страницы для постраничной навигации.</param>
    /// <returns>Десериализованный ответ с информацией о состоянии карточек товаров.</returns>
    public async Task<YMApiResponse<YMOfferCardsContentStatusResult>> GetOfferCardsContentStatusAsync(
        Cabinet cabinet,
        long businessId,
        YMOfferCardsContentStatusRequest request,
        int limit = 100,
        string? pageToken = null)
    {
      try
      {
        // Собираем параметры запроса
        var query = new Dictionary<string, object>
                {
                    { "limit", limit } // Лимит на странице :contentReference[oaicite:0]{index=0}:contentReference[oaicite:1]{index=1}
                };
        if (!string.IsNullOrEmpty(pageToken))
          query.Add("page_token", pageToken); // Токен страницы :contentReference[oaicite:2]{index=2}:contentReference[oaicite:3]{index=3}

        // Выполняем POST https://api.partner.market.yandex.ru/businesses/{businessId}/offer-cards :contentReference[oaicite:4]{index=4}:contentReference[oaicite:5]{index=5}
        var response = await _yMApiHttpClient.SendRequestAsync(
            MarketApiRequestType.OfferCardsContentStatus,
            cabinet,
            body: request,
            query: query,
            pathParams: new Dictionary<string, object>
            {
                        { "businessId", businessId } // Путь: {businessId} :contentReference[oaicite:6]{index=6}:contentReference[oaicite:7]{index=7}
            }
        );
        response.EnsureSuccessStatusCode();

        // Читаем и десериализуем JSON
        var json = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<YMApiResponse<YMOfferCardsContentStatusResult>>(json, new JsonSerializerSettings
        {
          StringEscapeHandling = StringEscapeHandling.Default,
          Culture = System.Globalization.CultureInfo.CurrentCulture,
          Converters = new List<JsonConverter>
                    {
                        new StringEnumConverter()
                    }
        });

        return result;
      }
      catch (Exception ex)
      {
        await Extensions.SendDebugMessage($"Ошибка получения информации о заполненности карточек: {ex.Message}");
        return default;
      }
    }

    #region Warehouses

    /// <summary>
    /// Получает список складов Маркета (FBY).
    /// </summary>
    public async Task<YMApiResponse<YMListResult<YMFulfillmentWarehouse>>> GetWarehousesAsync(Cabinet cabinet)
    {
      var response = await _yMApiHttpClient.SendRequestAsync(
          MarketApiRequestType.Warehouses,
          cabinet);
      response.EnsureSuccessStatusCode();
      var json = await response.Content.ReadAsStringAsync();
      var settings = new JsonSerializerSettings
      {
        StringEscapeHandling = StringEscapeHandling.Default,
        Culture = CultureInfo.CurrentCulture,
        Converters = { new StringEnumConverter(), new YMListResultConverter<YMFulfillmentWarehouse>("warehouses") }
      };
      return JsonConvert.DeserializeObject<YMApiResponse<YMListResult<YMFulfillmentWarehouse>>>(json, settings);
    }

    /// <summary>
    /// Получает информацию по складу Маркета (FBY) по идентификатору.
    /// </summary>
    public async Task<YMFulfillmentWarehouse?> GetWarehouseByIdAsync(Cabinet cabinet, long warehouseId)
    {
      var allResponse = await GetWarehousesAsync(cabinet);
      if (allResponse.Status == YMApiResponseStatusType.OK && allResponse.Result != null)
      {
        var warehouse = allResponse.Result.Items.FirstOrDefault(w => w.Id == warehouseId);
        return warehouse;
      }
      else
      {
        return default;
      }
    }

    #endregion

  }


  #region Response Models


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
  /// Информация о товаре в заявке.
  /// </summary>
  public class YMSupplyRequestItem
  {
    [JsonIgnore, Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }  // для EF Core

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

    [JsonProperty("counters")]
    public YMSupplyRequestItemCounters Counters { get; set; }


    [ForeignKey(nameof(SupplyRequest))]
    [JsonIgnore]
    public long SupplyRequestId { get; set; }

    [JsonIgnore, System.Text.Json.Serialization.JsonIgnore]
    public YMSupplyRequest SupplyRequest { get; set; }
  }

  /// <summary>
  /// Количество товаров в заявке.
  /// </summary>
  public class YMSupplyRequestItemCounters
  {
    [JsonIgnore, Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }  // для EF Core

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

    // обратная связь, если нужно (можно отключить):
    [JsonIgnore, System.Text.Json.Serialization.JsonIgnore]
    public YMSupplyRequestItem? Item { get; set; }
  }

  #endregion

  #region Yandex Market supplies DTOs

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

  ///// <summary>
  ///// Модель ответа с данными заявок.
  ///// </summary>
  //public class YMGetSupplyRequests
  //{
  //  /// <summary>
  //  /// Список заявок.
  //  /// </summary>
  //  [JsonProperty("requests")]
  //  [Display(Name = "Список заявок")]
  //  public List<YMSupplyRequest> Requests { get; set; }

  //  /// <summary>
  //  /// Пагинация — идентификатор следующей страницы.
  //  /// </summary>
  //  [JsonProperty("paging")]
  //  [Display(Name = "Идентификатор следующей страницы")]
  //  public YMForwardScrollingPager Paging { get; set; }
  //}

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
    //[JsonIgnore, Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    //public long Id { get; set; }  // для EF Core

    /// <summary>Внутренний идентификатор заявки.</summary>
    [JsonProperty("id")]
    [Display(Name = "Внутренний ID заявки")]
    [Key]
    public long Id { get; set; }

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
    [Key]
    public long Id { get; set; }

    [JsonProperty("fullAddress")]
    [Display(Name = "Полный адрес")]
    public string? FullAddress { get; set; }

    [JsonProperty("gps")]
    [Display(Name = "GPS-координаты")]
    public YMGps Gps { get; set; }

    [JsonIgnore, System.Text.Json.Serialization.JsonIgnore]
    public ICollection<YMSupplyRequestLocation>? LocationAddresses { get; set; }
  }

  /// <summary>
  /// Информация о складе хранения или ПВЗ.
  /// </summary>
  public class YMSupplyRequestLocation
  {
    [Key]
    [JsonProperty("serviceId")]
    [Display(Name = "Идентификатор склада/партнёра")]
    public long ServiceId { get; set; }

    [JsonProperty("name")]
    [Display(Name = "Название")]
    public string? Name { get; set; }

    [JsonProperty("type")]
    [Display(Name = "Тип склада/ПВЗ")]
    public YMSupplyRequestLocationType Type { get; set; }

    [JsonProperty("requestedDate")]
    [Display(Name = "Дата и время поставки")]
    public DateTime? RequestedDate { get; set; }

    // ─── Адрес ──────────────────────────────

    public long AddressId { get; set; }

    [ForeignKey(nameof(AddressId))]
    [JsonProperty("address")]
    [Display(Name = "Адрес")]
    public YMSupplyRequestLocationAddress Address { get; set; }

    // ─── Обратные связи ─────────────────────
    [JsonIgnore, System.Text.Json.Serialization.JsonIgnore]
    public ICollection<YMSupplyRequest>? AsTargetInRequests { get; set; }
    [JsonIgnore, System.Text.Json.Serialization.JsonIgnore]
    public ICollection<YMSupplyRequest>? AsTransitInRequests { get; set; }

    public override string ToString()
    {
      StringBuilder sb = new StringBuilder();
      sb.AppendLine($"Склад/ПВЗ: {Name}");
      sb.AppendLine($"Тип: {Type}");
      sb.AppendLine($"Адрес: {Address?.FullAddress}");
      return sb.ToString();
    }
  }


  /// <summary>
  /// Ссылка на связанную заявку.
  /// </summary>
  public class YMSupplyRequestReference
  {
    [Key]
    public long Id { get; set; }


    // ─── Ссылка «от кого» (Request → ChildrenLinks) ────────────────────────

    [ForeignKey(nameof(Request))]
    [JsonIgnore, System.Text.Json.Serialization.JsonIgnore]
    public long RequestId { get; set; }

    [JsonIgnore, System.Text.Json.Serialization.JsonIgnore]
    public YMSupplyRequest Request { get; set; }

    // ─── Ссылка «на кого» (RelatedRequest ← ParentLink) ────────────────────

    [ForeignKey(nameof(RelatedRequest))]
    public long? RelatedRequestId { get; set; }

    [JsonIgnore, System.Text.Json.Serialization.JsonIgnore]
    public YMSupplyRequest? RelatedRequest { get; set; }

    /// <summary>Идентификаторы связанной заявки.</summary>
    [JsonProperty("id")]
    [Display(Name = "Идентификаторы связанной заявки"), NotMapped]
    public YMSupplyRequestId? YMSupplyRequestId { get; set; }

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
