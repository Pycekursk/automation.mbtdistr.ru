using automation.mbtdistr.ru.Data;
using automation.mbtdistr.ru.Models;
using automation.mbtdistr.ru.Services.Google.Drive;
using automation.mbtdistr.ru.Services.Google.Sheets;
using automation.mbtdistr.ru.Services.Ozon;
using automation.mbtdistr.ru.Services.Wildberries;
using automation.mbtdistr.ru.Services.Ozon.Models;


using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using System.ComponentModel.DataAnnotations;
using automation.mbtdistr.ru.Services.YandexMarket.Models;
using Google.Apis.Sheets.v4.Data;
using Newtonsoft.Json;
using System.Text.Unicode;
using System.Globalization;
using System.Linq.Expressions;
using Newtonsoft.Json.Converters;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Runtime.Serialization;
using System.Text;
using Telegram.Bot.Types;
using System;
using automation.mbtdistr.ru.Services.YandexMarket;

namespace automation.mbtdistr.ru.Controllers
{
  [ApiController, Route("api/[controller]"), ApiExplorerSettings(IgnoreApi = false)]
  public class RefundsController : ControllerBase
  {
    private readonly WildberriesApiService _wb;
    private readonly OzonApiService _oz;
    private readonly ApplicationDbContext _db;
    private readonly DriveApiService _drive;
    private readonly SheetsApiService _sheets;
    private readonly YMApiService _ym;

    public RefundsController(ApplicationDbContext db, WildberriesApiService wb, OzonApiService oz, YMApiService ym, DriveApiService drive, SheetsApiService sheets)
    {
      _db = db;
      _wb = wb;
      _oz = oz;
      _ym = ym;
      _drive = drive;
      _sheets = sheets;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
      object response = new
      {
        Message = "RefundsController",
        Description = "Контроллер для работы с возвратами"
      };
      return Ok(JsonConvert.SerializeObject(response));
    }

    [HttpGet("createtable")]
    public async Task<IActionResult> CreateAndShareAsync([FromQuery] string title)
    {
      var spreadsheet = await _sheets.CreateAndShareAsync(title);

      return Redirect(spreadsheet.SpreadsheetUrl);
    }

    [HttpGet("gettable")]
    public async Task<IActionResult> GetTableAsync([FromQuery] string id)
    {
      var spreadsheet = await _sheets.GetSpreadsheetByIdAsync(id);
      return Redirect(spreadsheet.SpreadsheetUrl);
    }

    [HttpPost("wh/ozon")]
    public async Task<IActionResult> OzonWh(object data)
    {
      if (!Program.Environment.IsDevelopment())
      {
        try
        {
          //string logFilePath = Path.Combine("wwwroot", "logs", "ozon", $"{DateTime.UtcNow:yyyy-MM-dd}.log");
          //Directory.CreateDirectory(Path.GetDirectoryName(logFilePath) ?? string.Empty);
          //await System.IO.File.WriteAllTextAsync(logFilePath, data.ToString());
          //await Extensions.SendDebugObject<object>(data, "public async Task<IActionResult> OzonWh(object data)");
        }
        catch (Exception ex)
        {
          await Extensions.SendDebugMessage(ex.Message);
        }
      }

      try
      {
        var json = data.ToString();

        var notify = json?.FromJson<OzonNotify>();


        if (notify?.MessageType == OzonNotifyType.TYPE_PING)
        {
          var response = new OzonCheckResponse
          {
            Version = "1.0",
            Name = "MBTOzon",
            Time = DateTime.UtcNow
          };
          return Ok(response);
        }
      }
      catch (Exception ex)
      {
        await Extensions.SendDebugMessage($"public async Task<IActionResult> OzonWh(object data)\n{ex.Message}");
      }



      return Ok(new { result = true });
    }


    //метод получения заявок на вывоз от яндекс маркета
    [HttpGet("ym/sync-supplies")]
    public async Task<IActionResult> SyncYandexMarketSupplies()
    {
      var cabinets = await _db.Cabinets
        .AsNoTracking()
        .Include(c => c.Settings)
        .ThenInclude(cs => cs.ConnectionParameters)
        .Where(c => c.Marketplace.ToUpper() == "YANDEXMARKET")
        .ToListAsync();

      List<YMSupplyRequest> supplyRequests = new List<YMSupplyRequest>();

      foreach (var c in cabinets)
      {
        var campaignsResponse = await _ym.GetCampaignsAsync(c);
        if (campaignsResponse != null && campaignsResponse.Campaigns?.Count > 0)
        {
          foreach (var campaign in campaignsResponse.Campaigns)
          {
            if (campaign.PlacementType != "FBY") continue;
            var supplyResponse = await _ym.GetSupplyRequests(c, campaign);
            if (supplyResponse?.Result?.Items?.Count > 0)
            {
              supplyResponse.Result.Items.ForEach(r => r.CabinetId = c.Id);
              var supplies = supplyResponse.Result.Items;
              foreach (var supply in supplies)
              {
                if (supply.Status == YMSupplyRequestStatusType.Finished || supply.Status == YMSupplyRequestStatusType.Cancelled) continue;
                var itemsResponse = await _ym.GetSupplyRequestItemsAsync(c, campaign, supply.ExternalId.Id);
                var items = itemsResponse?.Result?.Items;
                if (items != null && items.Count > 0)
                {
                  supply.Items = items;
                }
                else
                {
                  supply.Items = new List<YMSupplyRequestItem>();
                }
              }
              supplyRequests.AddRange(supplyResponse.Result.Items);
            }
          }
        }
      }

      var saved = new List<YMSupplyRequest>();
      foreach (var request in supplyRequests)
      {
        var supply = await _ym.AddOrUpdateSupplyRequestAsync(request, _db);



        if (supply != null)
        {
          saved.Add(supply);
        }
      }

      var json = Newtonsoft.Json.JsonConvert.SerializeObject(saved, Formatting.Indented, new JsonSerializerSettings()
      {
        Culture = System.Globalization.CultureInfo.CurrentCulture,
        StringEscapeHandling = StringEscapeHandling.Default,
        Converters = { new Newtonsoft.Json.Converters.StringEnumConverter() },
      });
      ContentResult contentResult = new ContentResult
      {
        Content = json,
        ContentType = "application/json",
        StatusCode = 200
      };
      return contentResult;
    }

