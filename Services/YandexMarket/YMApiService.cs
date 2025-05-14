using automation.mbtdistr.ru.Data;
using automation.mbtdistr.ru.Models;
using automation.mbtdistr.ru.Services.Ozon;
using automation.mbtdistr.ru.Services.YandexMarket.Models;

using Microsoft.EntityFrameworkCore;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Text;



//using System.Text.Json.Serialization;

//using System.Text.Json.Serialization;


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

    public async Task<ReturnsListResponse?> GetReturnsListAsync(Cabinet cabinet, Campaign campaign, YMFilter? filter = null, int limit = 100, string? pageToken = null)
    {
      if (filter == null)
      {
        filter = new YMFilter
        {
          FromDate = DateTime.Now.AddDays(-20).ToString("yyyy-MM-dd"),
          ToDate = DateTime.Now.ToString("yyyy-MM-dd"),
          Limit = limit,
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

        var obj = Newtonsoft.Json.JsonConvert.DeserializeObject<ReturnsListResponse>(json, new JsonSerializerSettings()
        {
          Converters = new List<JsonConverter>
          {
            new StringEnumConverter()
          }
        });

        if (obj?.Result?.Paging?.NextPageToken is string e && !string.IsNullOrEmpty(e))
          await Extensions.SendDebugMessage($"В компании {campaign.Domain} ({campaign.Id}) с кабинетом {cabinet.Id} найдено {obj.Result.Paging.Total} возвратов. Нужна обработка постаничной загрузки");

        return obj;
      }
      catch (Exception ex)
      {
        await Extensions.SendDebugMessage($"Ошибка получения списка возвратов: {ex.Message}");
        return default;
      }
    }

    public async Task<YMApiResponse<YMGetSupplyRequests>> GetSupplyRequests(Cabinet cabinet, Campaign campaign, int limit = 100, YMFilter? filter = null, string? pageToken = null)
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
        var obj = Newtonsoft.Json.JsonConvert.DeserializeObject<YMApiResponse<YMGetSupplyRequests>>(json, new JsonSerializerSettings()
        {
          StringEscapeHandling = StringEscapeHandling.Default,
          Culture = System.Globalization.CultureInfo.CurrentCulture,
          Converters = new List<Newtonsoft.Json.JsonConverter>
          {
            new StringEnumConverter()
          }
        });
        if (obj?.Result?.Paging.NextPageToken is string s && !string.IsNullOrEmpty(s))
          await Extensions.SendDebugMessage($"В компании {campaign.Domain} ({campaign.Id}) нужна обработка постаничной загрузки");
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
    public async Task<YMApiResponse<YMSupplyRequestItemsResult>> GetSupplyRequestItemsAsync(
        Cabinet cabinet,
        Campaign campaign,
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
        var result = JsonConvert.DeserializeObject<YMApiResponse<YMSupplyRequestItemsResult>>(json, new JsonSerializerSettings
        {
          StringEscapeHandling = StringEscapeHandling.Default,
          Culture = System.Globalization.CultureInfo.CurrentCulture,
          Converters = new List<JsonConverter>()
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
    public async Task<YMApiResponse<YMSupplyRequestDocumentsResult>> GetSupplyRequestDocumentsAsync(
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
        await Extensions.SendDebugMessage($"Ошибка получения документов в заявке: {ex.Message}");
        return default;
      }
    }


    //public async Task<YMSupplyRequest> AddOrUpdateSupplyRequestAsync(YMSupplyRequest incoming, ApplicationDbContext context)
    //{
    //  var existing = await context.YMSupplyRequests
    //      .Include(r => r.TargetLocation).ThenInclude(l => l.Address)
    //      .Include(r => r.TransitLocation).ThenInclude(l => l.Address)
    //      .FirstOrDefaultAsync(r => r.ExternalIdId == incoming.ExternalId.Id);

    //  // ─── Обеспечить привязку к существующим или новым локациям ──────────────

    //  async Task<YMSupplyRequestLocation> ResolveLocationAsync(YMSupplyRequestLocation inputLocation)
    //  {
    //    var existingLoc = await context.YMSupplyRequestLocations
    //        .Include(l => l.Address)
    //        .FirstOrDefaultAsync(l => l.ServiceId == inputLocation.ServiceId);

    //    if (existingLoc != null)
    //    {
    //      // Обновить
    //      existingLoc.Name = inputLocation.Name;
    //      existingLoc.RequestedDate = inputLocation.RequestedDate;
    //      existingLoc.Type = inputLocation.Type;

    //      if (inputLocation.Address != null)
    //      {
    //        var existingAddr = await context.YMLocationAddresses
    //            .FirstOrDefaultAsync(a => a.Id == inputLocation.Address.Id);

    //        if (existingAddr != null)
    //        {
    //          existingAddr.FullAddress = inputLocation.Address.FullAddress;
    //          existingAddr.Gps = inputLocation.Address.Gps;
    //        }
    //        else
    //        {
    //          context.YMLocationAddresses.Add(inputLocation.Address);
    //          existingLoc.Address = inputLocation.Address;
    //        }
    //      }

    //      return existingLoc;
    //    }
    //    else
    //    {
    //      // Новая локация
    //      if (inputLocation.Address != null)
    //      {
    //        var addressExists = await context.YMLocationAddresses
    //            .AnyAsync(a => a.Id == inputLocation.Address.Id);

    //        if (!addressExists)
    //        {
    //          context.YMLocationAddresses.Add(inputLocation.Address);
    //        }
    //      }

    //      context.YMSupplyRequestLocations.Add(inputLocation);
    //      return inputLocation;
    //    }
    //  }

    //  // ─── Обработка TargetLocation ─────────────
    //  var targetLocation = await ResolveLocationAsync(incoming.TargetLocation);
    //  var transitLocation = incoming.TransitLocation != null
    //      ? await ResolveLocationAsync(incoming.TransitLocation)
    //      : null;

    //  if (existing == null)
    //  {
    //    // ─── Новая заявка ─────────────
    //    incoming.TargetLocation = targetLocation;
    //    incoming.TargetLocationServiceId = targetLocation.ServiceId;

    //    if (transitLocation != null)
    //    {
    //      incoming.TransitLocation = transitLocation;
    //      incoming.TransitLocationServiceId = transitLocation.ServiceId;
    //    }

    //    context.YMSupplyRequests.Add(incoming);
    //  }
    //  else
    //  {
    //    // ─── Обновление существующей ─────────────
    //    existing.Status = incoming.Status;
    //    existing.Type = incoming.Type;
    //    existing.Subtype = incoming.Subtype;
    //    existing.UpdatedAt = DateTime.UtcNow;

    //    existing.TargetLocation = targetLocation;
    //    existing.TargetLocationServiceId = targetLocation.ServiceId;

    //    if (transitLocation != null)
    //    {
    //      existing.TransitLocation = transitLocation;
    //      existing.TransitLocationServiceId = transitLocation.ServiceId;
    //    }
    //    else
    //    {
    //      existing.TransitLocation = null;
    //      existing.TransitLocationServiceId = null;
    //    }
    //  }

    //  await context.SaveChangesAsync();

    //  return incoming;
    //}
    //public async Task<YMSupplyRequest> AddOrUpdateSupplyRequestAsync(
    //YMSupplyRequest incoming,
    //ApplicationDbContext context)
    //{
    //  // 1. Подгружаем существующую заявку вместе со всем необходимым
    //  var existing = await context.YMSupplyRequests
    //      .Include(r => r.TargetLocation).ThenInclude(l => l.Address)
    //      .Include(r => r.TransitLocation).ThenInclude(l => l.Address)
    //      .Include(r => r.Items).ThenInclude(i => i.Price)
    //      .Include(r => r.Items).ThenInclude(i => i.Counters)
    //      .Include(r => r.ChildrenLinks)
    //      .Include(r => r.ParentLink)
    //      .FirstOrDefaultAsync(r => r.ExternalIdId == incoming.ExternalId.Id);

    //  // 2. Локальные функции для «разрешения» вложенных сущностей

    //  // 2.1. Location
    //  async Task<YMSupplyRequestLocation> ResolveLocationAsync(YMSupplyRequestLocation loc)
    //  {
    //    var dbLoc = await context.YMSupplyRequestLocations
    //        .Include(l => l.Address)
    //        .FirstOrDefaultAsync(l => l.ServiceId == loc.ServiceId);

    //    if (dbLoc != null)
    //    {
    //      // обновляем поля
    //      dbLoc.Name = loc.Name;
    //      dbLoc.Type = loc.Type;
    //      dbLoc.RequestedDate = loc.RequestedDate;

    //      if (loc.Address != null)
    //      {
    //        var dbAddr = await context.YMLocationAddresses
    //            .FirstOrDefaultAsync(a => a.Id == loc.Address.Id);

    //        if (dbAddr != null)
    //        {
    //          dbAddr.FullAddress = loc.Address.FullAddress;
    //          dbAddr.Gps = loc.Address.Gps;
    //        }
    //        else
    //        {
    //          context.YMLocationAddresses.Add(loc.Address);
    //          dbLoc.Address = loc.Address;
    //        }
    //      }

    //      return dbLoc;
    //    }
    //    else
    //    {
    //      // новая локация
    //      if (loc.Address != null)
    //        context.YMLocationAddresses.Add(loc.Address);

    //      context.YMSupplyRequestLocations.Add(loc);
    //      return loc;
    //    }
    //  }

    //  // 2.2. Item (+ Counters + Price)
    //  async Task<YMSupplyRequestItem> ResolveItemAsync(YMSupplyRequestItem item, YMSupplyRequest parent)
    //  {
    //    return await Task.Run<YMSupplyRequestItem>(() =>
    //    {
    //      // пытаемся найти по OfferId внутри этой заявки
    //      var dbItem = existing?.Items?
    //          .FirstOrDefault(i => i.OfferId == item.OfferId);

    //      if (dbItem != null)
    //      {
    //        // обновляем простые поля
    //        dbItem.Name = item.Name;

    //        // обновляем цену
    //        dbItem.Price.CurrencyId = item.Price.CurrencyId;
    //        dbItem.Price.Value = item.Price.Value;

    //        // обновляем счётчики
    //        dbItem.Counters.DefectCount = item.Counters.DefectCount;
    //        dbItem.Counters.FactCount = item.Counters.FactCount;
    //        dbItem.Counters.PlanCount = item.Counters.PlanCount;
    //        dbItem.Counters.ShortageCount = item.Counters.ShortageCount;
    //        dbItem.Counters.SurplusCount = item.Counters.SurplusCount;

    //        return dbItem;
    //      }
    //      else
    //      {
    //        // новый товар
    //        item.SupplyRequest = parent;
    //        context.YMSupplyRequestItems.Add(item);
    //        return item;
    //      }
    //    });
    //  }

    //  // 2.3. Reference (ChildrenLinks / ParentLink)
    //  YMSupplyRequestReference ResolveReference(YMSupplyRequestReference link, YMSupplyRequest parent, bool isParentLink)
    //  {
    //    if (isParentLink)
    //    {
    //      // одиночная связь
    //      link.RequestId = parent.Id;
    //      link.RelatedRequestId = link.YMSupplyRequestId!.Id;
    //      link.Request = parent;
    //      context.YMSupplyRequestReferences.Add(link);
    //      return link;
    //    }
    //    else
    //    {
    //      // коллекция
    //      link.RequestId = parent.Id;
    //      link.RelatedRequestId = link.YMSupplyRequestId!.Id;
    //      link.Request = parent;
    //      context.YMSupplyRequestReferences.Add(link);
    //      return link;
    //    }
    //  }

    //  // 3. «Разрешаем» все вложенные объекты
    //  var targetLoc = await ResolveLocationAsync(incoming.TargetLocation);
    //  var transitLoc = incoming.TransitLocation != null
    //      ? await ResolveLocationAsync(incoming.TransitLocation)
    //      : null;

    //  // 4. Если это новая заявка — просто добавляем всё «вместе»
    //  if (existing == null)
    //  {
    //    incoming.TargetLocation = targetLoc;
    //    incoming.TargetLocationServiceId = targetLoc.ServiceId;
    //    if (transitLoc != null)
    //    {
    //      incoming.TransitLocation = transitLoc;
    //      incoming.TransitLocationServiceId = transitLoc.ServiceId;
    //    }

    //    // Items
    //    if (incoming.Items != null)
    //    {
    //      foreach (var it in incoming.Items.ToList())
    //        await ResolveItemAsync(it, incoming);
    //    }

    //    // ChildrenLinks
    //    if (incoming.ChildrenLinks != null)
    //    {
    //      foreach (var link in incoming.ChildrenLinks.ToList())
    //        ResolveReference(link, incoming, isParentLink: false);
    //    }

    //    // ParentLink
    //    if (incoming.ParentLink != null)
    //      ResolveReference(incoming.ParentLink, incoming, isParentLink: true);

    //    context.YMSupplyRequests.Add(incoming);
    //    await context.SaveChangesAsync();
    //    return incoming;
    //  }

    //  // 5. Обновляем существующую
    //  existing.Status = incoming.Status;
    //  existing.Subtype = incoming.Subtype;
    //  existing.Type = incoming.Type;
    //  existing.UpdatedAt = DateTime.UtcNow;

    //  existing.TargetLocation = targetLoc;
    //  existing.TargetLocationServiceId = targetLoc.ServiceId;

    //  if (transitLoc != null)
    //  {
    //    existing.TransitLocation = transitLoc;
    //    existing.TransitLocationServiceId = transitLoc.ServiceId;
    //  }
    //  else
    //  {
    //    existing.TransitLocation = null;
    //    existing.TransitLocationServiceId = null;
    //  }

    //  // — обновляем список товаров —
    //  if (incoming.Items != null)
    //  {
    //    // удаляем те, которых больше нет
    //    var incomingOffers = incoming.Items.Select(i => i.OfferId).ToHashSet();
    //    foreach (var toRemove in existing.Items.Where(i => !incomingOffers.Contains(i.OfferId)).ToList())
    //      context.YMSupplyRequestItems.Remove(toRemove);

    //    // добавляем/обновляем остальные
    //    foreach (var it in incoming.Items)
    //      await ResolveItemAsync(it, existing);
    //  }

    //  // — обновляем связи ChildrenLinks —
    //  if (incoming.ChildrenLinks != null)
    //  {
    //    // удаляем старые
    //    foreach (var old in existing.ChildrenLinks.ToList())
    //      context.YMSupplyRequestReferences.Remove(old);

    //    // добавляем новые
    //    foreach (var link in incoming.ChildrenLinks)
    //      ResolveReference(link, existing, isParentLink: false);
    //  }

    //  // — обновляем ParentLink —
    //  if (incoming.ParentLink != null)
    //  {
    //    if (existing.ParentLink != null)
    //      context.YMSupplyRequestReferences.Remove(existing.ParentLink);

    //    ResolveReference(incoming.ParentLink, existing, isParentLink: true);
    //  }

    //  await context.SaveChangesAsync();
    //  return existing;
    //}

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
          dbItem.Price.CurrencyId = item.Price.CurrencyId;
          dbItem.Price.Value = item.Price.Value;
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

      // 4) Обновляем ссылки: сначала удаляем все старые
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
    [JsonProperty("requestId"), System.Text.Json.Serialization.JsonPropertyName("requestId")]
    [Required]
    public long RequestId { get; set; }
  }

  #endregion

  #region Response Models



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

    ///// <summary>
    ///// Количество товаров в заявке.
    ///// </summary>
    //[Display(Name = "Количество товаров в заявке")]
    //[JsonProperty("counters"), System.Text.Json.Serialization.JsonPropertyName("counters")]
    //public YMSupplyRequestItemCounters Counters { get; set; }
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

  /// <summary>
  /// Валюта и ее значение.
  /// </summary>


  public class YMCurrencyValue
  {
    [JsonIgnore, Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }  // для EF Core

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
    public long Value { get; set; }

    // ─── Ссылка на товар ──────

    [ForeignKey(nameof(SupplyRequestItem))]
    [JsonIgnore]
    public int YMSupplyRequestItemId { get; set; }

    [JsonIgnore, System.Text.Json.Serialization.JsonIgnore]
    public YMSupplyRequestItem SupplyRequestItem { get; set; }
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

  ///// <summary>
  ///// Адрес склада или пункта выдачи.
  ///// </summary>
  //public class YMSupplyRequestLocationAddress
  //{
  //  [Key]
  //  public long Id { get; set; }  // для EF Core

  //  /// <summary>Полный адрес склада или ПВЗ.</summary>
  //  [JsonProperty("fullAddress")]
  //  [Display(Name = "Полный адрес")]
  //  public string? FullAddress { get; set; }

  //  /// <summary>GPS-координаты склада или ПВЗ.</summary>
  //  [JsonProperty("gps")]
  //  [Display(Name = "GPS-координаты")]
  //  public YMGps Gps { get; set; }

  //  public ICollection<YMSupplyRequestLocation>? LocationAddresses { get; set; }
  //}

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


  ///// <summary>
  ///// Информация о складе хранения или ПВЗ.
  ///// </summary>
  //public class YMSupplyRequestLocation
  //{
  //  /// <summary>Название склада или ПВЗ.</summary>
  //  [JsonProperty("name")]
  //  [Display(Name = "Название")]
  //  public string? Name { get; set; }

  //  /// <summary>Идентификатор склада или логистического партнёра.</summary>
  //  [JsonProperty("serviceId"), Key]
  //  [Display(Name = "Идентификатор склада/партнёра")]
  //  public long ServiceId { get; set; }

  //  public long AddressId { get; set; }

  //  /// <summary>Адрес склада или ПВЗ.</summary>
  //  [JsonProperty("address")]
  //  [Display(Name = "Адрес")]
  //  [ForeignKey(nameof(AddressId))]
  //  public YMSupplyRequestLocationAddress Address { get; set; }

  //  /// <summary>Тип склада или ПВЗ.</summary>
  //  [JsonProperty("type")]
  //  [Display(Name = "Тип склада/ПВЗ")]
  //  public YMSupplyRequestLocationType Type { get; set; }

  //  /// <summary>Дата и время поставки на склад или в ПВЗ.</summary>
  //  [JsonProperty("requestedDate")]
  //  [Display(Name = "Дата и время поставки")]
  //  public DateTime? RequestedDate { get; set; }

  //  public ICollection<YMSupplyRequest>? AsTargetInRequests { get; set; }
  //  public ICollection<YMSupplyRequest>? AsTransitInRequests { get; set; }
  //}

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
    public long RelatedRequestId { get; set; }

    [JsonIgnore, System.Text.Json.Serialization.JsonIgnore]
    public YMSupplyRequest RelatedRequest { get; set; }

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
