using System;

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

    public ICollection<Cabinet> AssignedCabinets { get; set; }
           = new List<Cabinet>();
    public int CurrentCabinetId { get; internal set; }
  }
}