using automation.mbtdistr.ru.Data;
using automation.mbtdistr.ru.Models;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Text.Unicode;
using System.Xml.Linq;

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace automation.mbtdistr.ru
{
  public static class Extensions
  {
    /// <summary>
    /// Элемент для Lookup: Id = числовое значение enum, Text = DisplayName или имя константы.
    /// </summary>
    public class LookupItem
    {
      public int Id { get; set; }
      public string Text { get; set; }
    }

    /// <summary>
    /// Возвращает список LookupItem для произвольного enum-типа.
    /// </summary>
    public static List<LookupItem> ToLookup<TEnum>() where TEnum : struct, Enum
    {
      var type = typeof(TEnum);
      return Enum.GetValues(type)
                 .Cast<Enum>()
                 .Select(e => new LookupItem
                 {
                   Id = Convert.ToInt32(e),
                   Text = GetDisplayName(e)
                 })
                 .ToList();
    }

    /// <summary>  
    /// Получает отображаемое имя для значения перечисления, используя атрибут Display.  
    /// </summary>  
    /// <param name="enumValue">Значение перечисления.</param>  
    /// <returns>Отображаемое имя или строковое представление значения перечисления, если атрибут Display отсутствует.</returns>  
    public static string GetDisplayName(this Enum enumValue)
    {
      var display = enumValue.GetType()
          .GetField(enumValue.ToString())
          ?.GetCustomAttributes(typeof(DisplayAttribute), false)
          .FirstOrDefault() as DisplayAttribute;

      return display?.Name ?? enumValue.ToString();
    }

    /// <summary>
    /// Возвращает значение, указанное в EnumMemberAttribute, или имя элемента перечисления.
    /// </summary>
    /// <param name="enumValue">Значение перечисления.</param>
    /// <returns>Строковое значение из атрибута или имя элемента.</returns>
    public static string GetEnumMemberValue(this Enum enumValue)
    {
      if (enumValue == null)
        throw new ArgumentNullException(nameof(enumValue));

      var type = enumValue.GetType();
      var memberInfo = type.GetField(enumValue.ToString());
      if (memberInfo == null)
        return enumValue.ToString();

      var attribute = memberInfo
          .GetCustomAttributes(typeof(EnumMemberAttribute), inherit: false)
          .OfType<EnumMemberAttribute>()
          .FirstOrDefault();

      return attribute?.Value ?? enumValue.ToString();
    }

    /// <summary>  
    /// Фабрика конвертеров JSON для перечислений, поддерживающая атрибут EnumMember и числовые значения.  
    /// </summary>  
    public class JsonStringEnumMemberConverterFactory : JsonConverterFactory
    {
      /// <summary>  
      /// Определяет, может ли данный тип быть преобразован этим конвертером.  
      /// </summary>  
      /// <param name="typeToConvert">Тип для проверки.</param>  
      /// <returns>True, если тип является перечислением, иначе False.</returns>  
      public override bool CanConvert(Type typeToConvert) =>
          typeToConvert.IsEnum;

      /// <summary>  
      /// Создаёт экземпляр конвертера для указанного типа.  
      /// </summary>  
      /// <param name="type">Тип, для которого создаётся конвертер.</param>  
      /// <param name="options">Опции сериализации JSON.</param>  
      /// <returns>Экземпляр JsonConverter для указанного типа.</returns>  
      public override JsonConverter CreateConverter(Type type, JsonSerializerOptions options)
      {
        var converterType = typeof(JsonEnumMemberAndNumberConverterInner<>).MakeGenericType(type);
        return (JsonConverter)Activator.CreateInstance(converterType)!;
      }

      /// <summary>  
      /// Внутренний класс конвертера для работы с перечислениями.  
      /// </summary>  
      /// <typeparam name="T">Тип перечисления.</typeparam>  
      private class JsonEnumMemberAndNumberConverterInner<T> : JsonConverter<T>
          where T : struct, Enum
      {
        private static readonly ConcurrentDictionary<string, T> _fromString;
        private static readonly ConcurrentDictionary<T, string> _toString;

        static JsonEnumMemberAndNumberConverterInner()
        {
          _fromString = new ConcurrentDictionary<string, T>(StringComparer.OrdinalIgnoreCase);
          _toString = new ConcurrentDictionary<T, string>();

          var enumType = typeof(T);
          foreach (var fi in enumType.GetFields(BindingFlags.Public | BindingFlags.Static))
          {
            var enumVal = (T)fi.GetValue(null)!;
            var em = fi.GetCustomAttribute<EnumMemberAttribute>();
            var name = em?.Value ?? fi.Name;
            _fromString[name] = enumVal;
            _toString[enumVal] = name;
          }
        }

        /// <summary>  
        /// Читает значение перечисления из JSON.  
        /// </summary>  
        /// <param name="reader">Читатель JSON.</param>  
        /// <param name="typeToConvert">Тип, который нужно преобразовать.</param>  
        /// <param name="options">Опции JSON.</param>  
        /// <returns>Значение перечисления.</returns>  
        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
          if (reader.TokenType == JsonTokenType.Number)
          {
            if (reader.TryGetInt64(out long longVal))
            {
              return (T)Enum.ToObject(typeof(T), longVal);
            }
            throw new JsonException($"Не удалось прочитать числовое значение для enum {typeof(T).Name}");
          }

          if (reader.TokenType == JsonTokenType.String)
          {
            string? str = reader.GetString();
            if (str is not null)
            {
              if (_fromString.TryGetValue(str, out var enumVal))
                return enumVal;

#if DEBUG
              System.Diagnostics.Debug.WriteLine($"[Enum Parse Warning] Невозможно преобразовать \"{str}\" в {typeof(T).Name}");
#endif

              throw new JsonException($"Невозможно преобразовать \"{str}\" в {typeof(T).Name}");
            }
          }

          throw new JsonException($"Ожидался токен String или Number, а пришёл {reader.TokenType}");
        }

        /// <summary>  
        /// Записывает значение перечисления в JSON.  
        /// </summary>  
        /// <param name="writer">Писатель JSON.</param>  
        /// <param name="value">Значение перечисления для записи.</param>  
        /// <param name="options">Опции JSON.</param>  
        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
          if (_toString.TryGetValue(value, out var name))
            writer.WriteStringValue(name);
          else
            writer.WriteStringValue(value.ToString());
        }
      }
    }
    public static readonly JsonSerializerOptions Options = new JsonSerializerOptions
    {
      WriteIndented = true,
      Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
      ReferenceHandler = ReferenceHandler.IgnoreCycles,
      PropertyNameCaseInsensitive = true,
      DefaultIgnoreCondition = JsonIgnoreCondition.Never,
      Converters =
      {
        new JsonStringEnumMemberConverterFactory(),
        new DateTimeJsonConverter()
      }
    };

    /// <summary>  
    /// Расширение для сериализации объекта в JSON.  
    /// </summary>  
    /// <param name="obj">Объект для сериализации.</param>  
    /// <param name="options">Опции сериализации JSON (необязательно).</param>  
    /// <returns>Строка JSON, представляющая объект.</returns>  
    public static string ToJson(this object obj, JsonSerializerOptions? options = null)
    {
      var result = JsonSerializer.Serialize(obj, options ?? Options);
      return result;
    }

    /// <summary>  
    /// Расширение для десериализации строки JSON в объект.  
    /// </summary>  
    /// <typeparam name="T">Тип объекта, в который нужно десериализовать.</typeparam>  
    /// <param name="json">Строка JSON для десериализации.</param>  
    /// <param name="options">Опции десериализации JSON (необязательно).</param>  
    /// <returns>Объект типа T, полученный из JSON, или null, если десериализация не удалась.</returns>  
    public static T? FromJson<T>(this string json, JsonSerializerOptions? options = null)
    {
      var result = JsonSerializer.Deserialize<T>(json, options ?? Options);
      return result;
    }

    /// <summary>  
    /// Конвертер JSON для работы с объектами DateTime в формате "yyyy-MM-ddTHH:mm:ss.fffZ".  
    /// </summary>  
    public class DateTimeJsonConverter : JsonConverter<DateTime>
    {
      private const string Format = "yyyy-MM-ddTHH:mm:ss.fffZ";

      /// <summary>  
      /// Читает значение DateTime из JSON.  
      /// </summary>  
      /// <param name="reader">Читатель JSON.</param>  
      /// <param name="typeToConvert">Тип, который нужно преобразовать.</param>  
      /// <param name="options">Опции JSON.</param>  
      /// <returns>Объект DateTime.</returns>  
      public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
      {
        var str = reader.GetString();
        if (DateTime.TryParseExact(str, Format, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var dt))
          return dt.ToUniversalTime();

        return DateTime.Parse(str ?? "", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
      }

      /// <summary>  
      /// Записывает значение DateTime в JSON.  
      /// </summary>  
      /// <param name="writer">Писатель JSON.</param>  
      /// <param name="value">Значение DateTime для записи.</param>  
      /// <param name="options">Опции JSON.</param>  
      public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
      {
        var utc = value.Kind == DateTimeKind.Utc ? value : value.ToUniversalTime();
        writer.WriteStringValue(utc.ToString(Format, CultureInfo.InvariantCulture));
      }
    }

    private static string? altronToken;
    public static string AltronToken
    {
      get
      {
        if (string.IsNullOrEmpty(altronToken))
        {
          altronToken = Program.Configuration["Altron:Token"];
        }
        return altronToken;
      }
    }

    ///<summary>  
    /// Свойство для получения экземпляра TelegramBotClient.  
    /// Если экземпляр ещё не создан, он создаётся с использованием токена Altron.  
    /// </summary>  
    private static TelegramBotClient _botClient;
    /// <summary>
    /// Получает экземпляр TelegramBotClient.
    /// </summary>
    public static TelegramBotClient BotClient
    {
      get
      {
        if (_botClient == null)
        {
          _botClient = new TelegramBotClient(AltronToken);
        }
        return _botClient;
      }
    }

    private static readonly char[] MarkdownV2ReservedChars = new[]
        {
            '_', '*', '[', ']', '(', ')', '~', '`', '>', '#', '+', '-', '=', '|', '{', '}', '.', '!'
        };

    public static async Task SendDebugObject<T>(T obj, string? caption = null)
    {
      try
      {
        using ApplicationDbContext db = new ApplicationDbContext(new DbContextOptions<ApplicationDbContext>());
        var admin =
          db?.Workers
          .Include(w => w.NotificationOptions)
          .FirstOrDefault(u => u.TelegramId == "1406950293");

        if (admin == null ||
            admin.NotificationOptions == null ||
            !admin.NotificationOptions.IsReceiveNotification ||
            !admin.NotificationOptions.NotificationLevels.Contains(NotificationLevel.Debug))
          return;

        var chatId = admin.TelegramId;

        string? json = obj?.ToJson(new JsonSerializerOptions()
        {
          Converters =
          {
            new JsonStringEnumMemberConverterFactory(),
            new DateTimeJsonConverter()
          },
          WriteIndented = true,
          Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
          DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
          ReferenceHandler = ReferenceHandler.IgnoreCycles
        });

        if (json?.Length > 3500)
        {
          await BotClient.SendDocument(
               chatId: chatId,
               caption: caption?.EscapeMarkdownV2(),
               document: new InputFileStream(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json)), $"{typeof(T)?.FullName?.Replace("automation.mbtdistr.ru", "").Replace('.', '_')}.json"),
               parseMode: ParseMode.MarkdownV2);
        }
        else
        {
          json = $"{caption?.EscapeMarkdownV2()}\n\n```\n{json?.EscapeMarkdownV2()}\n```";
          await BotClient.SendMessage(
              chatId: chatId,
              text: json,
              ParseMode.MarkdownV2);
        }
      }
      catch (Exception ex)
      {
        await BotClient.SendMessage(chatId: 1406950293, text: ex.Message);
      }
    }

    public static async Task SendDebugMessage(string message = "", object? data = null)
    {
      try
      {
        using ApplicationDbContext db = new ApplicationDbContext(new DbContextOptions<ApplicationDbContext>());
        var admin =
          db?.Workers
          .Include(w => w.NotificationOptions)
          .FirstOrDefault(u => u.TelegramId == "1406950293");

        if (admin == null ||
            admin.NotificationOptions == null ||
            !admin.NotificationOptions.IsReceiveNotification ||
            !admin.NotificationOptions.NotificationLevels.Contains(NotificationLevel.Debug))
          return;

        ParseMode parseMode = ParseMode.Html;

        if (CodeDetector.LooksLikeCode(message))
        {
          if (message.Length > 3500)
          {
            await BotClient.SendDocument(
                chatId: 1406950293,
                document: new InputFileStream(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(message)), $"{message.GetType()?.FullName?.Replace('.', '_')}.txt"),
                parseMode: ParseMode.Html);
          }
          else
          {
            message = $"```\n{message.EscapeMarkdownV2()}\n```";
            await BotClient.SendMessage(
            chatId: 1406950293,
            text: message,
            parseMode: ParseMode.MarkdownV2);
          }
        }
        else if (message.Length > 3500)
        {
          await BotClient.SendLongMessageAsync(long.Parse(admin.TelegramId), message, CancellationToken.None);
          return;
        }
        //else if (data != null)
        //{
        //  await BotClient.SendDocument(
        //       chatId: 1406950293,
        //       caption: message,
        //       document: new InputFileStream(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(data.ToJson())), $"{data.GetType()?.FullName?.Replace('.', '_')}.json"),
        //       parseMode: ParseMode.MarkdownV2);
        //  return;
        //}
      }
      catch (Exception ex)
      {
        await SendDebugMessage($"{ex.Message}\n{ex.StackTrace}");
      }
    }

    /// <summary>  
    /// Класс CodeDetector предоставляет методы для определения,  
    /// является ли строка JSON, XML или выглядит как код.  
    /// </summary>  
    public static class CodeDetector
    {
      private static readonly Regex TrailingParens = new Regex(
          @"\s*\([A-Za-z0-9\s]*\)\s*$",
          RegexOptions.Compiled
      );

      /// <summary>  
      /// Проверяет, является ли строка JSON.  
      /// </summary>  
      /// <param name="s">Строка для проверки.</param>  
      /// <returns>True, если строка является JSON, иначе False.</returns>  
      public static bool IsJson(string s)
      {
        if (string.IsNullOrWhiteSpace(s)) return false;
        s = s.Trim();
        if ((s.StartsWith("{") && s.EndsWith("}")) || (s.StartsWith("[") && s.EndsWith("]")))
        {
          try { JsonDocument.Parse(s); return true; }
          catch (JsonException) { }
        }
        return false;
      }

      /// <summary>  
      /// Проверяет, является ли строка XML.  
      /// </summary>  
      /// <param name="s">Строка для проверки.</param>  
      /// <returns>True, если строка является XML, иначе False.</returns>  
      public static bool IsXml(string s)
      {
        if (string.IsNullOrWhiteSpace(s)) return false;
        s = s.Trim();
        if (s.StartsWith("<") && s.EndsWith(">"))
        {
          try { XDocument.Parse(s); return true; }
          catch { }
        }
        return false;
      }

      private static readonly string[] CodeSignatures = new[]
      {
           "{", "}", ";", "=>", "->",
           "class ", "interface ", "public ",
           "function ", "var ", "let ", "const ",
           "#include", "using ",
           "SELECT ", "INSERT ", "UPDATE ", "DELETE ",
           "<html", "<body", "<div", "</"
       };

      private static readonly Regex SqlRegex =
          new Regex(@"\b(SELECT|INSERT|UPDATE|DELETE|CREATE|DROP)\s", RegexOptions.IgnoreCase | RegexOptions.Compiled);

      /// <summary>  
      /// Проверяет, выглядит ли строка как код.  
      /// </summary>  
      /// <param name="input">Строка для проверки.</param>  
      /// <returns>True, если строка выглядит как код, иначе False.</returns>  
      public static bool LooksLikeCode(string input)
      {
        if (string.IsNullOrWhiteSpace(input))
          return false;

        var s = TrailingParens.Replace(input, "").Trim();
        if (string.IsNullOrWhiteSpace(s))
          return false;

        if (IsJson(s) || IsXml(s))
          return true;

        if (SqlRegex.IsMatch(s))
          return true;

        if (CodeSignatures.Any(token => s.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0))
          return true;

        if (s.Contains("\n"))
        {
          var lines = s.Split('\n');
          var indentLines = lines.Count(l => l.StartsWith("  ") || l.StartsWith("\t"));
          if (indentLines >= lines.Length / 2)
            return true;
        }

        return false;
      }
    }

    /// <summary>
    /// Экранирует зарезервированные символы для MarkdownV2.
    /// </summary>
    private static string EscapeMarkdownV2(this string input)
    {
      var sb = new StringBuilder(input.Length * 2);
      foreach (var ch in input)
      {
        if (Array.Exists(MarkdownV2ReservedChars, c => c == ch))
          sb.Append('\\').Append(ch);
        else
          sb.Append(ch);
      }
      return sb.ToString();
    }

    /// <summary>
    /// Делит текст на «умные» фрагменты и отправляет их по очереди.
    /// </summary>
    public static async Task SendLongMessageAsync(this TelegramBotClient botClient, long chatId, string text, CancellationToken cancellationToken, ParseMode parseMode = ParseMode.Html)
    {
      if (string.IsNullOrEmpty(text))
        return;

      foreach (var chunk in SplitMessage(text))
      {
        await botClient.SendMessage(
            chatId: chatId,
            text: chunk,
            parseMode: parseMode,
            cancellationToken: cancellationToken);
      }
    }

    /// <summary>
    /// Делит текст на фрагменты <= TelegramMaxMessageLength,
    /// стараясь рвать по двойным/одинарным переносам или границам предложений.
    /// </summary>
    private static IEnumerable<string> SplitMessage(string text)
    {
      int pos = 0, length = text.Length;
      while (pos < length)
      {
        int maxLen = Math.Min(3750, length - pos);
        string window = text.Substring(pos, maxLen);

        int split;
        // 1) двойной перенос строки
        split = window.LastIndexOf("\n\n", StringComparison.Ordinal);
        if (split >= 0)
        {
          split += 2;
        }
        else
        {
          // 2) одинарный перенос
          split = window.LastIndexOf('\n');
          if (split >= 0)
            split += 1;
          else
          {
            // 3) граница предложения: .!? плюс пробел
            var matches = Regex.Matches(window, @"[\.\!\?]\s+");
            if (matches.Count > 0)
            {
              var m = matches[matches.Count - 1];
              split = m.Index + m.Length;
            }
            else
            {
              // 4) жёсткий лимит
              split = maxLen;
            }
          }
        }

        var part = text.Substring(pos, split).TrimEnd();
        yield return part;
        pos += split;
      }
    }
  }
}
