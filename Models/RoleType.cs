using System.ComponentModel.DataAnnotations;

namespace automation.mbtdistr.ru.Models
{
  public enum RoleType
  {
    [Display(Name = "Гость")]
    Guest = 0,

    [Display(Name = "Администратор")]
    Admin = 1,

    [Display(Name = "Менеджер кабинета")]
    CabinetManager = 2,

    [Display(Name = "Менеджер претензий")]
    ClaimsManager = 3,

    [Display(Name = "Складской персонал")]
    WarehouseStaff = 4,

    [Display(Name = "Курьер")]
    Courier = 5,

    [Display(Name = "Директор")]
    Director = 6
  }

}
