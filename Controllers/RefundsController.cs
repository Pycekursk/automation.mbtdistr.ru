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
using System.Text.Json;
using System.Text.Json.Serialization;

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

    public RefundsController(ApplicationDbContext db, WildberriesApiService wb, OzonApiService oz, DriveApiService drive, SheetsApiService sheets)
    {
      _db = db;
      _wb = wb;
      _oz = oz;
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
      return Ok(JsonSerializer.Serialize(response));
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



  }

}