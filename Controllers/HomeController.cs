using System.Diagnostics;

using automation.mbtdistr.ru.Models;

using Microsoft.AspNetCore.Mvc;

namespace automation.mbtdistr.ru.Controllers
{
  public class HomeController : Controller
  {
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
      _logger = logger;
    }

    public IActionResult Index()
    {
      //если включен режим отладки, то переходим на метод Get RefundsController
      //if (Debugger.IsAttached)
      //{
      //  return RedirectToAction("Get", "Refunds");
      //}
      //иначе переходим на метод Index



      return View();
    }

    public IActionResult Privacy()
    {
      return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
      return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
  }
}
