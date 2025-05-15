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
using static automation.mbtdistr.ru.Models.Internal;
using System.Text.Json;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using automation.mbtdistr.ru.Services.Wildberries.Models;
using automation.mbtdistr.ru.Services.BarcodeService;
using automation.mbtdistr.ru.Services.LLM;
using automation.mbtdistr.ru.Services.YandexMarket;
using ZXing;
using automation.mbtdistr.ru.Services;
using System.Text.Json.Serialization;
using static automation.mbtdistr.ru.Services.MarketSyncService;
using automation.mbtdistr.ru.Services.YandexMarket.Models;
using DevExpress.XtraPrinting;
using DevExtreme.AspNet.Mvc;
using ZXing.OneD;
using iText.Kernel.Pdf;
using iText.Kernel.Colors;
using iText.Kernel.Geom;
using iText.Kernel.Font;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Layout;
using iText.Layout.Element;
using Path = System.IO.Path;
using iText.IO.Font;
using iText.Kernel.Pdf.Canvas.Parser.Data;
using OpenAI.Chat;
using DevExpress.Utils.Text;

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
    private readonly BarcodeService _barcodeService;
    private readonly OpenAiApiService _openAiApiService;
    private readonly YMApiService _yMApiService;

    public TelegramBotController(
UserInputWaitingService waitingService,
ITelegramBotClient botClient,
ApplicationDbContext db,
WildberriesApiService wb,
OzonApiService oz,
BarcodeService barcodeService,
OpenAiApiService openAiApiService,
YMApiService yMApiService,
ILogger<TelegramBotController> logger)
    {
      _yMApiService = yMApiService;
      _openAiApiService = openAiApiService;
      _waitingService = waitingService;
      _botClient = botClient;
      _db = db;
      _wb = wb;
      _oz = oz;
      _logger = logger;
      _barcodeService = barcodeService;
    }
    /// <summary>  
    /// Обрабатывает входящие обновления от Telegram Bot API.  
    /// </summary>  
    /// <param name="update">Обновление, полученное от Telegram Bot API.</param>  
    /// <returns>Объект IActionResult, указывающий результат операции.</returns>
    [HttpPost, IgnoreAntiforgeryToken]
    public async Task<IActionResult> Post([FromBody] object obj)
    {
      //await Extensions.SendDebugMessage(obj.ToString());
      string objString = obj.ToString() ?? string.Empty;
      JsonSerializerOptions options = new JsonSerializerOptions
      {
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
      };
      Update? update = objString?.FromJson<Update>(options);
      if (!(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development"))
        try
        {
          string directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "logs", "tg");
          // Проверяем, существует ли директория, если нет - создаем
          if (!Directory.Exists(directoryPath))
          {
            Directory.CreateDirectory(directoryPath);
          }
          string filePath = Path.Combine(directoryPath, $"{DateTime.Now:yyyyMMddHHmmss}.json");
          await System.IO.File.WriteAllTextAsync(filePath, update?.ToJson(options));
        }
        catch (Exception ex)
        {
          await Extensions.SendDebugMessage($"public async Task<IActionResult> Post([FromBody] object obj)\n{ex.Message}");
        }

      //else
      //{
      //  string docSavePath = Path.Combine("wwwroot", "data", "1doc_cn.pdf");
      //  string translatedDocPath = Path.Combine("wwwroot", "data", "1doc_ru.pdf");



      //}

      try
      {

        //обьявляем токен для отмены
        var cts = new CancellationTokenSource();
        var ct = cts.Token;


        string caption = $"Telegram Bot API\n" +
           $"{DateTime.Now}\n" +
           $"Тип обновления: {update.Type}\n" +
           $"ID чата: {update.Message?.Chat.Id ?? update.CallbackQuery?.Message?.Chat.Id}\n" +
           $"ID пользователя: {update.Message?.From?.Id ?? update.CallbackQuery?.From.Id}\n" +
           $"Текст сообщения: {update.Message?.Text ?? update.CallbackQuery?.Data}";


        // await Extensions.SendDebugObject<Update>(update, caption);


        //var chatT = update?.Message?.Chat.Type ?? update?.CallbackQuery?.Message?.Chat.Type;
        string chatTypeString = update?.Message?.Chat?.Type.ToString() ?? update?.CallbackQuery?.Message?.Chat.Type.ToString() ?? string.Empty;
        ChatType? chatType = Enum.TryParse(chatTypeString, out ChatType result) ? result : null;
        var temp = (ChatType)chatType.GetValueOrDefault();
        //если чат не приватный, то игнорируем
        if (!(chatType == ChatType.Private))
        {
          return Ok();
        }

        var userId = update.Message?.From?.Id ?? update.CallbackQuery?.From.Id;
        var user = await _db.Workers.FirstOrDefaultAsync(w => w.TelegramId == userId.ToString());

        if (user?.Role == RoleType.Admin)
        {
          if (update.Message?.Photo != null)
          {
            await _barcodeService.HandlePhotoAsync(update);
            return Ok();
          }

          if (update.Message?.Voice != null)
          {
            // 1. Получаем файл у Telegram
            var fileId = update.Message.Voice.FileId;
            var file = await _botClient.GetFile(fileId, cancellationToken: ct);

            // 2. Скачиваем содержимое в память
            using var ms = new MemoryStream();
            await _botClient.DownloadFile(file.FilePath, ms, cancellationToken: ct);
            ms.Position = 0;

            string? fileType = update?.Message?.Voice?.MimeType?.Split('/')[1];


            // 3. Транскрибируем через Whisper
            var transcription = await _openAiApiService.TranscribeAsync(ms, $"{fileId}.{fileType}", ct);

            // 4. Отправляем результат обратно пользователю
            await _botClient.SendMessage(
                chatId: update.Message.Chat.Id,
                text: transcription,
                cancellationToken: ct
            );
            return Ok();
          }

          if (update.Message?.Audio != null)
          {
            var fileId = update.Message.Audio.FileId;
            var file = await _botClient.GetFile(fileId, cancellationToken: ct);

            using var ms = new MemoryStream();
            await _botClient.DownloadFile(file.FilePath, ms, cancellationToken: ct);
            ms.Position = 0;

            string? fileType = update?.Message?.Audio?.MimeType?.Split('/')[1];

            var transcription = await _openAiApiService.TranscribeAsync(ms, $"{fileId}.{fileType}", ct);

            await _botClient.SendMessage(
                chatId: update.Message.Chat.Id,
                text: transcription,
                cancellationToken: ct
            );
            return Ok();
          }

          if (update?.Message?.Document != null)
          {
            string docSavePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "data", update?.Message?.Document?.FileName);
            string translatedDocPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "data", $"{update?.Message?.Document?.FileName}_ru.pdf");
            var fileId = update?.Message.Document.FileId;
            var file = await _botClient.GetFile(fileId, cancellationToken: ct);
            using var ms = new MemoryStream();
            await _botClient.DownloadFile(file.FilePath, ms, cancellationToken: ct);
            ms.Position = 0;
            await using var fileStream = new FileStream(docSavePath, FileMode.Create, FileAccess.Write);
            await ms.CopyToAsync(fileStream, ct);
            fileStream.Position = 0;
            fileStream.Dispose();

            await TranslatePdfInPlaceBatchedAsync(
          docSavePath,
          translatedDocPath,
          @"wwwroot/css/devextreme/fonts/Roboto-500.ttf",
          "Russian",
          CancellationToken.None);


            //загруженный файл для отправки
            using var outFileStream = new FileStream(translatedDocPath, FileMode.Open, FileAccess.Read);
            await _botClient.SendDocument(update.Message.Chat.Id, new InputFileStream(outFileStream, translatedDocPath), cancellationToken: ct);

            //удаляем оба файла с диска
            if (System.IO.File.Exists(docSavePath))
            {
              System.IO.File.Delete(docSavePath);
            }
            if (System.IO.File.Exists(translatedDocPath))
            {
              System.IO.File.Delete(translatedDocPath);
            }
          }
        }
      }
      catch (Exception ex)
      {
        await Extensions.SendDebugMessage($"Exception - public async Task<IActionResult> Post([FromBody] Update update)\n\n{ex.Message}\n{ex.StackTrace}\n\n{ex.InnerException?.Message}");
      }

      try
      {
        var worker = await GetOrCreateWorkerAsync(update);

        if (worker.Role == RoleType.Guest)
        {
          await _botClient.SendMessage(
              worker.TelegramId,
              "Вы зарегистрированы как Гость. Ожидайте назначения роли администратором.");
          return Ok();
        }

        switch (update.Type)
        {
          case UpdateType.Message:
            var msg = update.Message!;
            if (await TryHandleForceReplyAsync(msg))
              return Ok();
            await HandleTextMessageAsync(msg, worker);
            break;

          case UpdateType.CallbackQuery:
            await HandleCallbackQueryAsync(update.CallbackQuery!, worker);
            break;

          default:
            _logger.LogWarning("Unsupported update type: {UpdateType}", update.Type);
            break;
        }
      }
      catch (Exception ex)
      {
        await Extensions.SendDebugMessage($"Exception - public async Task<IActionResult> Post([FromBody] Update update)\n{ex.Message}\n{ex.StackTrace}\n{ex.InnerException?.Message}");
      }

      return Ok();
    }


    [HttpPost("webappdata")]
    public async Task<IActionResult> WebAppData([FromBody] object obj)
    {
      await Extensions.SendDebugObject<object>(obj, "public async Task<IActionResult> WebAppData([FromBody] object obj)");
      return Ok();
    }

    [HttpGet("open")]
    public async Task<IActionResult> Open()
    {
      var chatId = 1406950293;
      var message = await _botClient.SendMessage(chatId, "Тестовое сообщение");
      return Ok(message);
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

      // _waitingService.Register(msg.From.Id, "", 0);

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
          await UpdateCabinetParameterKeyAsync(msg.Chat.Id, msg.Text!, entityId, msg.Id);
          break;
        case "edit_cab_settings_value":
          await UpdateCabinetParameterValueAsync(msg.Chat.Id, msg.Text!, entityId, msg.Id);
          break;

        case "create_cab_marketplace":
          // Передаём userId для корректной регистрации следующего шага
          await HandleCreateCabinetMarketplaceAsync(msg.Chat.Id, entityId, msg.Text!);
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
        new[] { InlineKeyboardButton.WithCallbackData("↩️ Назад", $"select_cab_{cabinet.Id}") },
        new[] {
            InlineKeyboardButton.WithCallbackData("✏️ Название", $"edit_cab_name_{cabinet.Id}"),
            InlineKeyboardButton.WithCallbackData(" Настройки", $"edit_cab_settings_{cabinet.Id}"),
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

    private async Task UpdateCabinetParameterValueAsync(long id, string v, int entityId, long messageId)
    {
      var parameter = await _db.ConnectionParameters.FindAsync(entityId);
      var cabinet = await _db.Cabinets.Include(c => c.Settings).Where(cs => cs.Settings.ConnectionParameters.Any(cp => cp.Id == entityId)).FirstOrDefaultAsync();

      if (parameter == null) return;
      parameter.Value = v;
      await _db.SaveChangesAsync();

      //устанавливаем таймер на удаление сообщения
      var message = await _botClient.SendMessage(id, $"Новое значение: {parameter.Value}");
      var delay = TimeSpan.FromSeconds(2);
      _waitingService.Remove(id);
      await Task.Delay(delay);
      await _botClient.DeleteMessage(id, message.MessageId);

      await _botClient.AnswerCallbackQuery(id.ToString(), $"Новое значение: {parameter.Value}");
    }

    private async Task UpdateCabinetParameterKeyAsync(long id, string v, int entityId, long messageId)
    {
      var parameter = await _db.ConnectionParameters.FindAsync(entityId);
      if (parameter == null) return;
      parameter.Key = v;
      await _db.SaveChangesAsync();

      //отправляем сообщение с новым значением
      await _botClient.SendMessage(id, $"Новое имя: {parameter.Key}");

      //устанавливаем таймер на удаление сообщения
      var message = await _botClient.SendMessage(id, $"Новое имя: {parameter.Key}");
      var delay = TimeSpan.FromSeconds(2);
      _waitingService.Remove(id);
      await Task.Delay(delay);
      await _botClient.DeleteMessage(id, message.MessageId);

      await _botClient.AnswerCallbackQuery(id.ToString(), $"Новое имя: {parameter.Key}");
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
      try
      {
        var text = msg.Text?.Trim().ToLower();
        switch (text)
        {
          case "/start":
            await _botClient.SendMessage(msg.Chat.Id, $"Привет, {worker.Name}! Вы {worker.Role.GetDisplayName()}");
            break;
          case "/help":
            await HandleGetHelpAsync(msg, worker);
            break;
          case "/myrole":
            await _botClient.SendMessage(msg.Chat.Id, $"Вы {worker.Role.GetDisplayName()}");
            break;
          case "/cabinets":
            await HandleGetCabinetsAsync(msg, worker);
            break;
          case "/workers":
            await HandleGetWorkersAsync(msg);
            break;
          case "/wb":
            if (worker.Role != RoleType.Admin)
            {
              await _botClient.SendMessage(msg.Chat.Id, "У вас нет прав для просмотра кабинетов.");
              return;
            }
            //получаем обьект кабинета на вб
            var cabinets = await _db.Cabinets
                   .Include(c => c.Settings)
                    .ThenInclude(s => s.ConnectionParameters).Where(c => c.Marketplace.ToUpper() == "WB").ToListAsync();

            List<Services.Wildberries.Models.ReturnsListResponse> obj = new List<Services.Wildberries.Models.ReturnsListResponse>();
            foreach (var cab in cabinets)
            {
              obj.Add(await _wb.GetReturnsListAsync(cab, true));
            }
            //  await Extensions.SendDebugObject<List<Services.Wildberries.Models.ReturnsListResponse>>(obj, $"{obj.GetType().AssemblyQualifiedName?.Replace("automation_mbtdistr_ru_", "")}");
            break;
          case "/ozon":
            if (worker.Role != RoleType.Admin)
            {
              await _botClient.SendMessage(msg.Chat.Id, "У вас нет прав для просмотра кабинетов.");
              return;
            }
            //получаем обьект кабинета на вб
            var cabinets2 = await _db.Cabinets
                 .Include(c => c.Settings)
                     .ThenInclude(s => s.ConnectionParameters).Where(c => c.Marketplace.ToUpper() == "OZON").ToListAsync();

            List<Services.Ozon.Models.ReturnsListResponse> obj2 = new List<Services.Ozon.Models.ReturnsListResponse>();

            foreach (var cab in cabinets2)
            {
              //выбираем те возвраты у которых вижуал статус айди не равен 34
              var response = await _oz.GetReturnsListAsync(cab);
              response.Returns = response.Returns.Where(r => r.Visual.Status.Id != 34 && r.Schema == "Fbo").ToList();
              obj2.Add(response);
              //obj2.Add(await _oz.GetReturnsListAsync(cab));
            }

            // await Extensions.SendDebugObject<List<Services.Ozon.Models.ReturnsListResponse>>(obj2, $"{obj2.GetType().FullName?.Replace("automation_mbtdistr_ru_", "")}");

            break;
          //case "/subscribe":
          //  MarketSyncService.ReturnStatusChanged += OnReturnStatusChanged;
          //  await _botClient.SendMessage(msg.Chat.Id, "Вы подписаны на уведомления о статусах возвратов.");
          //  break;
          //case "/unsubscribe":
          //  MarketSyncService.ReturnStatusChanged -= OnReturnStatusChanged;
          //  await _botClient.SendMessage(msg.Chat.Id, "Вы отписаны от уведомлений о статусах возвратов.");
          //  break;
          default:
            await _botClient.SendMessage(msg.Chat.Id, "Команда не распознана или у вас нет прав. Напишите /help.");
            break;
        }
      }
      catch (Exception ex)
      {
        Extensions.SendDebugMessage($"Exception - private async Task HandleTextMessageAsync(Message msg, Worker worker)\n{ex.Message}\n{ex.InnerException?.Message}\n{ex.InnerException?.StackTrace}");
        throw;
      }
    }

    private async Task HandleGetWorkersAsync(Message msg, bool editPrev = false)
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
                text: $"{w.Name} ({w.Role.GetDisplayName()})",
                callbackData: $"select_user_{w.Id}"))
            .Chunk(1)
            .Select(chunk => chunk.ToArray())
            .ToArray();

        if (editPrev)
        {
          await _botClient.EditMessageText(
              chatId: msg.Chat.Id,
              messageId: msg.MessageId,
              text: "Выберите пользователя: ",
              replyMarkup: new InlineKeyboardMarkup(buttons));
          return;
        }

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

    private async Task HandleGetCabinetsAsync(Message msg, Worker worker, bool editPrev = false)
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
            .OrderBy(c => c.Marketplace)
            .ThenBy(c => c.Name)
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


        if (editPrev)
        {
          await _botClient.EditMessageText(
              chatId: msg.Chat.Id,
              messageId: msg.MessageId,
              text: "Выберите кабинет:",
              replyMarkup: new InlineKeyboardMarkup(buttons.ToArray()));
          return;
        }


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

    private async Task HandleCallbackQueryAsync(CallbackQuery cb, Worker worker)
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

        case "list_cabs":
          await HandleGetCabinetsAsync(cb.Message, worker, true);
          break;

        case "list_workers":
          await HandleGetWorkersAsync(cb.Message, true);
          break;

        case "create_cab":
          await PromptCreateCabinetAsync(cb);
          break;
        case "get_cab_users":
          await HandleGetCabinetWorkersAsync(cb, id);
          break;

        case "add_cab_settings":
          await PromptCabinetSettingsEditKeyAsync(cb, cabId: id);
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
          await DisplayUserDetailsAsync(cb, id, true);
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
          await PromptCabinetSettingsEditKeyAsync(cb, paramId: id);
          break;

        case "edit_cab_settings_value":
          await PromptCabinetSettingsEditValueAsync(cb, id);
          break;

        case "delete_cab_settings":
          await DeleteCabinetSettingsAsync(cb, id);
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
                text: $"{w.Name} ({w.Role.GetDisplayName()})",
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
      sb.AppendLine($"✅ Сотрудники {cabinet.Marketplace} / {cabinet.Name}");
      foreach (var user in cabinet.AssignedWorkers)
        sb.AppendLine($"{user.Name} ({(user.Role)})");

      //другой вариант клавиатуры где на каждого сотрдуника в строку будет по две кнопки одна для перехода в профиль другая для удаления

      var keyboard = new List<InlineKeyboardButton[]>
      {
           new[] {
              InlineKeyboardButton.WithCallbackData("↩️ Назад", $"select_cab_{cabinet.Id}")
          }
      };

      var buttons = cabinet.AssignedWorkers
          .Select(u => new[]
          {
              InlineKeyboardButton.WithCallbackData(
                  text: $"{u.Name} ({u.Role.GetDisplayName()})",
                  callbackData: $"select_user_{u.Id}"),
              InlineKeyboardButton.WithCallbackData(
                  text: "❌ Удалить",
                  callbackData: $"delete_cab_user_{cabinet.Id}_{u.Id}")
          }
          ).ToArray();

      keyboard.AddRange(buttons);

      // Добавляем кнопку для добавления нового пользователя
      keyboard.Add(new[]
      {
          InlineKeyboardButton.WithCallbackData(
              text: "➕ Добавить пользователя",
              callbackData: $"add_cub_users_{cabinet.Id}")
      });

      await _botClient.EditMessageText(cb.Message.Chat.Id, cb.Message.MessageId, sb.ToString().TrimEnd(), replyMarkup: new InlineKeyboardMarkup(keyboard));
    }

    #region Pdf translate

    private string OpenAiTranslate(string text, string fromLang, string toLang)
    {
      _openAiApiService.TranslateText(text, toLang).ContinueWith(t => text = t.Result).Wait();
      return text;
    }

    //record TextChunk(int Page, Rectangle Rect, string Text,
    //             PdfFont Font, float FontSize);

    // добавили Angle и Rect полный
    record TextChunk(
        int Page,
        Rectangle Rect,      // полный бокс (ascent∪descent)
        string Text,
        PdfFont Font,
        float FontSize,
        float Angle          // угол вывода в радианах
    );

    class ChunkCollector : IEventListener
    {
      private readonly List<TextChunk> _store;
      private readonly int _page;
      public ChunkCollector(List<TextChunk> store, int page) =>
          (_store, _page) = (store, page);

      public void EventOccurred(IEventData data, EventType type)
      {
        if (type != EventType.RENDER_TEXT) return;
        var info = (TextRenderInfo)data;

        // 1) полный бокс текста:
        // 1. Собираем ascent и descent
        var ascent = info.GetAscentLine().GetBoundingRectangle();
        var descent = info.GetDescentLine().GetBoundingRectangle();

        // 2. Вычисляем их объединённый бокс
        float x1 = Math.Min(ascent.GetX(), descent.GetX());
        float y1 = Math.Min(ascent.GetY(), descent.GetY());
        float x2 = Math.Max(ascent.GetX() + ascent.GetWidth(),
                            descent.GetX() + descent.GetWidth());
        float y2 = Math.Max(ascent.GetY() + ascent.GetHeight(),
                            descent.GetY() + descent.GetHeight());
        var fullBox = new Rectangle(x1, y1, x2 - x1, y2 - y1);

        // 2) угол между start→end baseline:
        var bs = info.GetBaseline();
        var p0 = bs.GetStartPoint();
        var p1 = bs.GetEndPoint();
        float angle = MathF.Atan2(
            p1.Get(Vector.I2) - p0.Get(Vector.I2),
            p1.Get(Vector.I1) - p0.Get(Vector.I1)
        );

        // далее сохраняем fullBox в TextChunk вместо .Union(...)
        _store.Add(new TextChunk(
            _page,
            fullBox,
            info.GetText(),
            info.GetFont(),
            info.GetFontSize(),
            angle
        ));
      }

      public ICollection<EventType> GetSupportedEvents() =>
          new HashSet<EventType> { EventType.RENDER_TEXT };
    }


    static float FitFontSize(PdfFont font, float orig, string text, float width)
    {
      float w = font.GetWidth(text, orig);
      return w > width ? orig * width / w : orig;
    }

    public static void TranslatePdfInPlace(
     string srcPath,
     string dstPath,
     Func<string, string> translate,
     string fallbackFontPath)
    {
      // 1. Сбор текста + геометрии



      var chunks = new List<TextChunk>();
      using (var pdfR = new PdfReader(srcPath))
      using (var pdfW = new PdfWriter(dstPath))
      using (var pdfDoc = new PdfDocument(pdfR, pdfW))
      {
        for (int p = 1; p <= pdfDoc.GetNumberOfPages(); p++)
        {
          var proc = new PdfCanvasProcessor(new ChunkCollector(chunks, p));
          proc.ProcessPageContent(pdfDoc.GetPage(p));
        }
      }

      // 2. Перезапись в новом документе
      using var pdf = new PdfDocument(new PdfReader(srcPath), new PdfWriter(dstPath));
      var fallback = PdfFontFactory.CreateFont(fallbackFontPath, PdfEncodings.IDENTITY_H);

      foreach (var chunk in chunks)
      {
        var page = pdf.GetPage(chunk.Page);
        var canvas = new PdfCanvas(page);

        // 1. закрашиваем весь старый бокс
        canvas.SaveState()
              .SetFillColor(ColorConstants.WHITE)
              .Rectangle(chunk.Rect)
              .Fill()
              .RestoreState();

        // 2. перевод
        string newText = translate(chunk.Text);

        // 3. шрифт
        var fontToUse = chunk.Font.Supports(newText) ? chunk.Font : fallback;
        float size = FitFontSize(fontToUse, chunk.FontSize, newText, chunk.Rect.GetWidth());

        // 4. отрисовка с тем же углом
        new Canvas(canvas, chunk.Rect)
            .SetFont(fontToUse)
            .SetFontSize(size)
            .ShowTextAligned(
                newText,
                chunk.Rect.GetLeft(),
                chunk.Rect.GetBottom(),
              iText.Layout.Properties.TextAlignment.LEFT,
              iText.Layout.Properties.VerticalAlignment.BOTTOM,
                chunk.Angle     // здесь угол
            )
            .Close();
      }

      pdf.Close();
    }


    private const int MaxTokensPerRequest = 6000;

    /* Перерисовка PDF с учётом словаря переводов */
    //[ApiExplorerSettings(IgnoreApi = true)]
    //private static void RewritePdfWithTranslations(
    //        string srcPath,
    //        string dstPath,
    //        IEnumerable<TextChunk> chunks,
    //        IReadOnlyDictionary<string, string> dict,
    //        string fallbackFontPath)
    //{
    //  using var pdf = new PdfDocument(new PdfReader(srcPath), new PdfWriter(dstPath));
    //  var fallback = PdfFontFactory.CreateFont(fallbackFontPath, PdfEncodings.IDENTITY_H);
    //  /*  Кеш: «оригинальный шрифт из A» → «его копия в B»  */
    //  var fontCache = new Dictionary<PdfFont, PdfFont>(
    //                      ReferenceEqualityComparer.Instance);
    //  foreach (var chunk in chunks)
    //  {
    //    try
    //    {
    //      /* 1. Заменяем текст, если перевод найден; иначе — оставляем оригинал */
    //      if (!dict.TryGetValue(chunk.Text, out var newText))
    //        newText = chunk.Text;

    //      /* 2. Подбираем подходящий шрифт */
    //      /* 2.  Подбираем шрифт */
    //      PdfFont fontToUse;
    //      if (chunk.Font.Supports(newText))
    //      {
    //        /* 2.a  Нужен тот же шрифт → копируем (или берём из кеша) */
    //        if (!fontCache.TryGetValue(chunk.Font, out fontToUse))
    //        {
    //          fontToUse = chunk.Font.CopyTo(pdf);      // <<<
    //          fontCache[chunk.Font] = fontToUse;
    //        }
    //      }
    //      else
    //      {
    //        /* 2.b  Не хватает глифов → fallback */
    //        fontToUse = fallback;
    //      }

    //      float size = FitFontSize(fontToUse, chunk.FontSize, newText, chunk.Rect.GetWidth());

    //      /* 3. Протираем старый бокс и печатаем перевод */
    //      var page = pdf.GetPage(chunk.Page);
    //      var canvas = new PdfCanvas(page);

    //      canvas.SaveState()
    //            .SetFillColor(ColorConstants.WHITE)
    //            .Rectangle(chunk.Rect)
    //            .Fill()
    //            .RestoreState();

    //      new Canvas(canvas, chunk.Rect)
    //          .SetFont(fontToUse)
    //          .SetFontSize(size)
    //          .ShowTextAligned(
    //              newText,
    //              chunk.Rect.GetLeft(),
    //              chunk.Rect.GetBottom(),
    //              iText.Layout.Properties.TextAlignment.LEFT,
    //              iText.Layout.Properties.VerticalAlignment.BOTTOM,
    //              chunk.Angle)
    //          .Close();
    //    }
    //    catch (Exception ex)
    //    {

    //    }


    //  }
    //}

    /* Сбор текстовых чанков (логика 1-го этапа вынесена в отдельный метод) */
    [ApiExplorerSettings(IgnoreApi = true)]
    private static List<TextChunk> CollectChunks(string srcPath)
    {
      var chunks = new List<TextChunk>();
      using var pdfR = new PdfReader(srcPath);
      using var pdfDoc = new PdfDocument(pdfR);

      for (int p = 1; p <= pdfDoc.GetNumberOfPages(); p++)
      {
        var proc = new PdfCanvasProcessor(new ChunkCollector(chunks, p));
        proc.ProcessPageContent(pdfDoc.GetPage(p));
      }
      return chunks;
    }

    /* Строит словарь «оригинал → перевод», используя пакетную обработку */
    [ApiExplorerSettings(IgnoreApi = true)]
    private async Task<Dictionary<string, string>> BuildTranslationDictionaryAsync(
            IEnumerable<string> texts,
            string targetLanguage,
            CancellationToken ct = default)
    {
      var dict = new Dictionary<string, string>(StringComparer.Ordinal);
      var batches = SplitIntoBatches(texts);

      /* Запускаем партии параллельно, но при желании можно ограничить DegreeOfParallelism */
      var tasks = batches.Select(b => _openAiApiService.TranslateBatchAsync(b, targetLanguage, ct)).ToArray();
      var results = await Task.WhenAll(tasks);

      for (int i = 0; i < batches.Count; i++)
      {
        var src = batches[i];
        var dst = results[i];
        for (int j = 0; j < src.Length; j++)
          dict[src[j]] = dst[j];
      }
      return dict;
    }



    /* Разбиваем исходные строки на серии, укладывающиеся в лимит токенов */
    [ApiExplorerSettings(IgnoreApi = true)]
    private static List<string[]> SplitIntoBatches(IEnumerable<string> texts)
    {
      var result = new List<string[]>();
      var current = new List<string>();
      int currentTokens = 0;

      foreach (var t in texts)
      {
        int tTokens = t.Length / 3 + 10;          // грубая оценка «символы → токены»
        if (currentTokens + tTokens > MaxTokensPerRequest && current.Count > 0)
        {
          result.Add(current.ToArray());
          current = new List<string>();
          currentTokens = 0;
        }
        current.Add(t);
        currentTokens += tTokens;
      }
      if (current.Count > 0) result.Add(current.ToArray());
      return result;
    }


    /* --- Публичная точка входа: пакетный перевод + перерисовка PDF ------------ */
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task TranslatePdfInPlaceBatchedAsync(
        string srcPath,
        string dstPath,
        string fallbackFontPath,
        string targetLanguage,
        CancellationToken ct = default)
    {
      /* 1.  Открываем PDF ОДИН раз: read+write */
      using var pdf = new PdfDocument(
          new PdfReader(srcPath),
          new PdfWriter(dstPath));

      /* 1-a.  Сбор текста + геометрии */
      var chunks = new List<TextChunk>();
      for (int p = 1; p <= pdf.GetNumberOfPages(); p++)
      {
        var proc = new PdfCanvasProcessor(new ChunkCollector(chunks, p));
        proc.ProcessPageContent(pdf.GetPage(p));
      }

      /* 2.  Пакетный перевод */
      var distinctTexts = chunks.Select(c => c.Text)
                                .Distinct(StringComparer.Ordinal)
                                .ToList();

      var dict = await BuildTranslationDictionaryAsync(
                      distinctTexts, targetLanguage, ct);

      /* 3.  Перерисовка */
      var fallback = PdfFontFactory.CreateFont(
          fallbackFontPath, PdfEncodings.IDENTITY_H);

      foreach (var chunk in chunks)
      {
        /* 3-a.  Берём перевод (или оригинал) */
        var newText = dict.TryGetValue(chunk.Text, out var t) ? t : chunk.Text;

        /* 3-b.  Шрифт уже «родной» для pdf, можно пользоваться напрямую */
        var fontToUse = chunk.Font.Supports(newText) ? chunk.Font : fallback;
        float size = FitFontSize(
                            fontToUse, chunk.FontSize,
                            newText, chunk.Rect.GetWidth());

        /* 3-c.  Стираем старый бокс и печатаем текст */
        var page = pdf.GetPage(chunk.Page);
        var canvas = new PdfCanvas(page);

        canvas.SaveState()
              .SetFillColor(ColorConstants.WHITE)
              .Rectangle(chunk.Rect)
              .Fill()
              .RestoreState();

        new Canvas(canvas, chunk.Rect)
            .SetFont(fontToUse)
            .SetFontSize(size)
            .ShowTextAligned(
                newText,
                chunk.Rect.GetLeft(),
                chunk.Rect.GetBottom(),
               iText.Layout.Properties.TextAlignment.LEFT,
                iText.Layout.Properties.VerticalAlignment.BOTTOM,
                chunk.Angle)
            .Close();
      }                 // using/Dispose закроет документ ровно один раз
    }


    #endregion

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

      var keyboard = new List<InlineKeyboardButton[]>
      {
        new[] {
            InlineKeyboardButton.WithCallbackData("↩️ Назад", $"select_cab_{cabinet.Id}")
        }
      };

      // Строим клавиатуру: для каждого параметра — две кнопки (редактировать ключ/значение)
      keyboard.AddRange(cabinet.Settings.ConnectionParameters
          .Select(param => new[]
          {
            InlineKeyboardButton.WithCallbackData(
                text: $"✏️ {param.Key}",
                callbackData: $"edit_cab_settings_key_{param.Id}"),
            InlineKeyboardButton.WithCallbackData(
                text: $"✏️ {param.Value}",
                callbackData: $"edit_cab_settings_value_{param.Id}"),
             InlineKeyboardButton.WithCallbackData(
                text: "❌",
                callbackData: $"delete_cab_settings_{param.Id}")
          }));

      // (Опционально) добавить кнопку для возврата или добавления нового параметра
      keyboard.Add(
        new[]
      {
        InlineKeyboardButton.WithCallbackData(
            text: "➕ Добавить параметр",
            callbackData: $"add_cab_settings_{cabinetId}")
    });

      // Обновляем сообщение с новым меню
      await _botClient.EditMessageText(
          chatId: cb.Message.Chat.Id,
          messageId: cb.Message.MessageId,
          text: $"Кабинет {cabinet.Id}\n{cabinet.Marketplace} / {cabinet.Name}\nВыберите параметр для редактирования:",
          replyMarkup: new InlineKeyboardMarkup(keyboard)
      );
    }

    // 2) Запросить у пользователя новый ключ (callbackData: "edit_cab_settings_key_{paramId}")
    private async Task PromptCabinetSettingsEditKeyAsync(CallbackQuery cb, int cabId = 0, int paramId = 0)
    {
      var param = await _db.ConnectionParameters.FindAsync(paramId);
      if (param == null && cabId != 0)
      {
        var cabinet = _db.Cabinets
           .Where(c => c.Id == cabId)
           .Include(c => c.Settings)
           .FirstOrDefault();

        param = new Models.ConnectionParameter()
        {
          Key = "",
          Value = "",
          CabinetSettingsId = cabinet.Settings.Id,
        };

        _db.ConnectionParameters.Add(param);
        await _db.SaveChangesAsync();

      }

      await _botClient.SendMessage(
        chatId: cb.Message.Chat.Id,
        text: $"Введите новое значение параметра (текущее: «{param.Key}»):",
        replyMarkup: new ForceReplyMarkup { Selective = true }
      );

      if (paramId != 0)
        _waitingService.Register(cb.From.Id, "edit_cab_settings_key", paramId);
      else
        _waitingService.Register(cb.From.Id, "add_cab_settings", param.Id);
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

    // 4) Удалить параметр (callbackData: "delete_cab_settings_{paramId}")
    private async Task DeleteCabinetSettingsAsync(CallbackQuery cb, int paramId)
    {
      var parameter = await _db.ConnectionParameters.FindAsync(paramId);
      if (parameter == null)
      {
        await _botClient.AnswerCallbackQuery(cb.Id, "Параметр не найден.");
        return;
      }
      _db.ConnectionParameters.Remove(parameter);
      await _db.SaveChangesAsync();
      await _botClient.AnswerCallbackQuery(cb.Id, "Параметр успешно удалён.");
      await SetCabinetSettingsAsync(cb, new[] { $"edit_cab_settings_{parameter.CabinetSettingsId}" });
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
                    InlineKeyboardButton.WithCallbackData("↩️ Назад", $"list_cabs")
                },
                new[] {
                    InlineKeyboardButton.WithCallbackData("✏️ Название", $"edit_cab_name_{cabinet.Id}"),
                    InlineKeyboardButton.WithCallbackData("🛡 Настройки", $"edit_cab_settings_{cabinet.Id}")
                },
                new[] {
                    InlineKeyboardButton.WithCallbackData("❌ Удалить", $"delete_cab_{cabinet.Id}"),
                    InlineKeyboardButton.WithCallbackData("👤 Пользователи", $"get_cab_users_{cabinet.Id}")
            }
      };

      await _botClient.EditMessageText(cb.Message.Chat.Id, cb.Message.MessageId, sb.ToString().TrimEnd(), replyMarkup: new InlineKeyboardMarkup(buttons));

      //await _botClient.EditMessageText(cb.Message.Chat.Id, cb.Message.MessageId, sb.ToString().TrimEnd(), replyMarkup: new InlineKeyboardMarkup(buttons));
    }

    private async Task DisplayUserDetailsAsync(CallbackQuery cb, int userId, bool editPrev = false)
    {
      var user = await _db.Workers.FirstOrDefaultAsync(w => w.Id == userId);
      if (user == null)
      {
        var backButton = new[]
        {
                new[] {
                    InlineKeyboardButton.WithCallbackData("↩️ Назад", $"list_workers")
                }
            };
        await _botClient.EditMessageText(cb.Message.Chat.Id, cb.Message.MessageId, "Пользователь не найден.", replyMarkup: backButton);

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
      sb.AppendLine($"#{user.Id}");
      sb.AppendLine($"Имя: {user.Name}");
      sb.AppendLine($"Telegram ID: {user.TelegramId}");
      sb.AppendLine($"Создан: {user.CreatedAt}");
      sb.AppendLine($"Роль: {user.Role.GetDisplayName()}");

      var buttons = new[]
      {
                new[] {
                    InlineKeyboardButton.WithCallbackData("↩️ Назад", $"list_workers")
                },
                new[] {
                    InlineKeyboardButton.WithCallbackData("✏️ Изменить имя", $"edit_user_name_{user.Id}"),
                    InlineKeyboardButton.WithCallbackData("🛡 Изменить роль", $"edit_user_role_{user.Id}")
                },
                new[] {
                    InlineKeyboardButton.WithCallbackData("❌ Удалить пользователя", $"delete_user_{user.Id}")
                }
            };

      if (editPrev && cb.Message != null)
      {
        await _botClient.EditMessageText(
            chatId: cb.Message.Chat.Id,
            messageId: cb.Message.MessageId,
            text: sb.ToString().TrimEnd(),
            replyMarkup: new InlineKeyboardMarkup(buttons));
        return;
      }

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
      List<InlineKeyboardButton[]> buttons = new()
      {
          new[] {
              InlineKeyboardButton.WithCallbackData("↩️ Назад", $"select_user_{userId}")
          }
      };


      //Получаем список ролей
      var roles = Enum.GetValues<RoleType>()
          .Select(r => InlineKeyboardButton.WithCallbackData(
              text: r.GetDisplayName(),
              callbackData: $"set_user_role_{(int)r}_{userId}"))
          .Chunk(2)
          .Select(chunk => chunk.ToArray())
          .ToArray();

      buttons.AddRange(roles);

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
            replyMarkup: new InlineKeyboardMarkup(buttons));
    }

    private async Task PromptUserRoleSelectionAsync(long chatId, int userId)
    {
      var user = await _db.Workers.FirstOrDefaultAsync(w => w.Id == userId);
      if (user == null) return;


      var keyboard = new List<InlineKeyboardButton[]>
      {
           new[] {
              InlineKeyboardButton.WithCallbackData("↩️ Назад", $"list_workers")
          }
      };

      keyboard.AddRange(
        Enum.GetValues<RoleType>()
          .Select(r => InlineKeyboardButton.WithCallbackData(
              text: r.GetDisplayName(),
              callbackData: $"set_user_role_{user.Id}_{(int)r}"))
          .Chunk(2)
          .Select(chunk => chunk.ToArray())
          .ToArray()
        );

      await _botClient.SendMessage(
          chatId,
          $"Выберите новую роль для пользователя #{user.Id}:",
          replyMarkup: new InlineKeyboardMarkup(keyboard));
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
          $"Введите новое имя для кабинета {cabinet.Marketplace} / {cabinet.Name} ({cabinet.Id}):",
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
          text: $"✅ Роль пользователя обновлена на: {user.Role.GetDisplayName()}");

      // Отправляем уведомление пользователю о новой роли и сообщением о команде /help для получения списка доступных команд
      await _botClient.SendMessage(
          chatId: user.TelegramId,
          text: $"Ваша роль была изменена на: {user.Role.GetDisplayName()}\n" +
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
