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
using automation.mbtdistr.ru.Services.YandexMarket;
using automation.mbtdistr.ru.Services.YandexMarket.Models;
using Google.Apis.Sheets.v4.Data;
using Newtonsoft.Json;
using System.Text.Unicode;

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
    [HttpGet("ym/supply-requests")]
    public async Task<IActionResult> GetYandexMarketSupplyRequests()
    {
      var cabinets = await _db.Cabinets
        .AsNoTracking()
        .Include(c => c.Settings)
        .ThenInclude(cs => cs.ConnectionParameters)
        .Where(c => c.Marketplace == "YANDEXMARKET")
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
            if (supplyResponse?.Result?.Requests?.Count > 0)
            {
              supplyRequests.AddRange(supplyResponse.Result.Requests);
            }
          }
        }
      }

      List<YMSupplyRequestLocation> locations = new List<YMSupplyRequestLocation>();

      //проходим по всем заявкам и выбираем адреса
      foreach (var request in supplyRequests)
      {
        //проверяем обьекты YMSupplyRequestLocation на наличие в массиве locations и если нет, то добавляем

        var transitLocation = request.TransitLocation;
        var targetLocation = request.TargetLocation;
        if (transitLocation != null && locations.FirstOrDefault(l => l.Id == transitLocation.Id) == null)
        {
          locations.Add(transitLocation);
        }
        if (targetLocation != null && locations.FirstOrDefault(l => l.Id == targetLocation.Id) == null)
        {
          locations.Add(targetLocation);
        }
      }
      //проверяем обьекты YMSupplyRequestLocation на наличие в базе и если нет, то добавляем
      foreach (var location in locations)
      {
        var dbLocation = await _db.YMLocations.AsNoTracking().FirstOrDefaultAsync(l => l.Id == location.Id);
        if (dbLocation == null)
        {
          _db.YMLocations.Add(location);
          await _db.SaveChangesAsync();
        }
        else
        {
          dbLocation = location;
          _db.YMLocations.Update(dbLocation);
          await _db.SaveChangesAsync();
        }
      }

      //проверяем обьекты YMSupplyRequest на наличие в базе и если нет, то добавляем, если есть то обновляем
      var dbSupplyRequests = await _db.YMSupplyRequests.AsNoTracking().ToListAsync();
      foreach (var request in supplyRequests)
      {
        try
        {
          var dbRequest = dbSupplyRequests.FirstOrDefault(r => r.Id == request.Id);
          if (dbRequest == null)
          {
            _db.YMSupplyRequests.Add(request);
          }
          else
          {
            dbRequest = request;
            _db.YMSupplyRequests.Update(dbRequest);
          }
          await _db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
          await Extensions.SendDebugMessage($"public async Task<IActionResult> GetYandexMarketSupplyRequests()\n{ex.Message}");
        }
      }
      var json = Newtonsoft.Json.JsonConvert.SerializeObject(supplyRequests, Formatting.Indented, new JsonSerializerSettings()
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
  }
}