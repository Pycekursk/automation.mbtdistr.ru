using automation.mbtdistr.ru.Data;
using automation.mbtdistr.ru.Models;
using automation.mbtdistr.ru.Services.Google.Drive;
using automation.mbtdistr.ru.Services.Google.Sheets;
using automation.mbtdistr.ru.Services.Ozon;
using automation.mbtdistr.ru.Services.Wildberries;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using System.Text.Json;

namespace automation.mbtdistr.ru.Controllers
{
  [Route("api/[controller]"), ApiExplorerSettings(IgnoreApi = true)]
  [ApiController]
  //[AuthorizeRole(RoleType.Admin, RoleType.ClaimsManager, RoleType.Director)]
  public class RefundsController : ControllerBase
  {

    private readonly WildberriesApiService _wb;
    private readonly OzonApiService _oz;
    private readonly ApplicationDbContext _db;
    private readonly DriveApiService _driveApi;
    private readonly SheetsApiService _sheetsApi;

    public RefundsController(ApplicationDbContext db, WildberriesApiService wb, OzonApiService oz, DriveApiService driveApi, SheetsApiService sheetsApi)
    {
      _db = db;
      _wb = wb;
      _oz = oz;
      _driveApi = driveApi;
      _sheetsApi = sheetsApi;
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
      var spreadsheet = await _sheetsApi.CreateAndShareAsync(title);

      return Redirect(spreadsheet.SpreadsheetUrl);
    }

    [HttpGet("gettable")]
    public async Task<IActionResult> GetTableAsync([FromQuery] string id)
    {
      var spreadsheet = await _sheetsApi.GetSpreadsheetByIdAsync(id);
      return Redirect(spreadsheet.SpreadsheetUrl);
    }



    //// GET api/returns/wildberries/{cabinetId}/seller-info  
    //[HttpGet("wildberries/{cabinetId:int}/seller-info")]
    //public async Task<IActionResult> WildberriesSellerInfo(int cabinetId)
    //    => Ok(await _wb.GetSellerInfoAsync(cabinetId));

    //// GET api/returns/ozon/{cabinetId}/products  
    //[HttpGet("ozon/{cabinetId:int}/returns")]
    //public async Task<IActionResult> OzonProducts(int cabinetId)
    //{
    //  var cabinet = new Cabinet { Id = cabinetId }; // Assuming Cabinet has an Id property  
    //  return Ok(await _oz.GetReturnsListAsync(cabinet, new Services.Ozon.Models.Filter()));
    //}
  }
}
