using automation.mbtdistr.ru.Data;
using automation.mbtdistr.ru.Models;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using System.Collections.Concurrent;
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
    public class JsonStringEnumMemberConverterFactory : JsonConverterFactory
    {
      public override bool CanConvert(Type typeToConvert) =>
          typeToConvert.IsEnum;

      public override JsonConverter CreateConverter(Type type, JsonSerializerOptions options)
      {
        var converterType = typeof(JsonEnumMemberAndNumberConverterInner<>)
            .MakeGenericType(type);
        return (JsonConverter)Activator.CreateInstance(converterType)!;
      }

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
            // Всегда приводим к нижнему регистру для ключа
            _fromString[name] = enumVal;
            _toString[enumVal] = name;
          }
        }

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

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
          if (_toString.TryGetValue(value, out var name))
            writer.WriteStringValue(name);
          else
            writer.WriteStringValue(value.ToString());
        }
      }
    }

    public static string ToJson(this object obj, JsonSerializerOptions? options = null)
    {
      var result = JsonSerializer.Serialize(obj, options ?? Options);
      return result;
    }

    public static readonly JsonSerializerOptions Options = new JsonSerializerOptions
    {
      WriteIndented = true,
      Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
      ReferenceHandler = ReferenceHandler.IgnoreCycles,
      DefaultIgnoreCondition = JsonIgnoreCondition.Never,
      Converters =
      {
        new JsonStringEnumMemberConverterFactory(),
        new DateTimeJsonConverter()
      }
    };

    public static T? FromJson<T>(this string json, JsonSerializerOptions? options = null)
    {
      var result = JsonSerializer.Deserialize<T>(json, options ?? Options);
      return result;
    }

    public class DateTimeJsonConverter : JsonConverter<DateTime>
    {
      private const string Format = "yyyy-MM-ddTHH:mm:ss.fffZ";

      public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
      {
        var str = reader.GetString();
        if (DateTime.TryParseExact(str, Format, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var dt))
          return dt.ToUniversalTime();

        return DateTime.Parse(str ?? "", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
      }

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
    private static TelegramBotClient _botClient;
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

        var chatId = 1406950293;

        string? json = obj?.ToJson(new JsonSerializerOptions(){
          Converters =
          {
            new JsonStringEnumMemberConverterFactory(),
            new DateTimeJsonConverter()
          },
          WriteIndented = true,
          Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
          DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
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
          parseMode = ParseMode.MarkdownV2;
          message = $"```json\n{message.EscapeMarkdownV2()}\n```";
        }
        else if (data != null)
        {
          await BotClient.SendDocument(
               chatId: 1406950293,
               caption: message.EscapeMarkdownV2(),
               document: new InputFileStream(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(data.ToJson())), $"{data.GetType()?.FullName?.Replace('.', '_')}.json"),
               parseMode: ParseMode.MarkdownV2);
          return;
        }
        await BotClient.SendMessage(
        chatId: 1406950293,
        text: message,
        parseMode);
      }
      catch (Exception)
      {
        throw;
      }
    }

    public static class CodeDetector
    {
      private static readonly Regex TrailingParens = new Regex(
          @"\s*\([A-Za-z0-9\s]*\)\s*$",
          RegexOptions.Compiled
      );

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
  }
}
