using System.Text;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using automation.mbtdistr.ru.Data;
using automation.mbtdistr.ru.Models;
using automation.mbtdistr.ru.Services.Ozon;
using automation.mbtdistr.ru.Services.Wildberries;

namespace automation.mbtdistr.ru.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class TelegramBotController : ControllerBase
  {
    private readonly ITelegramBotClient _botClient;
    private readonly ApplicationDbContext _db;
    private readonly WildberriesApiService _wb;
    private readonly OzonApiService _oz;
    private readonly UserInputWaitingService _waitingService;
    private readonly ILogger<TelegramBotController> _logger;

    //public TelegramBotController(
    //    UserInputWaitingService waitingService,
    //    ITelegramBotClient botClient,
    //    ApplicationDbContext db,
    //    WildberriesApiService wb,
    //    OzonApiService oz,
    //    ILogger<TelegramBotController> logger)
    //{
    //  _waitingService = waitingService;
    //  _botClient = botClient;
    //  _db = db;
    //  _wb = wb;
    //  _oz = oz;
    //  _logger = logger;
    //}

    //[HttpPost]
    //public async Task<IActionResult> Post([FromBody] Update update)
    //{
    //  LogUpdate(update);
    //  var worker = await GetOrCreateWorkerAsync(update);

    //  switch (update.Type)
    //  {
    //    case UpdateType.Message:
    //      var msg = update.Message!;
    //      if (await TryHandleForceReplyAsync(msg))
    //        return Ok();
    //      await HandleTextMessageAsync(msg, worker);
    //      break;

    //    case UpdateType.CallbackQuery:
    //      await HandleCallbackQueryAsync(update.CallbackQuery!);
    //      break;


    //    default:
    //      _logger.LogWarning("Unsupported update type: {UpdateType}", update.Type);
    //      break;
    //  }

    //  return Ok();
    //}

    //private async Task<Worker> GetOrCreateWorkerAsync(Update update)
    //{
    //  var userId = update.Message?.From?.Id ?? update.CallbackQuery?.From.Id;
    //  var tgId = userId.ToString();
    //  var worker = await _db.Workers
    //      .Include(w => w.AssignedCabinets)
    //      .FirstOrDefaultAsync(w => w.TelegramId == tgId);

    //  if (worker == null && update.Message != null)
    //  {
    //    worker = new Worker
    //    {
    //      TelegramId = tgId,
    //      Name = $"{update.Message.From.FirstName} {update.Message.From.LastName}".Trim(),
    //      Role = RoleType.Guest,
    //      CreatedAt = DateTime.UtcNow
    //    };
    //    _db.Workers.Add(worker);
    //    await _db.SaveChangesAsync();
    //    await _botClient.SendMessage(
    //        update.Message.Chat.Id,
    //        "Добро пожаловать! Вы зарегистрированы как Гость. Ожидайте назначения роли администратором.");

    //    // Отправляем сообщение администраторам о новом пользователе
    //    var admins = await _db.Workers
    //        .Where(w => w.Role == RoleType.Admin)
    //        .ToListAsync();

    //    foreach (var admin in admins)
    //    {
    //      await _botClient.SendMessage(
    //          admin.TelegramId,
    //          $"Новый пользователь зарегистрирован: {worker.Name} ({worker.TelegramId}).");
    //    }
    //  }

    //  return worker!;
    //}

    //private async Task<bool> TryHandleForceReplyAsync(Message msg)
    //{
    //  if (msg.ReplyToMessage == null)
    //    return false;

    //  var waiting = _waitingService.Get(msg.From.Id);

    //  if (waiting == null)
    //    return false;

    //  var (action, entityId) = waiting.Value;
    //  switch (action)
    //  {
    //    case "edit_user_name":
    //      await EditUserNameAsync(msg.Chat.Id, msg.Text!, entityId);
    //      break;

    //    case "edit_user_role":
    //      await PromptUserRoleSelectionAsync(msg.Chat.Id, entityId);
    //      break;

    //    case "edit_cab_name":
    //      await EditCabinetNameAsync(msg.Chat.Id, msg.Text!, entityId);
    //      break;

    //    case "edit_cab_settings_key":
    //      await UpdateCabinetParameterKeyAsync(msg.Chat.Id, msg.Text!, entityId);
    //      break;
    //    case "edit_cab_settings_value":
    //      await UpdateCabinetParameterValueAsync(msg.Chat.Id, msg.Text!, entityId);
    //      break;

    //    case "create_cab_marketplace":
    //      await HandleCreateCabinetMarketplaceAsync(msg.Chat.Id, msg.Text!);
    //      break;

    //    case "create_cab_name":
    //      await HandleCreateCabinetNameAsync(msg.Chat.Id, msg.Text!, entityId);
    //      break;
    //  }

    //  _waitingService.Remove(msg.From.Id);
    //  return true;
    //}

    //// 3.1) Шаг 2: сохраняем marketplace и просим имя
    //private async Task HandleCreateCabinetMarketplaceAsync(long chatId, int userId, string marketplace)
    //{
    //  var cabinet = new Cabinet
    //  {
    //    Marketplace = marketplace.Trim(),
    //    Name = string.Empty
    //  };
    //  _db.Cabinets.Add(cabinet);
    //  await _db.SaveChangesAsync();

    //  await _botClient.SendMessage(
    //     chatId,
    //     $"Введите название для кабинета (Marketplace: {cabinet.Marketplace}):",
    //     replyMarkup: new ForceReplyMarkup { Selective = true });
    //  _waitingService.Register(userId, "create_cab_name", cabinet.Id);
    //}

    //// 3.2) Шаг 3: сохраняем имя и показываем детали
    //private async Task HandleCreateCabinetNameAsync(long chatId, string name, int cabinetId)
    //{
    //  var cabinet = await _db.Cabinets
    //      .Include(c => c.Settings)
    //      .ThenInclude(s => s.ConnectionParameters)
    //      .FirstOrDefaultAsync(c => c.Id == cabinetId);

    //  if (cabinet == null)
    //  {
    //    await _botClient.SendMessage(chatId, "Ошибка: кабинет не найден.");
    //    return;
    //  }

    //  cabinet.Name = name.Trim();
    //  await _db.SaveChangesAsync();

    //  var sb = new StringBuilder();
    //  sb.AppendLine($"✅ Кабинет:\n#{cabinet.Id} {cabinet.Marketplace}/{cabinet.Name}");
    //  sb.AppendLine("Настройки подключения:");
    //  foreach (var p in cabinet.Settings.ConnectionParameters)
    //    sb.AppendLine($"- {p.Key}: {p.Value}");

    //  var buttons = new[]
    //  {
    //    new[] {
    //        InlineKeyboardButton.WithCallbackData("✏️ Название", $"edit_cab_name_{cabinet.Id}"),
    //        InlineKeyboardButton.WithCallbackData(" Настройки", $"edit_cab_settings_{cabinet.Id}")
    //    },
    //    new[] {
    //        InlineKeyboardButton.WithCallbackData("❌ Удалить", $"delete_cab_{cabinet.Id}"),
    //        InlineKeyboardButton.WithCallbackData(" Пользователи", $"get_cab_users_{cabinet.Id}")
    //    }
    //};

    //  await _botClient.SendMessage(
    //      chatId,
    //      sb.ToString().TrimEnd(),
    //      replyMarkup: new InlineKeyboardMarkup(buttons));
    //}

    //private async Task UpdateCabinetParameterValueAsync(long id, string v, int entityId)
    //{
    //  var parameter = await _db.ConnectionParameters.FindAsync(entityId);
    //  if (parameter == null) return;
    //  parameter.Value = v;
    //  await _db.SaveChangesAsync();
    //  await _botClient.SendMessage(id, $"Параметр {parameter.Key} обновлён на: {parameter.Value}");
    //}

    //private async Task UpdateCabinetParameterKeyAsync(long id, string v, int entityId)
    //{
    //  var parameter = await _db.ConnectionParameters.FindAsync(entityId);
    //  if (parameter == null) return;
    //  parameter.Key = v;
    //  await _db.SaveChangesAsync();
    //  await _botClient.SendMessage(id, $"Параметр {parameter.Key} обновлён на: {parameter.Value}");
    //}

    //private async Task EditCabinetNameAsync(long id, string v, int entityId)
    //{
    //  var cabinet = await _db.Cabinets.FindAsync(entityId);
    //  if (cabinet != null)
    //  {
    //    cabinet.Name = v;
    //    await _db.SaveChangesAsync();
    //    await _botClient.SendMessage(id, $"Имя кабинета обновлено на: {cabinet.Name}");
    //  }
    //}

    //private async Task HandleTextMessageAsync(Message msg, Worker worker)
    //{
    //  var text = msg.Text?.Trim().ToLower();
    //  switch (text)
    //  {
    //    case "/start":
    //      await _botClient.SendMessage(msg.Chat.Id, $"Привет, {worker.Name}! Вы {GetEnumDisplayName(worker.Role)}");
    //      break;

    //    case "/help":
    //      await HandleGetHelpAsync(msg, worker);
    //      break;
    //    case "/myrole":
    //      await _botClient.SendMessage(msg.Chat.Id, $"Вы {GetEnumDisplayName(worker.Role)}");
    //      break;
    //    case "/cabinets":
    //      await HandleGetCabinetsAsync(msg, worker);
    //      break;
    //    case "/workers":
    //      await HandleGetWorkersAsync(msg);
    //      break;
    //    default:
    //      await _botClient.SendMessage(msg.Chat.Id, "Команда не распознана или у вас нет прав. Напишите /help.");
    //      break;
    //  }
    //}

    //private async Task HandleGetWorkersAsync(Message msg)
    //{
    //  try
    //  {
    //    var workers = await _db.Workers.ToListAsync();
    //    if (workers.Count == 0)
    //    {
    //      await _botClient.SendMessage(msg.Chat.Id, "Нет зарегистрированных пользователей.");
    //      return;
    //    }

    //    var buttons = workers
    //        .Select(w => InlineKeyboardButton.WithCallbackData(
    //            text: $"{w.Name} ({GetEnumDisplayName(w.Role)})",
    //            callbackData: $"select_user_{w.Id}"))
    //        .Chunk(1)
    //        .Select(chunk => chunk.ToArray())
    //        .ToArray();

    //    await _botClient.SendMessage(
    //        msg.Chat.Id,
    //        "Выберите пользователя:",
    //        replyMarkup: new InlineKeyboardMarkup(buttons));
    //  }
    //  catch (Exception ex)
    //  {
    //    await _botClient.SendMessage(msg.Chat.Id, $"Ошибка: {ex.Message}");
    //  }
    //}

    //private async Task HandleGetHelpAsync(Message msg, Worker worker)
    //{
    //  switch (worker.Role)
    //  {
    //    case RoleType.Admin:
    //      await _botClient.SendMessage(msg.Chat.Id, "Доступные команды для администратора:\n" +
    //          "/start - Начать взаимодействие\n" +
    //          "/help - Получить помощь\n" +
    //          "/myrole - Узнать свою роль\n" +
    //          "/cabinets - Получить список кабинетов\n" +
    //          "/workers - Получить список пользователей\n");
    //      break;
    //    case RoleType.CabinetManager:
    //      await _botClient.SendMessage(msg.Chat.Id, "Доступные команды для менеджера кабинета:\n" +
    //          "/start - Начать взаимодействие\n" +
    //          "/help - Список команд\n" +
    //          "/myrole - Узнать свою роль\n" +
    //          "/cabinets - Список кабинетов");
    //      break;
    //    case RoleType.ClaimsManager:
    //      await _botClient.SendMessage(msg.Chat.Id, "Доступные команды для менеджера по возвратам:\n" +
    //          "/start - Начать взаимодействие\n" +
    //          "/help - Список команд\n" +
    //          "/myrole - Узнать свою роль");
    //      break;
    //    case RoleType.WarehouseStaff:
    //      await _botClient.SendMessage(msg.Chat.Id, "Доступные команды для склада:\n" +
    //          "/start - Начать взаимодействие\n" +
    //          "/help - Список команд\n" +
    //          "/myrole - Узнать свою роль");
    //      break;
    //    case RoleType.Courier:
    //      await _botClient.SendMessage(msg.Chat.Id, "Доступные команды для курьера:\n" +
    //          "/start - Начать взаимодействие\n" +
    //          "/help - Список команд\n" +
    //          "/myrole - Узнать свою роль");
    //      break;
    //    case 0:
    //      break;
    //  }


    //  //var helpText = "Доступные команды:\n" +
    //  //               "/start - Начать взаимодействие\n" +
    //  //               "/help - Получить помощь\n" +
    //  //               "/myrole - Узнать свою роль\n" +
    //  //               "/getcabinets - Получить список кабинетов";
    //  //await _botClient.SendMessage(msg.Chat.Id, helpText);
    //}

    //private async Task HandleGetCabinetsAsync(Message msg, Worker worker)
    //{
    //  if (worker.Role != RoleType.Admin && worker.Role != RoleType.CabinetManager)
    //  {
    //    await _botClient.SendMessage(msg.Chat.Id, "У вас нет прав для просмотра кабинетов.");
    //    return;
    //  }

    //  try
    //  {
    //    List<Cabinet> cabinets;
    //    if (worker.Role == RoleType.CabinetManager)
    //    {
    //      cabinets = await _db.Cabinets
    //          .Where(c => c.AssignedWorkers.Any(w => w.Id == worker.Id))
    //          .ToListAsync();
    //      if (!cabinets.Any())
    //      {
    //        await _botClient.SendMessage(msg.Chat.Id, "Нет доступных кабинетов.");
    //        return;
    //      }
    //    }
    //    else
    //    {
    //      cabinets = await _db.Cabinets.ToListAsync();
    //      if (!cabinets.Any())
    //      {
    //        await _botClient.SendMessage(msg.Chat.Id, "Нет доступных кабинетов.");
    //        return;
    //      }
    //    }

    //    // Список кнопок для существующих кабинетов
    //    var buttons = cabinets
    //        .Select(c => InlineKeyboardButton.WithCallbackData(
    //            text: $"{c.Marketplace} / {c.Name}",
    //            callbackData: $"select_cab_{c.Id}"))
    //        .Chunk(1)
    //        .Select(chunk => chunk.ToArray())
    //        .ToList();

    //    // Добавляем кнопку создания нового кабинета
    //    buttons.Add(new[]
    //    {
    //        InlineKeyboardButton.WithCallbackData("➕ Создать кабинет", "create_cab")
    //    });

    //    await _botClient.SendMessage(
    //        msg.Chat.Id,
    //        "Выберите кабинет:",
    //        replyMarkup: new InlineKeyboardMarkup(buttons.ToArray()));
    //  }
    //  catch (Exception ex)
    //  {
    //    await _botClient.SendMessage(msg.Chat.Id, $"Ошибка: {ex.Message}");
    //  }
    //}





    public TelegramBotController(
    UserInputWaitingService waitingService,
    ITelegramBotClient botClient,
    ApplicationDbContext db,
    WildberriesApiService wb,
    OzonApiService oz,
    ILogger<TelegramBotController> logger)
    {
      _waitingService = waitingService;
      _botClient = botClient;
      _db = db;
      _wb = wb;
      _oz = oz;
      _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Update update)
    {
      LogUpdate(update);
      var worker = await GetOrCreateWorkerAsync(update);

      switch (update.Type)
      {
        case UpdateType.Message:
          var msg = update.Message!;
          if (await TryHandleForceReplyAsync(msg))
            return Ok();
          await HandleTextMessageAsync(msg, worker);
          break;

        case UpdateType.CallbackQuery:
          await HandleCallbackQueryAsync(update.CallbackQuery!);
          break;

        default:
          _logger.LogWarning("Unsupported update type: {UpdateType}", update.Type);
          break;
      }

      return Ok();
    }

    private async Task<Worker> GetOrCreateWorkerAsync(Update update)
    {
      var userId = update.Message?.From?.Id ?? update.CallbackQuery?.From.Id;
      var tgId = userId.ToString();
      var worker = await _db.Workers
          .Include(w => w.AssignedCabinets)
          .FirstOrDefaultAsync(w => w.TelegramId == tgId);

      if (worker == null && update.Message != null)
      {
        worker = new Worker
        {
          TelegramId = tgId,
          Name = $"{update.Message.From.FirstName} {update.Message.From.LastName}".Trim(),
          Role = RoleType.Guest,
          CreatedAt = DateTime.UtcNow
        };
        _db.Workers.Add(worker);
        await _db.SaveChangesAsync();
        await _botClient.SendMessage(
            update.Message.Chat.Id,
            "Добро пожаловать! Вы зарегистрированы как Гость. Ожидайте назначения роли администратором.");

        var admins = await _db.Workers
            .Where(w => w.Role == RoleType.Admin)
            .ToListAsync();

        foreach (var admin in admins)
        {
          await _botClient.SendMessage(
              admin.TelegramId,
              $"Новый пользователь зарегистрирован: {worker.Name} ({worker.TelegramId}).");
        }
      }

      return worker!;
    }

    private async Task<bool> TryHandleForceReplyAsync(Message msg)
    {
      if (msg.ReplyToMessage == null)
        return false;

      var waiting = _waitingService.Get(msg.From.Id);
      if (waiting == null)
        return false;

      var (action, entityId) = waiting.Value;
      switch (action)
      {
        case "edit_user_name":
          await EditUserNameAsync(msg.Chat.Id, msg.Text!, entityId);
          break;

        case "edit_user_role":
          await PromptUserRoleSelectionAsync(msg.Chat.Id, entityId);
          break;

        case "edit_cab_name":
          await EditCabinetNameAsync(msg.Chat.Id, msg.Text!, entityId);
          break;

        case "edit_cab_settings_key":
          await UpdateCabinetParameterKeyAsync(msg.Chat.Id, msg.Text!, entityId);
          break;
        case "edit_cab_settings_value":
          await UpdateCabinetParameterValueAsync(msg.Chat.Id, msg.Text!, entityId);
          break;

        case "create_cab_marketplace":
          // Передаём userId для корректной регистрации следующего шага
          await HandleCreateCabinetMarketplaceAsync(msg.Chat.Id, msg.From.Id, msg.Text!);
          break;

        case "create_cab_name":
          await HandleCreateCabinetNameAsync(msg.Chat.Id, msg.Text!, entityId);
          break;

        default:
          return false;
      }

      _waitingService.Remove(msg.From.Id);
      return true;
    }

    // Шаг 2: сохраняем marketplace и просим имя
    private async Task HandleCreateCabinetMarketplaceAsync(long chatId, long userId, string marketplace)
    {
      var cabinet = new Cabinet
      {
        Marketplace = marketplace.Trim(),
        Name = string.Empty
      };
      _db.Cabinets.Add(cabinet);
      await _db.SaveChangesAsync();

      await _botClient.SendMessage(
         chatId,
         $"Введите название для кабинета (Marketplace: {cabinet.Marketplace}):",
         replyMarkup: new ForceReplyMarkup { Selective = true });
      _waitingService.Register(userId, "create_cab_name", cabinet.Id);
    }

    // Шаг 3: сохраняем имя и показываем детали
    private async Task HandleCreateCabinetNameAsync(long chatId, string name, int cabinetId)
    {
      var cabinet = await _db.Cabinets
          .Include(c => c.Settings)
          .ThenInclude(s => s.ConnectionParameters)
          .FirstOrDefaultAsync(c => c.Id == cabinetId);

      if (cabinet == null)
      {
        await _botClient.SendMessage(chatId, "Ошибка: кабинет не найден.");
        return;
      }

      cabinet.Name = name.Trim();
      await _db.SaveChangesAsync();

      var sb = new StringBuilder();
      sb.AppendLine($"✅ Кабинет:\n#{cabinet.Id} {cabinet.Marketplace}/{cabinet.Name}");
      sb.AppendLine("Настройки подключения:");
      foreach (var p in cabinet.Settings.ConnectionParameters)
        sb.AppendLine($"- {p.Key}: {p.Value}");

      var buttons = new[]
      {
        new[] {
            InlineKeyboardButton.WithCallbackData("✏️ Название", $"edit_cab_name_{cabinet.Id}"),
            InlineKeyboardButton.WithCallbackData(" Настройки", $"edit_cab_settings_{cabinet.Id}")
        },
        new[] {
            InlineKeyboardButton.WithCallbackData("❌ Удалить", $"delete_cab_{cabinet.Id}"),
            InlineKeyboardButton.WithCallbackData(" Пользователи", $"get_cab_users_{cabinet.Id}")
        }
      };

      await _botClient.SendMessage(
          chatId,
          sb.ToString().TrimEnd(),
          replyMarkup: new InlineKeyboardMarkup(buttons));
    }

    private async Task UpdateCabinetParameterValueAsync(long id, string v, int entityId)
    {
      var parameter = await _db.ConnectionParameters.FindAsync(entityId);
      if (parameter == null) return;
      parameter.Value = v;
      await _db.SaveChangesAsync();
      await _botClient.SendMessage(id, $"Параметр {parameter.Key} обновлён на: {parameter.Value}");
    }

    private async Task UpdateCabinetParameterKeyAsync(long id, string v, int entityId)
    {
      var parameter = await _db.ConnectionParameters.FindAsync(entityId);
      if (parameter == null) return;
      parameter.Key = v;
      await _db.SaveChangesAsync();
      await _botClient.SendMessage(id, $"Параметр {parameter.Key} обновлён на: {parameter.Value}");
    }

    private async Task EditCabinetNameAsync(long id, string v, int entityId)
    {
      var cabinet = await _db.Cabinets.FindAsync(entityId);
      if (cabinet != null)
      {
        cabinet.Name = v;
        await _db.SaveChangesAsync();
        await _botClient.SendMessage(id, $"Имя кабинета обновлено на: {cabinet.Name}");
      }
    }

    private async Task HandleTextMessageAsync(Message msg, Worker worker)
    {
      var text = msg.Text?.Trim().ToLower();
      switch (text)
      {
        case "/start":
          await _botClient.SendMessage(msg.Chat.Id, $"Привет, {worker.Name}! Вы {GetEnumDisplayName(worker.Role)}");
          break;

        case "/help":
          await HandleGetHelpAsync(msg, worker);
          break;
        case "/myrole":
          await _botClient.SendMessage(msg.Chat.Id, $"Вы {GetEnumDisplayName(worker.Role)}");
          break;
        case "/cabinets":
          await HandleGetCabinetsAsync(msg, worker);
          break;
        case "/workers":
          await HandleGetWorkersAsync(msg);
          break;
        default:
          await _botClient.SendMessage(msg.Chat.Id, "Команда не распознана или у вас нет прав. Напишите /help.");
          break;
      }
    }

    private async Task HandleGetWorkersAsync(Message msg)
    {
      try
      {
        var workers = await _db.Workers.ToListAsync();
        if (workers.Count == 0)
        {
          await _botClient.SendMessage(msg.Chat.Id, "Нет зарегистрированных пользователей.");
          return;
        }

        var buttons = workers
            .Select(w => InlineKeyboardButton.WithCallbackData(
                text: $"{w.Name} ({GetEnumDisplayName(w.Role)})",
                callbackData: $"select_user_{w.Id}"))
            .Chunk(1)
            .Select(chunk => chunk.ToArray())
            .ToArray();

        await _botClient.SendMessage(
            msg.Chat.Id,
            "Выберите пользователя:",
            replyMarkup: new InlineKeyboardMarkup(buttons));
      }
      catch (Exception ex)
      {
        await _botClient.SendMessage(msg.Chat.Id, $"Ошибка: {ex.Message}");
      }
    }

    private async Task HandleGetHelpAsync(Message msg, Worker worker)
    {
      switch (worker.Role)
      {
        case RoleType.Admin:
          await _botClient.SendMessage(msg.Chat.Id, "Доступные команды для администратора:\n" +
              "/start - Начать взаимодействие\n" +
              "/help - Получить помощь\n" +
              "/myrole - Узнать свою роль\n" +
              "/cabinets - Получить список кабинетов\n" +
              "/workers - Получить список пользователей\n");
          break;
        case RoleType.CabinetManager:
          await _botClient.SendMessage(msg.Chat.Id, "Доступные команды для менеджера кабинета:\n" +
              "/start - Начать взаимодействие\n" +
              "/help - Список команд\n" +
              "/myrole - Узнать свою роль\n" +
              "/cabinets - Список кабинетов");
          break;
        case RoleType.ClaimsManager:
          await _botClient.SendMessage(msg.Chat.Id, "Доступные команды для менеджера по возвратам:\n" +
              "/start - Начать взаимодействие\n" +
              "/help - Список команд\n" +
              "/myrole - Узнать свою роль");
          break;
        case RoleType.WarehouseStaff:
          await _botClient.SendMessage(msg.Chat.Id, "Доступные команды для склада:\n" +
              "/start - Начать взаимодействие\n" +
              "/help - Список команд\n" +
              "/myrole - Узнать свою роль");
          break;
        case RoleType.Courier:
          await _botClient.SendMessage(msg.Chat.Id, "Доступные команды для курьера:\n" +
              "/start - Начать взаимодействие\n" +
              "/help - Список команд\n" +
              "/myrole - Узнать свою роль");
          break;
        default:
          break;
      }
    }

    private async Task HandleGetCabinetsAsync(Message msg, Worker worker)
    {
      if (worker.Role != RoleType.Admin && worker.Role != RoleType.CabinetManager)
      {
        await _botClient.SendMessage(msg.Chat.Id, "У вас нет прав для просмотра кабинетов.");
        return;
      }

      try
      {
        List<Cabinet> cabinets;
        if (worker.Role == RoleType.CabinetManager)
        {
          cabinets = await _db.Cabinets
              .Where(c => c.AssignedWorkers.Any(w => w.Id == worker.Id))
              .ToListAsync();
          if (!cabinets.Any())
          {
            await _botClient.SendMessage(msg.Chat.Id, "Нет доступных кабинетов.");
            return;
          }
        }
        else
        {
          cabinets = await _db.Cabinets.ToListAsync();
          if (!cabinets.Any())
          {
            await _botClient.SendMessage(msg.Chat.Id, "Нет доступных кабинетов.");
            return;
          }
        }

        var buttons = cabinets
            .Select(c => InlineKeyboardButton.WithCallbackData(
                text: $"{c.Marketplace} / {c.Name}",
                callbackData: $"select_cab_{c.Id}"))
            .Chunk(1)
            .Select(chunk => chunk.ToArray())
            .ToList();

        // кнопка создания кабинета
        buttons.Add(new[]
        {
            InlineKeyboardButton.WithCallbackData("➕ Создать кабинет", "create_cab")
        });

        await _botClient.SendMessage(
            msg.Chat.Id,
            "Выберите кабинет:",
            replyMarkup: new InlineKeyboardMarkup(buttons.ToArray()));
      }
      catch (Exception ex)
      {
        await _botClient.SendMessage(msg.Chat.Id, $"Ошибка: {ex.Message}");
      }
    }





    private async Task HandleCallbackQueryAsync(CallbackQuery cb)
    {
      var data = cb.Data?.Split('_');
      if (data == null || data.Length < 2)
      {
        await _botClient.AnswerCallbackQuery(cb.Id, "Неподдерживаемый формат данных");
        return;
      }
      var command = string.Join("_", data.Where(item => !int.TryParse(item, out _)));
      bool anyParsableToInt = data.Any(a => int.TryParse(a, out _));
      if (!int.TryParse(data.Last(), out var id) && anyParsableToInt)
      {
        await _botClient.AnswerCallbackQuery(cb.Id, "Неверный идентификатор");
        return;
      }

      switch (command)
      {
        case "select_cab":
          await DisplayCabinetDetailsAsync(cb, id);
          break;
        case "create_cab":
          await PromptCreateCabinetAsync(cb);
          break;
        case "get_cab_users":
          await HandleGetCabinetWorkersAsync(cb, id);
          break;

        case "add_cub_user":
          await SetCabinetUserAsync(cb, data);
          break;

        case "delete_cab_user":
          await DeleteCabinetUserAsync(cb, data);
          break;

        case "add_cub_users":
          await PromptAddUserToCabinetAsync(cb, data);
          break;

        case "select_user":
          await DisplayUserDetailsAsync(cb, id);
          break;

        case "edit_user_name":
          await PromptUserNameEditAsync(cb, id);
          break;

        case "edit_user_role":
          await EditUserRoleAsync(cb, id);
          break;

        case "set_user_role":
          await SetUserRoleAsync(cb, data);
          break;

        case "delete_user":
          await DeleteUserAsync(cb, id);
          break;

        case "edit_cab_name":
          await PromptCabinetNameEditAsync(cb, id);
          break;

        case "edit_cab_settings":
          await SetCabinetSettingsAsync(cb, data);
          break;

        case "edit_cab_settings_key":
          await PromptCabinetSettingsEditKeyAsync(cb, id);
          break;

        case "edit_cab_settings_value":
          await PromptCabinetSettingsEditValueAsync(cb, id);
          break;

        case "set_cab_settings":
          await SetCabinetSettingsAsync(cb, data);
          break;

        case "delete_cab":
          await DeleteCabinetAsync(cb, id);
          break;

        default:
          await _botClient.AnswerCallbackQuery(cb.Id, "Неподдерживаемое действие.");
          break;
      }
    }

    private async Task DeleteCabinetUserAsync(CallbackQuery cb, string[] data)
    {
      if (data.Length != 5
         || !int.TryParse(data[3], out var cabinetId)
         || !int.TryParse(data[4], out var workerId))
      {
        await _botClient.AnswerCallbackQuery(cb.Id, "Неверные данные.");
        return;
      }

      var cabinet = await _db.Cabinets
          .Include(c => c.AssignedWorkers)
          .FirstOrDefaultAsync(c => c.Id == cabinetId);
      var worker = await _db.Workers.FindAsync(workerId);
      if (cabinet == null || worker == null)
      {
        await _botClient.AnswerCallbackQuery(cb.Id, "Кабинет или пользователь не найдены.");
        return;
      }
      if (!cabinet.AssignedWorkers.Any(w => w.Id == workerId))
      {
        await _botClient.AnswerCallbackQuery(cb.Id, "Пользователь не найден в кабинете.");
        return;
      }
      cabinet.AssignedWorkers.Remove(worker);
      await _db.SaveChangesAsync();
      await _botClient.AnswerCallbackQuery(cb.Id, "Пользователь успешно удалён.");

      //редактируем сообщение с пользователями кабинета
      await HandleGetCabinetWorkersAsync(cb, cabinetId);

      await _botClient.SendMessage(
          long.Parse(worker.TelegramId),
          $"Вы были удалены из кабинета: \"{cabinet.Marketplace} / {cabinet.Name}\".");
    }

    private async Task PromptCreateCabinetAsync(CallbackQuery cb)
    {
      await _botClient.AnswerCallbackQuery(cb.Id);
      await _botClient.SendMessage(
          chatId: cb.Message.Chat.Id,
          text: "Введите Marketplace для нового кабинета:",
          replyMarkup: new ForceReplyMarkup { Selective = true });
      _waitingService.Register(cb.From.Id, "create_cab_marketplace", 0);
    }

    #region Добавление пользователя в кабинет

    // 1) Показываем список всех работников, чтобы выбрать, кого добавить
    // callbackData: "add_cub_list_{cabinetId}"
    private async Task PromptAddUserToCabinetAsync(CallbackQuery cb, string[] data)
    {
      if (!int.TryParse(data.Last(), out var cabinetId))
      {
        await _botClient.AnswerCallbackQuery(cb.Id, "Неверный идентификатор кабинета.");
        return;
      }

      var workers = await _db.Workers.ToListAsync();
      if (!workers.Any())
      {
        await _botClient.AnswerCallbackQuery(cb.Id, "Нет доступных пользователей.");
        return;
      }

      var buttons = workers
          .Select(w => new[]
          {
            InlineKeyboardButton.WithCallbackData(
                text: $"{w.Name} (ID:{w.Id})",
                callbackData: $"add_cub_user_{cabinetId}_{w.Id}")
          })
          .ToList();

      // Кнопка «Назад» к списку пользователей кабинета
      buttons.Add(new[]
      {
        InlineKeyboardButton.WithCallbackData(
            text: "↩️ Назад",
            callbackData: $"get_cab_users_{cabinetId}")
    });

      await _botClient.EditMessageText(
          chatId: cb.Message.Chat.Id,
          messageId: cb.Message.MessageId,
          text: "Выберите пользователя, которого нужно добавить в кабинет:",
          replyMarkup: new InlineKeyboardMarkup(buttons)
      );
    }

    // 2) Добавляем выбранного пользователя в кабинет
    // callbackData: "add_cub_user_{cabinetId}_{workerId}"
    private async Task SetCabinetUserAsync(CallbackQuery cb, string[] data)
    {
      // data = ["add","cub","user","{cabinetId}","{workerId}"]
      if (data.Length != 5
          || !int.TryParse(data[3], out var cabinetId)
          || !int.TryParse(data[4], out var workerId))
      {
        await _botClient.AnswerCallbackQuery(cb.Id, "Неверные данные.");
        return;
      }

      var cabinet = await _db.Cabinets
          .Include(c => c.AssignedWorkers)
          .FirstOrDefaultAsync(c => c.Id == cabinetId);
      var worker = await _db.Workers.FindAsync(workerId);

      if (cabinet == null || worker == null)
      {
        await _botClient.AnswerCallbackQuery(cb.Id, "Кабинет или пользователь не найдены.");
        return;
      }

      if (cabinet.AssignedWorkers.Any(w => w.Id == workerId))
      {
        await _botClient.AnswerCallbackQuery(cb.Id, "Пользователь уже добавлен.");
        return;
      }

      cabinet.AssignedWorkers.Add(worker);
      await _db.SaveChangesAsync();

      await _botClient.AnswerCallbackQuery(cb.Id, "Пользователь успешно добавлен.");

      // отправляем уведомление пользователю
      await _botClient.SendMessage(
          long.Parse(worker.TelegramId),
          $"Вы были добавлены в кабинет  \"{cabinet.Marketplace} / {cabinet.Name}\".");

      // Обновляем список пользователей кабинета
      await HandleGetCabinetWorkersAsync(cb, cabinetId);
    }

    #endregion

    private async Task HandleGetCabinetWorkersAsync(CallbackQuery cb, int id)
    {
      var cabinet = await _db.Cabinets
          .Include(c => c.AssignedWorkers)
          .FirstOrDefaultAsync(c => c.Id == id);
      if (cabinet == null)
      {
        await _botClient.EditMessageText(cb.Message.Chat.Id, cb.Message.MessageId, "Кабинет не найден.");
        return;
      }
      var sb = new StringBuilder();
      sb.AppendLine($"✅ Сотрудники ЛК:\n{cabinet.Marketplace} / {cabinet.Name}:");
      foreach (var user in cabinet.AssignedWorkers)
        sb.AppendLine($"- {user.Name} ({GetEnumDisplayName(user.Role)})");


      //добавляем кнопки удаления пользователей уже записанных в кабинет и последней кнопкой выводим "Добавить"
      var buttons = cabinet.AssignedWorkers
          .Select(u => InlineKeyboardButton.WithCallbackData(
              text: $"{u.Name} ({GetEnumDisplayName(u.Role)})",
              callbackData: $"delete_cab_user_{cabinet.Id}_{u.Id}"))
          .Concat(new[] { InlineKeyboardButton.WithCallbackData("➕ Добавить", $"add_cub_users_{cabinet.Id}") })
          .Chunk(1)
          .Select(chunk => chunk.ToArray())
          .ToArray();

      await _botClient.EditMessageText(cb.Message.Chat.Id, cb.Message.MessageId, sb.ToString().TrimEnd(), replyMarkup: buttons);
    }

    #region Cabinet Settings Handlers

    // 1) Показать меню редактирования настроек (callbackData: "edit_cab_settings_{cabinetId}")
    private async Task SetCabinetSettingsAsync(CallbackQuery cb, string[] data)
    {
      // Парсим идентификатор кабинета
      if (!int.TryParse(data.Last(), out var cabinetId))
      {
        await _botClient.AnswerCallbackQuery(cb.Id, "Неверный идентификатор кабинета.");
        return;
      }

      // Загружаем кабинет со всеми параметрами
      var cabinet = await _db.Cabinets
          .Include(c => c.Settings)
              .ThenInclude(s => s.ConnectionParameters)
          .FirstOrDefaultAsync(c => c.Id == cabinetId);

      if (cabinet == null)
      {
        await _botClient.AnswerCallbackQuery(cb.Id, "Кабинет не найден.");
        return;
      }

      // Строим клавиатуру: для каждого параметра — две кнопки (редактировать ключ/значение)
      var buttons = cabinet.Settings.ConnectionParameters
          .Select(param => new[]
          {
            InlineKeyboardButton.WithCallbackData(
                text: $"✏️ Ключ: {param.Key}",
                callbackData: $"edit_cab_settings_key_{param.Id}"),
            InlineKeyboardButton.WithCallbackData(
                text: $"✏️ Значение: {param.Value}",
                callbackData: $"edit_cab_settings_value_{param.Id}")
          })
          .ToList();

      // (Опционально) добавить кнопку для возврата или добавления нового параметра
      buttons.Add(new[]
      {
        InlineKeyboardButton.WithCallbackData(
            text: "↩️ Назад",
            callbackData: $"select_cab_{cabinetId}")
    });

      // Обновляем сообщение с новым меню
      await _botClient.EditMessageText(
          chatId: cb.Message.Chat.Id,
          messageId: cb.Message.MessageId,
          text: "Выберите параметр для редактирования:",
          replyMarkup: new InlineKeyboardMarkup(buttons)
      );
    }

    // 2) Запросить у пользователя новый ключ (callbackData: "edit_cab_settings_key_{paramId}")
    private async Task PromptCabinetSettingsEditKeyAsync(CallbackQuery cb, int paramId)
    {
      var param = await _db.ConnectionParameters.FindAsync(paramId);
      if (param == null)
      {
        await _botClient.AnswerCallbackQuery(cb.Id, "Параметр не найден.");
        return;
      }

      // Отправляем ForceReply для ввода нового ключа
      await _botClient.SendMessage(
          chatId: cb.Message.Chat.Id,
          text: $"Введите новое имя параметра (текущее: «{param.Key}»):",
          replyMarkup: new ForceReplyMarkup { Selective = true }
      );

      // Регистрируем ожидание ответа пользователя
      _waitingService.Register(cb.From.Id, "edit_cab_settings_key", paramId);
    }

    // 3) Запросить у пользователя новое значение (callbackData: "edit_cab_settings_value_{paramId}")
    private async Task PromptCabinetSettingsEditValueAsync(CallbackQuery cb, int paramId)
    {
      var param = await _db.ConnectionParameters.FindAsync(paramId);
      if (param == null)
      {
        await _botClient.AnswerCallbackQuery(cb.Id, "Параметр не найден.");
        return;
      }

      // Отправляем ForceReply для ввода нового значения
      await _botClient.SendMessage(
          chatId: cb.Message.Chat.Id,
          text: $"Введите новое значение параметра (текущее: «{param.Value}»):",
          replyMarkup: new ForceReplyMarkup { Selective = true }
      );

      // Регистрируем ожидание ответа пользователя
      _waitingService.Register(cb.From.Id, "edit_cab_settings_value", paramId);
    }

    #endregion


    #region Callback Handlers

    private async Task DisplayCabinetDetailsAsync(CallbackQuery cb, int cabinetId)
    {
      var cabinet = await _db.Cabinets
          .Include(c => c.Settings)
              .ThenInclude(s => s.ConnectionParameters)
          .FirstOrDefaultAsync(c => c.Id == cabinetId);
      if (cabinet == null)
      {
        await _botClient.EditMessageText(cb.Message.Chat.Id, cb.Message.MessageId, "Кабинет не найден.");
        return;
      }

      var sb = new StringBuilder();
      sb.AppendLine($"✅ Кабинет:\n#{cabinet.Id} {cabinet.Marketplace}/{cabinet.Name}");
      sb.AppendLine("Настройки подключения:");
      if (cabinet?.Settings?.ConnectionParameters != null)
        foreach (var p in cabinet.Settings.ConnectionParameters)
          sb.AppendLine($"- {p.Key}: {p.Value}");

      var buttons = new[]
      {
                new[] {
                    InlineKeyboardButton.WithCallbackData("✏️ Название", $"edit_cab_name_{cabinet.Id}"),
                    InlineKeyboardButton.WithCallbackData("🛡 Настройки", $"edit_cab_settings_{cabinet.Id}")
                },
                new[] {
                    InlineKeyboardButton.WithCallbackData("❌ Удалить", $"delete_cab_{cabinet.Id}"),
                    InlineKeyboardButton.WithCallbackData("👤 Пользователи", $"get_cab_users_{cabinet.Id}")
            }
      };

      await _botClient.SendMessage(cb.Message.Chat.Id, sb.ToString().TrimEnd(), replyMarkup: new InlineKeyboardMarkup(buttons));

      //await _botClient.EditMessageText(cb.Message.Chat.Id, cb.Message.MessageId, sb.ToString().TrimEnd(), replyMarkup: new InlineKeyboardMarkup(buttons));
    }

    private async Task DisplayUserDetailsAsync(CallbackQuery cb, int userId)
    {
      var user = await _db.Workers.FirstOrDefaultAsync(w => w.Id == userId);
      if (user == null)
      {
        await _botClient.EditMessageText(cb.Message.Chat.Id, cb.Message.MessageId, "Пользователь не найден.");
        return;
      }

      //если пользователь с айди 3 то не редактируем с сообщением "Запрещено редактировать"
      if (user.TelegramId == (1406950293).ToString())
      {
        await _botClient.EditMessageText(cb.Message.Chat.Id, cb.Message.MessageId, "Запрещено редактировать этого пользователя.");
        return;
      }


      var sb = new StringBuilder();
      sb.AppendLine("✅ Пользователь:");
      sb.AppendLine($"#{user.Id} [{user.TelegramId}] {user.Name}");
      sb.AppendLine($"Роль: {user.Role}");

      var buttons = new[]
      {
                new[] {
                    InlineKeyboardButton.WithCallbackData("✏️ Изменить имя", $"edit_user_name_{user.Id}"),
                    InlineKeyboardButton.WithCallbackData("🛡 Изменить роль", $"edit_user_role_{user.Id}")
                },
                new[] {
                    InlineKeyboardButton.WithCallbackData("❌ Удалить пользователя", $"delete_user_{user.Id}")
                }
            };

      await _botClient.SendMessage(
          cb.Message.Chat.Id,
          sb.ToString().TrimEnd(),
          replyMarkup: new InlineKeyboardMarkup(buttons));
    }

    private Task PromptUserNameEditAsync(CallbackQuery cb, int userId)
    {
      _botClient.SendMessage(
          cb.Message.Chat.Id,
          $"Введите новое имя для пользователя #{userId}:",
          replyMarkup: new ForceReplyMarkup { Selective = true });

      _waitingService.Register(cb.From.Id, "edit_user_name", userId);
      return Task.CompletedTask;
    }

    private async Task EditUserRoleAsync(CallbackQuery cb, int userId)
    {
      //Получаем список ролей
      var roles = Enum.GetValues<RoleType>()
          .Select(r => InlineKeyboardButton.WithCallbackData(
              text: GetEnumDisplayName(r),
              callbackData: $"set_user_role_{(int)r}_{userId}"))
          .Chunk(2)
          .Select(chunk => chunk.ToArray())
          .ToArray();

      //получаем пользователя
      var user = await _db.Workers.FirstOrDefaultAsync(w => w.Id == userId);

      if (user == null)
      {
        await _botClient.EditMessageText(cb.Message.Chat.Id, cb.Message.MessageId, "Пользователь не найден.");
        return;
      }

      //Отправляем сообщение с кнопками
      await _botClient.EditMessageText(
            cb.Message.Chat.Id,
            cb.Message.MessageId,
            $"Выберите новую роль для пользователя:\n" +
            $"#{user.Id} [{user.TelegramId}] {user.Name}",
            replyMarkup: new InlineKeyboardMarkup(roles));
    }

    private async Task PromptUserRoleSelectionAsync(long chatId, int userId)
    {
      var user = await _db.Workers.FirstOrDefaultAsync(w => w.Id == userId);
      if (user == null) return;

      var buttons = Enum.GetValues<RoleType>()
          .Select(r => InlineKeyboardButton.WithCallbackData(
              text: GetEnumDisplayName(r),
              callbackData: $"set_user_role_{user.Id}_{(int)r}"))
          .Chunk(2)
          .Select(chunk => chunk.ToArray())
          .ToArray();

      await _botClient.SendMessage(
          chatId,
          $"Выберите новую роль для пользователя #{user.Id}:",
          replyMarkup: new InlineKeyboardMarkup(buttons));
    }
    private async Task PromptCabinetNameEditAsync(CallbackQuery cb, int id)
    {
      var cabinet = await _db.Cabinets.FirstOrDefaultAsync(c => c.Id == id);
      if (cabinet == null)
      {
        await _botClient.AnswerCallbackQuery(cb.Id, "Кабинет не найден");
        return;
      }
      await _botClient.SendMessage(
          cb.Message.Chat.Id,
          $"Введите новое имя для кабинета #{cabinet.Id}:",
          replyMarkup: new ForceReplyMarkup { Selective = true });
      _waitingService.Register(cb.From.Id, "edit_cab_name", cabinet.Id);
    }
    private async Task EditUserNameAsync(long chatId, string newName, int userId)
    {
      var user = await _db.Workers.FirstOrDefaultAsync(w => w.Id == userId);
      if (user == null) return;

      user.Name = newName.Trim();
      await _db.SaveChangesAsync();
      await _botClient.SendMessage(chatId, $"Имя пользователя обновлено на: {user.Name}");

      await _botClient.SendMessage(
          chatId: user.TelegramId,
          text: $"Теперь вас зовут: {user.Name}");
    }

    private async Task SetUserRoleAsync(CallbackQuery cb, string[] data)
    {
      if (data.Length < 4) return;
      if (!int.TryParse(data[4], out var userId) ||
          !int.TryParse(data[3], out var roleValue))
      {
        await _botClient.AnswerCallbackQuery(cb.Id, "Неверные данные");
        return;
      }

      var user = await _db.Workers.FirstOrDefaultAsync(w => w.Id == userId);
      if (user == null)
      {
        await _botClient.AnswerCallbackQuery(cb.Id, "Пользователь не найден");
        return;
      }

      user.Role = (RoleType)roleValue;
      await _db.SaveChangesAsync();
      await _botClient.EditMessageText(
          chatId: cb.Message.Chat.Id,
          messageId: cb.Message.MessageId,
          text: $"✅ Роль пользователя обновлена на: {GetEnumDisplayName(user.Role)}");

      // Отправляем уведомление пользователю о новой роли и сообщением о команде /help для получения списка доступных команд
      await _botClient.SendMessage(
          chatId: user.TelegramId,
          text: $"Ваша роль была изменена на: {GetEnumDisplayName(user.Role)}\n" +
          "/help, чтобы получить список доступных команд.");
    }

    private async Task DeleteUserAsync(CallbackQuery cb, int userId)
    {
      var user = await _db.Workers.FirstOrDefaultAsync(w => w.Id == userId);
      if (user == null)
      {
        await _botClient.EditMessageText(cb.Message.Chat.Id, cb.Message.MessageId, "Пользователь не найден.");
        return;
      }

      _db.Workers.Remove(user);
      await _db.SaveChangesAsync();
      await _botClient.EditMessageText(cb.Message.Chat.Id, cb.Message.MessageId, "Пользователь успешно удалён.");
    }

    private async Task DeleteCabinetAsync(CallbackQuery cb, int cabinetId)
    {
      var cabinet = await _db.Cabinets.FindAsync(cabinetId);
      if (cabinet == null)
      {
        await _botClient.EditMessageText(cb.Message.Chat.Id, cb.Message.MessageId, "Кабинет не найден.");
        return;
      }
      _db.Cabinets.Remove(cabinet);
      await _db.SaveChangesAsync();
      await _botClient.EditMessageText(cb.Message.Chat.Id, cb.Message.MessageId, "Кабинет успешно удалён.");
    }

    #endregion

    #region Webhook Management
    [HttpGet("getwebhookinfo")]
    public async Task<IActionResult> GetWebhookInfo() => Ok(await _botClient.GetWebhookInfo());

    [HttpGet("setwebhook")]
    public async Task<IActionResult> SetWebhook(string url)
    {
      var info = await _botClient.GetWebhookInfo();
      if (info.Url != url)
      {
        await _botClient.DeleteWebhook(true);
        await _botClient.SetWebhook(url);
      }
      return Ok();
    }

    [HttpGet("reload")]
    public async Task<IActionResult> ReloadWebhook()
    {
      var url = (await _botClient.GetWebhookInfo()).Url!;
      await _botClient.DeleteWebhook(true);
      await _botClient.SetWebhook(url);
      return Ok();
    }

    [HttpGet("clearqueue")]
    public Task<IActionResult> ClearQueue() => ReloadWebhook();
    #endregion

    #region Helper Methods
    private static string GetEnumDisplayName(Enum enumValue)
    {
      var display = enumValue.GetType()
          .GetField(enumValue.ToString())
          ?.GetCustomAttributes(typeof(DisplayAttribute), false)
          .FirstOrDefault() as DisplayAttribute;

      return display?.Name ?? enumValue.ToString();
    }

    private void LogUpdate(Update update)
    {
      var json = System.Text.Json.JsonSerializer.Serialize(update);
      var fileName = $"update_{DateTime.UtcNow:yyyyMMdd_HHmmss}.json";
      var filePath = Path.Combine("logs", "tg", fileName);
      Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
      System.IO.File.WriteAllText(filePath, json);
      _logger.LogInformation("Incoming Update: {UpdateJson}", json);
    }
    #endregion
  }
}
