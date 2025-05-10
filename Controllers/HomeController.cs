using System.Diagnostics;
using System.Threading.Tasks;

using automation.mbtdistr.ru.Data;
using automation.mbtdistr.ru.Models;
using automation.mbtdistr.ru.Services;
using automation.mbtdistr.ru.Services.YandexMarket.Models;
using automation.mbtdistr.ru.ViewModels;

using Microsoft.AspNetCore.Html;
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
        mainMenu.WorkerId = user.Id;

        if (user.Role == RoleType.Guest)
        {
          mainMenu.GreetingMessage = "Вы не зарегистрированы в системе. Пожалуйста, свяжитесь с администратором.";
          mainMenu.Menu = new List<MenuItem>();
        }
        else if (user.Role == RoleType.Admin)
        {
          mainMenu.Menu = new List<MenuItem>
          {
            new MenuItem
            {
              Icon = "bi bi-gear-fill",
              Action = "orderslist",
              Title = "Заказы",
              CSS = "btn btn-outline-success"
            },
            new MenuItem
            {
              Icon = "bi bi-building",
              Action = "cabinetslist",
              Title = "Кабинеты",
              CSS = "btn btn-outline-primary"
            },
            new MenuItem
            {
              Icon = "bi bi-box-seam",
              Action = "returnslist",
              Title = "Возвраты",
              CSS = "btn btn-outline-danger"
            },
            new MenuItem
            {
              Icon = "bi bi-gear-fill",
              Action = "workersettings",
              Title = "Настройки",
              CSS = "btn btn-outline-light"
            }
          };
        }
        else if (user.Role == RoleType.ClaimsManager || user.Role == RoleType.CabinetManager)
        {
          mainMenu.GreetingMessage = $"Привет, {user.Name}! Вы {user.Role.GetDisplayName()}";

          //меню с кнопками: Кабинеты, Возвраты, Настройки
          mainMenu.Menu = new List<MenuItem>
          {
            new MenuItem
            {

              Action = "cabinetslist",
              Title = "Кабинеты",
              CSS = "btn btn-outline-primary",
              Icon = "bi bi-building"
            },
            new MenuItem
            {

              Action = "returnslist",
              Title = "Возвраты",
              CSS = "btn btn-outline-danger",
              Icon = "bi bi-box-seam"
            },
            new MenuItem
            {

              Action = "workersettings",
              Title = "Настройки",
              CSS = "btn btn-outline-light",
              Icon = "bi bi-gear-fill"
            }
          };
        }
      }
      else
      {
        mainMenu.GreetingMessage = "Вы не зарегистрированы в системе. Пожалуйста, свяжитесь с администратором.";
        mainMenu.Menu = new List<MenuItem>();
      }
      return View(mainMenu);
    }

    [HttpGet("botmenu/{id?}/cabinetslist")]
    public IActionResult BotMenuCabinetsList([FromRoute] long id)
    {
      var user = _db.Workers
        .Include(w => w.AssignedCabinets)
        .FirstOrDefault(w => w.Id == id);

      if (user.Role == RoleType.Admin)
      {
        user.AssignedCabinets = _db.Cabinets.ToList();
      }

      MainMenuViewModel mainMenu = new MainMenuViewModel()
      {
        WorkerId = user.Id
      };


      foreach (var cabinet in user?.AssignedCabinets!)
      {
        mainMenu.Menu.Add(new MenuItem
        {
          Action = "cabinet",
          EntityId = $"{cabinet.Id}",
          Title = $"{cabinet.Marketplace} / {cabinet.Name}",
          Icon = "bi bi-building me-2",
          CSS = "list-group-item bg-transparent text-primary border-primary"
        });
      }
      return View(mainMenu);
    }

    [HttpGet("botmenu/{id?}/cabinet/{cabinetId?}")]
    public IActionResult BotMenuCabinet([FromRoute] long id, [FromRoute] int? cabinetId)
    {
      var user = _db.Workers
        .Include(w => w.AssignedCabinets)
        .First(w => w.Id == id);
      if (user == null)
      {
        return Redirect("https://t.me/MbtdistrBot");
      }
      MainMenuViewModel mainMenu = new MainMenuViewModel() { WorkerId = user.Id };

      var cabinet = _db?.Cabinets?.FirstOrDefault(c => c.Id == cabinetId);

      if (cabinet != null)
      {
        mainMenu.GreetingMessage = $"Вы находитесь в кабинете {cabinet.Name} ({cabinet.Marketplace})";
        //mainMenu.Menu.Add(new MenuItem
        //{
        //  Worker = user,
        //  Icon = "📦",
        //  Action = "orderslist",
        //  EntityId = $"{cabinet.Id}",
        //  Title = "Заказы"
        //});
        mainMenu.Menu.Add(new MenuItem
        {
          Action = "returnslist",
          EntityId = $"{cabinet.Id}",
          Title = "Возвраты",
          CSS = "btn btn-outline-danger",
          Icon = "bi bi-box-seam"
        });
      }

      return View(mainMenu);
    }


    [HttpGet("botmenu/{id?}/cabinet/{cabinetId?}/orderslist")]
    public IActionResult OrdersList([FromRoute] long id, [FromRoute] int? cabinetId)
    {
      return NoContent();
    }

    [HttpGet("botmenu/{id?}/returnslist")]
    public async Task<IActionResult> ReturnsList([FromRoute] long id)
    {
      var worker = await _db.Workers.Include(w => w.AssignedCabinets).FirstOrDefaultAsync(w => w.Id == id);
      if (worker == null)
      {
        return Redirect("https://t.me/MbtdistrBot");
      }
      MainMenuViewModel mainMenu = new MainMenuViewModel
      {
        WorkerId = worker.Id,

      };
      string greetingMessage = $"Вы находитесь в разделе Возвраты. Здесь отображаются возвраты по всем закрепленным кабинетам ({worker.AssignedCabinets.Count})";

      if (worker.AssignedCabinets?.Count == 0)
      {
        greetingMessage = "У вас нет закрепленных кабинетов. Пожалуйста, свяжитесь с администратором.";
        mainMenu.GreetingMessage = greetingMessage;
        return View(mainMenu);
      }
      List<MenuItem> menuItems = new List<MenuItem>();
      foreach (var cab in worker?.AssignedCabinets!)
      {
        var _returns = _db.Returns.Include(r => r.Info).Where(r => r.CabinetId == cab.Id).ToList();
        if (_returns.Count > 0)
        {
          foreach (var r in _returns)
          {
            menuItems.Add(new MenuItem
            {
              Action = "returninfo",
              EntityId = $"{r.Id}",
              Title = $"{cab.Marketplace} / {cab.Name} от {r.CreatedAt}",
              Icon = "bi bi-box-seam me-2",
              CSS = "list-group-item bg-transparent text-primary"
            });
          }
        }
      }
      if (menuItems.Count == 0)
      {
        greetingMessage = "У закрепленных за Вами кабинетов нет активных возвратов.";
        mainMenu.GreetingMessage = greetingMessage;
        return View(mainMenu);
      }
      mainMenu.Menu = menuItems;
      return View(mainMenu);
    }

    [HttpGet("botmenu/{id?}/cabinet/{cabinetId?}/returnslist")]
    public IActionResult ReturnsList([FromRoute] int id, [FromRoute] int? cabinetId)
    {
      var returns = _db.Returns.Include(r => r.Info).Include(r => r.Cabinet).Where(r => r.CabinetId == cabinetId).ToList();

      MainMenuViewModel mainMenu = new MainMenuViewModel
      {
        WorkerId = id,
        GreetingMessage = $"Возвраты кабинета {cabinetId}"
      };

      if (returns?.Count > 0)
      {
        foreach (var r in returns)
        {
          mainMenu.Menu.Add(new MenuItem
          {
            Action = "returninfo",
            EntityId = $"{r.Id}",
            Icon = "bi bi-box-seam me-2",
            CSS = "list-group-item bg-transparent text-primary"
          });
        }
      }
      else
      {
        mainMenu.GreetingMessage = "У кабинета нет активных возвратов.";
      }

      return View(mainMenu);
    }

    [HttpGet("botmenu/{workerId?}/returninfo/{returnId?}")]
    public IActionResult ReturnInfo([FromRoute] int workerId, [FromRoute] long returnId)
    {
      var _return = _db.Returns.Include(r => r.Info).Include(r => r.Cabinet).FirstOrDefault(r => r.Id == returnId);
      if (_return == null)
      {
        return Redirect("https://t.me/MbtdistrBot");
      }
      MainMenuViewModel mainMenu = new MainMenuViewModel
      {
        WorkerId = workerId,
        GreetingMessage = $"Возврат {_return.Info.ReturnInfoId}\n{_return.CreatedAt}"
      };

      mainMenu.HtmlContent = MarketSyncService.FormatReturnHtmlContent(_return, _return.Cabinet, null);

      return View(mainMenu);
    }

    [HttpPost("botmenu/{id?}/cabinet/{cabinetId?}/workersettings")]
    public IActionResult WorkerSettings([FromRoute] long id, [FromBody] NotificationOptions notificationOptions)
    {
      return Ok();
    }

    //YMSupplyRequests
    [HttpGet("botmenu/{id?}/cabinet/{cabinetId?}/ymsupplyrequests")]
    public IActionResult YMSupplyRequests([FromRoute] int workerId, [FromRoute] int? cabinetId)
    {
      var worker = _db.Workers.Include(w => w.AssignedCabinets).FirstOrDefault(w => w.Id == workerId);
      if (worker == null)
      {
        return Redirect("https://t.me/MbtdistrBot");
      }

      ListViewModel<YMSupplyRequest> listViewModel = new ListViewModel<YMSupplyRequest>
      {
        //TODO: доделать отображение вьюмодели
      };
      listViewModel.Items = _db.YMSupplyRequests
        .Include(r => r.Cabinet)
        .Where(r => r.CabinetId == cabinetId)
        .ToList();

      listViewModel.FilterButtonModels = new Dictionary<string, FilterButtonsViewModel>();

      KeyValuePair<string, FilterButtonsViewModel> filterButton = new KeyValuePair<string, FilterButtonsViewModel>(
        "status",
        new FilterButtonsViewModel
        {
          FilterName = "Статус",
          Options = new List<FilterButtonOption> {
            new FilterButtonOption
            {
              DisplayName = "Создана",
              Value = "PUBLISHED",
              Selected = true
            },
            new FilterButtonOption
            {
              DisplayName = "Готов к выдаче",
              Value = "READY_TO_WITHDRAW",
              Selected = false
            },
            new FilterButtonOption
            {
              DisplayName = "Готовы к утилизации",
              Value = "READY_FOR_UTILIZATION ",
              Selected = false
            },
            new FilterButtonOption
            {
              DisplayName = "Завершен",
              Value = "FINISHED",
              Selected = false
            },
            new FilterButtonOption
            {
              DisplayName = "Отменен",
              Value = "CANCELLED",
              Selected = false
            },
          }
        });

      KeyValuePair<string, FilterButtonsViewModel> filterButton2 = new KeyValuePair<string, FilterButtonsViewModel>(
        "type",
        new FilterButtonsViewModel
        {
          FilterName = "Тип",
          Options = new List<FilterButtonOption>
          {
            new FilterButtonOption
            {
              DisplayName = "Поставка",
              Value = "SUPPLY",
              Selected = false
            },
            new FilterButtonOption
            {
              DisplayName = "Вывоз",
              Value = "WITHDRAW",
              Selected = false
            },
            new FilterButtonOption
            {
              DisplayName = "Утилизация",
              Value = "UTILIZATION",
              Selected = false
            },
          }
        });

      KeyValuePair<string, FilterButtonsViewModel> filterButton3 = new KeyValuePair<string, FilterButtonsViewModel>(
        "subtype",
       new FilterButtonsViewModel
       {
         FilterName = "Подтип",
         Options = new List<FilterButtonOption>()
       {
         new FilterButtonOption
         {
          DisplayName = "Утилизация по запросу склада",
          Value = "FORCE_PLAN",
          Selected = false
         },
          new FilterButtonOption
          {
            DisplayName = "Утилизация непринятых товаров",
            Value = "FORCE_PLAN_ANOMALY_PER_SUPPLY",
            Selected = false
          },
          new FilterButtonOption
          {
            DisplayName = "Утилизация по запросу магазина",
            Value = "PLAN_BY_SUPPLIER",
            Selected = false
          },
          new FilterButtonOption
          {
            DisplayName = "Вывоз непринятых товаров",
            Value = "ANOMALY_WITHDRAW",
            Selected = false
          },
          new FilterButtonOption
          {
            DisplayName = "Ручная утилизация по запросу склада",
            Value = "MAN_UTIL",
            Selected = false
          } }
       });

      listViewModel.FilterButtonModels.Add(filterButton.Key, filterButton.Value);
      listViewModel.FilterButtonModels.Add(filterButton2.Key, filterButton2.Value);
      listViewModel.FilterButtonModels.Add(filterButton3.Key, filterButton3.Value);

      listViewModel.Title = "Поставки, вывоз и утилизация";

      return View(listViewModel);
    }
  }
}
