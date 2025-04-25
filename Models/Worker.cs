using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace automation.mbtdistr.ru.Models
{
  public class Worker
  {
    public int Id { get; set; }
    public string TelegramId { get; set; }
    public string Name { get; set; }
    public RoleType Role { get; set; } = RoleType.Guest;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Username { get; set; }
    public DateTime CreatedAt { get; set; }

    public NotificationOptions NotificationOptions { get; set; } = new NotificationOptions();

    public ICollection<Cabinet> AssignedCabinets { get; set; }
           = new List<Cabinet>();

    public int CurrentCabinetId { get; internal set; }
  }

  public class NotificationOptions
  {
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public bool IsReceiveNotification { get; set; } = true;
    public List<NotificationLevel> NotificationLevels { get; set; } = new List<NotificationLevel>();

    public int WorkerId { get; set; }
    public Worker Worker { get; set; }
  }

  public enum NotificationLevel
  {
    [Display(Name = "Уведомления о возвратах")]
    ReturnNotification,
    [Display(Name = "Уведомления о новых заказах")]
    OrderNotification,
    [Display(Name = "Уведомления о новых сообщениях")]
    MessageNotification,
    [Display(Name = "Уровень отладки")]
    DebugNotification,
    [Display(Name = "Уровень администратора")]
    AdminNotification,
  }
}