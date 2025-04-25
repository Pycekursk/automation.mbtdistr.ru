using System.Diagnostics;

using automation.mbtdistr.ru.Data;
using automation.mbtdistr.ru.Models;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
      //if (Debugger.IsAttached)
      //{
      //  var db = HttpContext.RequestServices.GetService<ApplicationDbContext>();
      //  //получаем работника с айди телеграмм 1406950293
      //  var user = db.Workers
      //    .Where(u => u.TelegramId == "1406950293")
      //    .Include(w => w.NotificationOptions)
      //    .FirstOrDefault();

      //  if (user != null)
      //  {
      //    user.NotificationOptions = new NotificationOptions
      //    {
      //      WorkerId = user.Id,
      //      NotificationLevels = new List<NotificationLevel>
      //      {
      //        NotificationLevel.DeepDegugNotification,
      //        NotificationLevel.LogNotification
      //      }
      //    };
      //    user.NotificationOptions.IsReceiveNotification = true;
      //    //db.Entry(user.NotificationOptions).State = EntityState.Modified;

      //    //db.Entry(user).State = EntityState.Modified;

      //    var changes = db.SaveChanges();




      //    ////получаем настройки уведомлений
      //    //var notificationOptions = user.NotificationOptions;
      //    ////получаем уровень уведомлений
      //    //if (!notificationOptions.NotificationLevels.Contains(NotificationLevel.DeepDegugNotification))
      //    //{
      //    //  //если уровень уведомлений не содержит DeepDegugNotification, то добавляем его
      //    //  notificationOptions.NotificationLevels.Add(NotificationLevel.DeepDegugNotification);
      //    //  //сохраняем изменения в БД
      //    //}
      //    //if (!notificationOptions.NotificationLevels.Contains(NotificationLevel.LogNotification))
      //    //{
      //    //  //если уровень уведомлений не содержит LogNotification, то добавляем его
      //    //  notificationOptions.NotificationLevels.Add(NotificationLevel.LogNotification);
      //    //}

      //    //notificationOptions.WorkerId = user.Id;

      //    //db.Entry(notificationOptions).State = EntityState.Modified;
      //    ////db.Entry(notificationOptions).State = EntityState.Modified;


      //    //user.NotificationOptions = notificationOptions;
      //    //var changes = db.SaveChanges();
      //  }
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
