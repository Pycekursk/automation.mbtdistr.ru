namespace automation.mbtdistr.ru.Models
{
  public class MainMenuModel
  {
    public int WorkerId { get; set; } = 0; // Идентификатор работника, для которого создается меню. По умолчанию 0 (гость).

    public List<MainMenuButton> Buttons { get; set; } = new List<MainMenuButton>();

    public MainMenuButton? Selected { get; set; }

    public MainMenuModel(IEnumerable<MainMenuButton> menuButtons, MainMenuButton? selected = null)
    {
      Buttons = menuButtons.ToList();
      Selected = selected;
    }

    public static MainMenuModel Create(Worker worker)
    {
      List<MainMenuButton> buttons = new List<MainMenuButton>();
      switch (worker.Role)
      {
        case RoleType.Guest:
          break;
        case RoleType.Admin:
          buttons.Add(new MainMenuButton(1, "Кабинеты", ButtonIcon.Orderedlist, "cabinetslist"));
          buttons.Add(new MainMenuButton(2, "Заказы", ButtonIcon.Ordersbox, "orderslist", false));
          buttons.Add(new MainMenuButton(3, "Возвраты", ButtonIcon.Undo, "returnslist"));
          buttons.Add(new MainMenuButton(4, "Заявки", ButtonIcon.Car, "supplieslist"));
          buttons.Add(new MainMenuButton(5, "Склады", ButtonIcon.Home, "warehouseslist"));
          break;
        case RoleType.CabinetManager:
          buttons.Add(new MainMenuButton(1, "Кабинеты", ButtonIcon.Orderedlist, "cabinetslist"));
          buttons.Add(new MainMenuButton(2, "Заказы", ButtonIcon.Ordersbox, "orderslist", false));
          buttons.Add(new MainMenuButton(3, "Возвраты", ButtonIcon.Undo, "returnslist"));
          buttons.Add(new MainMenuButton(4, "Заявки", ButtonIcon.Car, "supplieslist"));
          buttons.Add(new MainMenuButton(5, "Склады", ButtonIcon.Home, "warehouseslist"));
          break;
        case RoleType.ClaimsManager:
          buttons.Add(new MainMenuButton(3, "Возвраты", ButtonIcon.Undo, "returnslist"));
          buttons.Add(new MainMenuButton(4, "Заявки", ButtonIcon.Car, "supplieslist"));
          break;
        case RoleType.WarehouseStaff:
          break;
        case RoleType.Courier:
          buttons.Add(new MainMenuButton(4, "Заявки", ButtonIcon.Car, "supplieslist"));
          buttons.Add(new MainMenuButton(5, "Склады", ButtonIcon.Home, "warehouseslist"));

          break;
        case RoleType.Director:
          buttons.Add(new MainMenuButton(2, "Заказы", ButtonIcon.Ordersbox, "orderslist", false));
          buttons.Add(new MainMenuButton(3, "Возвраты", ButtonIcon.Undo, "returnslist"));
          buttons.Add(new MainMenuButton(4, "Заявки", ButtonIcon.Car, "supplieslist"));
          buttons.Add(new MainMenuButton(5, "Склады", ButtonIcon.Home, "warehouseslist"));

          break;
        default:
          break;
      }
      MainMenuModel? model = new MainMenuModel(buttons);
      if (worker.Id > 0)
      {
        model.WorkerId = worker.Id;
      }
      else
      {
        model = CreateDefault(worker);
      }
      return model;
    }

    private static MainMenuModel CreateDefault(Worker worker)
    {
      //TODO: Логика создания модели главного меню по умолчанию (гостевого или неизвестного работника).
      var buttons = new List<MainMenuButton>
      {

      };
      return new MainMenuModel(buttons);
    }
  }

  public class MainMenuButton
  {
    [System.Text.Json.Serialization.JsonPropertyName("id")]
    [Newtonsoft.Json.JsonProperty("id")]
    public int Id { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("text")]
    [Newtonsoft.Json.JsonProperty("text")]
    public string Text { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("icon")]
    [Newtonsoft.Json.JsonProperty("icon")]
    public string Icon { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("action")]
    [Newtonsoft.Json.JsonProperty("action")]
    public string? Action { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("entityId")]
    [Newtonsoft.Json.JsonProperty("entityId")]
    public string? EntityId { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("isActive")]
    [Newtonsoft.Json.JsonProperty("isActive")]
    public bool IsActive { get; set; } = true;

    public MainMenuButton(int id, string text, string icon, string? action = null, bool isActive = true, string? entityId = null)
    {
      Id = id;
      Text = text;
      Icon = icon;
      Action = action;
      EntityId = entityId;
      IsActive = isActive;
    }
  }
}
