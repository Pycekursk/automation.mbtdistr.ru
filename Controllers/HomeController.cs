using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;

using automation.mbtdistr.ru.Data;
using automation.mbtdistr.ru.Models;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace automation.mbtdistr.ru.Controllers
{
  [ApiExplorerSettings(IgnoreApi = true)]
  public class HomeController : Controller
  {
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _db;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext db)
    {
      _db = db;
      _logger = logger;
    }

    // Controllers/HomeController.cs
    public IActionResult Index([FromRoute(Name = "id")] int? userId)
    {
      if (userId == null)
      {
        return Redirect("https://t.me/MbtdistrBot");
      }

      // 1) собираем модель для меню
      var user = _db.Workers.Find(userId);

      var mainMenu = new List<MenuItem>();

      var model = new MainMenuViewModel
      {
        GreetingMessage = $"Привет, {user?.Name}! Вы {user.Role.GetDisplayName()}",
        Menu = mainMenu
      };
      return View(model);
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

    [HttpGet("botmenu/{id?}")]
    public IActionResult BotMenu([FromQuery] long? id)
    {
      var user = _db.Workers.FirstOrDefault(w => w.TelegramId == id.ToString());
      MainMenuViewModel mainMenu = new MainMenuViewModel();
      if (user != null)
      {
        mainMenu.GreetingMessage = $"Привет, {user.Name}! Вы {user.Role.GetDisplayName()}";
        mainMenu.Worker = user;
        mainMenu.Menu = new List<MenuItem>
        {
          new MenuItem
          {
            Icon = "📦",
            Action = "orders",
            Title = "Заказы"
          },
          new MenuItem
          {
            Icon = "📦",
            Action = "returns",
            Title = "Возвраты"
          },
          new MenuItem
          {
            Icon = "⚙️",
            Action = "settings",
            Title = "Настройки"
          }
        };
      }
      else
      {
        mainMenu.GreetingMessage = "Вы не зарегистрированы в системе. Пожалуйста, свяжитесь с администратором.";
        mainMenu.Menu = new List<MenuItem>();
      }
      return View(mainMenu);
    }
  }

  public class MainMenuViewModel
  {
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public string GreetingMessage { get; set; }
    public Worker Worker { get; set; }
    public List<MenuItem> Menu { get; set; }
  }

  public class MenuItem
  {
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public Worker Worker { get; set; }
    public string Icon { get; set; }
    public string Action { get; set; }
    public string Title { get; set; }
  }
}