    //метод синхронизации возвратов от яндекс маркета
    [HttpGet("ym/sync-returns")]
    public async Task<IActionResult> SyncYandexMarketReturns()
    {
      var cabinets = await _db.Cabinets
        .AsNoTracking()
        .Include(c => c.Settings)
        .ThenInclude(cs => cs.ConnectionParameters)
        .Where(c => c.Marketplace.ToUpper() == "YANDEXMARKET")
        .ToListAsync();

      if (Program.Environment.IsDevelopment())
      {
        cabinets = new List<Cabinet> { cabinets.FirstOrDefault(c => c.Id == 14) };
      }

      List<Return> returns = new List<Return>();
      foreach (var c in cabinets)
      {
        var campaignsResponse = await _ym.GetCampaignsAsync(c);
        if (campaignsResponse != null && campaignsResponse.Campaigns?.Count > 0)
        {
          foreach (var campaign in campaignsResponse.Campaigns)
          {
            var returnResponse = await _ym.GetReturnsListAsync(c, campaign);

            if (returnResponse?.Result?.Items?.Count > 0)
            {
              foreach (var ret in returnResponse.Result.Items)
              {
                var dbChangeDate = _db.Returns.Where(r => r.ReturnId == ret.Id.ToString()).Select(r => r.ChangedAt).FirstOrDefault();
                if (dbChangeDate != null && dbChangeDate == ret.UpdateDate)
                  continue;

                ret.Order = (await _ym.GetOrdersAsync(c, campaign, new long[] { ret.OrderId }))?.Items?.FirstOrDefault();
                if (ret.Items?.Count > 0)
                {
                  var warehouse = await _ym.GetWarehouseByIdAsync(c, ret.LogisticPickupPoint.Id);

                  foreach (var item in ret.Items)
                  {
                    var decision = item?.Decisions?.FirstOrDefault();
                    if (decision != null && decision.Images?.Count > 0)
                    {
                      List<string> imagesUrl = new List<string>();
                      foreach (var img in decision.Images)
                      {
                        var fileName = $"{ret.OrderId}_{ret.Id}_{decision.ReturnItemId}_{img}.jpg";
                        var filePath = Path.Combine("wwwroot", "images", "returns", fileName);
                        var fileDir = Path.GetDirectoryName(filePath);
                        if (!Directory.Exists(fileDir))
                          Directory.CreateDirectory(fileDir);

                        if (System.IO.File.Exists(filePath))
                          continue;

                        var image = await _ym.GetReturnImageAsync(c, campaign, ret.OrderId, ret.Id, decision.ReturnItemId, img);
                        var imageBytes = Convert.FromBase64String(image.Result.ImageData);
                        await System.IO.File.WriteAllBytesAsync(filePath, imageBytes);
                        var fileUrl = $"{Request.Scheme}://{Request.Host}/images/returns/{fileName}";
                        imagesUrl.Add(fileUrl);
                      }
                      decision.Images = imagesUrl;
                    }
                  }

                }
                var @return = Return.Parse<YMReturn>(ret);
                @return.CabinetId = c.Id;
                returns.Add(@return);
              }
            }
          }
        }
      }

      var json = Newtonsoft.Json.JsonConvert.SerializeObject(returns, Formatting.Indented, new Newtonsoft.Json.JsonSerializerSettings()
      {
        Culture = System.Globalization.CultureInfo.CurrentCulture,
        StringEscapeHandling = StringEscapeHandling.Default,
        Converters = { new Newtonsoft.Json.Converters.StringEnumConverter() },
      });
      ContentResult contentResult = new ContentResult
      {
        Content = json,
        ContentType = "application/json",
        StatusCode = 200
      };
      return contentResult;
    }

