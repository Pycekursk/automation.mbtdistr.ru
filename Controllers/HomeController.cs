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
      //  //�������� ��������� � ���� ��������� 1406950293
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




      //    ////�������� ��������� �����������
      //    //var notificationOptions = user.NotificationOptions;
      //    ////�������� ������� �����������
      //    //if (!notificationOptions.NotificationLevels.Contains(NotificationLevel.DeepDegugNotification))
      //    //{
      //    //  //���� ������� ����������� �� �������� DeepDegugNotification, �� ��������� ���
      //    //  notificationOptions.NotificationLevels.Add(NotificationLevel.DeepDegugNotification);
      //    //  //��������� ��������� � ��
      //    //}
      //    //if (!notificationOptions.NotificationLevels.Contains(NotificationLevel.LogNotification))
      //    //{
      //    //  //���� ������� ����������� �� �������� LogNotification, �� ��������� ���
      //    //  notificationOptions.NotificationLevels.Add(NotificationLevel.LogNotification);
      //    //}

      //    //notificationOptions.WorkerId = user.Id;

      //    //db.Entry(notificationOptions).State = EntityState.Modified;
      //    ////db.Entry(notificationOptions).State = EntityState.Modified;


      //    //user.NotificationOptions = notificationOptions;
      //    //var changes = db.SaveChanges();
      //  }
      //}
      //����� ��������� �� ����� Index



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
