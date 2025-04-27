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

    private readonly WildberriesApiService _wb;
    private readonly OzonApiService _oz;
    private readonly ApplicationDbContext _db;

    public RefundsController(ApplicationDbContext db, WildberriesApiService wb, OzonApiService oz)
    {
      _db = db;
      _wb = wb;
      _oz = oz;
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