    [HttpGet("ym/get-supplies")]
    public async Task<IActionResult> GetYandexMarketSupplyRequests([FromQuery] string[]? filters)
    {
      // 1) Берём IQueryable без Include
      IQueryable<YMSupplyRequest> query = _db.YMSupplyRequests
          .AsNoTracking();

      // 2) Применяем динамические enum-фильтры
      if (filters != null && filters.Length > 0)
        query = ApplyEnumFilters(query, filters);

      // 3) Дальше уже делаем Include на IQueryable — обе перегрузки Include будут правильными
      query = query
          .Include(r => r.TransitLocation)
          .ThenInclude(l => l.Address)
          .ThenInclude(a => a.Gps)
          .Include(r => r.TargetLocation)
          .ThenInclude(l => l.Address)
          .ThenInclude(a => a.Gps)
          .Include(r => r.ExternalId)
          //.Include(r => r.ParentLink)
          //.Include(r => r.ChildrenLinks)
          .Include(r => r.Counters);



      // 4) Выполняем запрос
      var supplyRequests = await query.ToListAsync();

      // 5) Сериализуем в JSON
      var json = JsonConvert.SerializeObject(
          supplyRequests,
          Formatting.Indented,
          new JsonSerializerSettings
          {
            Culture = CultureInfo.CurrentCulture,
            StringEscapeHandling = StringEscapeHandling.Default,
            Converters = { new StringEnumConverter() }
          });

      return new ContentResult
      {
        Content = json,
        ContentType = "application/json",
        StatusCode = 200
      };
    }

    #region Enum filter methods
    private static IQueryable<T> ApplyEnumFilters<T>(IQueryable<T> source, string[] filters)
    {
      var entityType = typeof(T);
      var parameter = Expression.Parameter(entityType, "x");

      // 1) Составляем словарь: json-имя свойства → PropertyInfo
      var allProps = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
      var jsonNameMap = allProps
          .Where(p => p.PropertyType.IsEnum)
          .ToDictionary(
              p => GetJsonName(p).ToLowerInvariant(),
              p => p);

      // 2) Разбираем переданные фильтры, нормализуем json-имя в нижний регистр
      var parsed = filters
          .Select(f => f.Split(new[] { '=' }, 2))
          .Where(a => a.Length == 2)
          .Select(a => new
          {
            JsonNameKey = a[0].Trim().ToLowerInvariant(),
            Value = a[1].Trim()
          })
          .ToList();

      Expression? finalPredicate = null;

      // 3) Группируем по JsonNameKey (OR внутри группы, AND между группами)
      foreach (var grp in parsed.GroupBy(x => x.JsonNameKey))
      {
        if (!jsonNameMap.TryGetValue(grp.Key, out var propInfo))
          continue;

        Expression? groupExpr = null;
        foreach (var item in grp)
        {
          if (!TryParseEnumValue(propInfo.PropertyType, item.Value, out var enumVal))
            continue;

          // x => x.Prop == enumVal
          var member = Expression.Property(parameter, propInfo);
          var constant = Expression.Constant(enumVal, propInfo.PropertyType);
          var equal = Expression.Equal(member, constant);

          groupExpr = groupExpr == null
              ? equal
              : Expression.OrElse(groupExpr, equal);
        }

        if (groupExpr != null)
        {
          finalPredicate = finalPredicate == null
              ? groupExpr
              : Expression.AndAlso(finalPredicate, groupExpr);
        }
      }

      if (finalPredicate == null)
        return source;

      var lambda = Expression.Lambda<Func<T, bool>>(finalPredicate, parameter);
      return source.Where(lambda);
    }

    /// <summary>
    /// Пытается сопоставить строку со значением enum:
    ///   1) ищет в EnumMemberAttribute.Value (ignore case),
    ///   2) затем пробует Enum.TryParse(ignore case).
    /// </summary>
    private static bool TryParseEnumValue(Type enumType, string value, out object enumValue)
    {
      // 1) Смотрим на EnumMemberAttribute
      foreach (var field in enumType.GetFields(BindingFlags.Public | BindingFlags.Static))
      {
        var em = field.GetCustomAttribute<EnumMemberAttribute>();
        if (em != null && string.Equals(em.Value, value, StringComparison.OrdinalIgnoreCase))
        {
          enumValue = field.GetValue(null)!;
          return true;
        }
      }
      // 2) Фоллбэк — по имени enum (ignore case)
      if (Enum.TryParse(enumType, value, ignoreCase: true, out var parsed))
      {
        enumValue = parsed!;
        return true;
      }

      enumValue = null!;
      return false;
    }

    /// <summary>
    /// Получает имя поля для JSON (JsonProperty / JsonPropertyName) или CLR-имя.
    /// </summary>
    private static string GetJsonName(PropertyInfo prop)
    {
      var newton = prop.GetCustomAttribute<JsonPropertyAttribute>();
      if (newton != null && !string.IsNullOrWhiteSpace(newton.PropertyName))
        return newton.PropertyName;

      var sys = prop.GetCustomAttribute<JsonPropertyNameAttribute>();
      if (sys != null && !string.IsNullOrWhiteSpace(sys.Name))
        return sys.Name;

      return prop.Name;
    }

    #endregion
  }
}