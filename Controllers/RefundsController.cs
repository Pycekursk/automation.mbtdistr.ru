using automation.mbtdistr.ru.Data;
using automation.mbtdistr.ru.Models;
using automation.mbtdistr.ru.Services.Ozon;
using automation.mbtdistr.ru.Services.Wildberries;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using System.Text.Json;

namespace automation.mbtdistr.ru.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  //[AuthorizeRole(RoleType.Admin, RoleType.ClaimsManager, RoleType.Director)]
  public class RefundsController : ControllerBase
  {
    [HttpGet]
    public IActionResult Get()
    {
      object response = new
      {
        Message = "RefundsController",
        Description = "Контроллер для работы с возвратами"
      };
      //var settings = _db.CabinetSettings.ToArray();
      //var arr = _db.ConnectionParameters.ToArray();

      //var cabinets = _db.Cabinets
      //    .Include(c => c.Settings)
      //    .ThenInclude(s => s.ConnectionParameters)
      //    .ToList();

      //Cabinet cabinet = new Cabinet
      //{
      //  Marketplace = "Ozon",
      //  Name = "Титан Электроникс",
      //  Settings = new CabinetSettings
      //  {
      //    ConnectionParameters = new List<ConnectionParameter>
      //    {
      //      new ConnectionParameter
      //      {
      //        Key = "ClientId",
      //        Value = "2198559"
      //      },
      //      new ConnectionParameter
      //      {
      //        Key = "Api-Key",
      //        Value = "08ae6742-29ef-42d4-bdc0-bdd3ad750df6"
      //      }
      //    }
      //  }
      //};
      //_db.Cabinets.Add(cabinet);
      //var changes = _db.SaveChanges();


      //var cabinets = _db.Cabinets
      //    .Include(c => c.Settings)
      //    .ThenInclude(s => s.ConnectionParameters)
      //    .ToList();

      return Ok(JsonSerializer.Serialize(response));
    }

    private readonly WildberriesApiService _wb;
    private readonly OzonApiService _oz;
    private readonly ApplicationDbContext _db;

    public RefundsController(ApplicationDbContext db, WildberriesApiService wb, OzonApiService oz)
    {
      _db = db;
      _wb = wb;
      _oz = oz;
    }

    // GET api/returns/wildberries/{cabinetId}/seller-info  
    [HttpGet("wildberries/{cabinetId:int}/seller-info")]
    public async Task<IActionResult> WildberriesSellerInfo(int cabinetId)
        => Ok(await _wb.GetSellerInfoAsync(cabinetId));

    // GET api/returns/ozon/{cabinetId}/products  
    [HttpGet("ozon/{cabinetId:int}/returns")]
    public async Task<IActionResult> OzonProducts(int cabinetId)
    {
      var cabinet = new Cabinet { Id = cabinetId }; // Assuming Cabinet has an Id property  
      return Ok(await _oz.GetReturnsListAsync(cabinet.Id, new Services.Ozon.Models.Filter()));
    }
  }
}
