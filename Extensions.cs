using automation.mbtdistr.ru.Data;
using automation.mbtdistr.ru.Models;

using Microsoft.EntityFrameworkCore;

using System.Collections.Concurrent;
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
        return (JsonConverter)Activator.CreateInstance(converterType);
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
            var enumVal = (T)fi.GetValue(null);
            var em = fi.GetCustomAttribute<EnumMemberAttribute>();
            var name = em?.Value ?? fi.Name;
            _fromString[name] = enumVal;
            _toString[enumVal] = name;
          }
        }

        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
          // 1) если в JSON приходит число — просто приводим
          if (reader.TokenType == JsonTokenType.Number)
          {
            if (reader.TryGetInt64(out long longVal))
            {
              return (T)Enum.ToObject(typeof(T), longVal);
            }
            throw new JsonException($"Не удалось прочитать числовое значение для enum {typeof(T).Name}");
          }

          // 2) если строка — смотрим в наш словарь из EnumMember.Value
          if (reader.TokenType == JsonTokenType.String)
          {
            string? str = reader.GetString();
            if (str is not null && _fromString.TryGetValue(str, out var enumVal))
              return enumVal;

            throw new JsonException($"Невозможно преобразовать \"{str}\" в {typeof(T).Name}");
          }

          throw new JsonException($"Ожидался токен String или Number, а пришёл {reader.TokenType}");
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
          // при сериализации всегда пишем строку из EnumMember (или имя, если атрибута нет)
          if (_toString.TryGetValue(value, out var name))
            writer.WriteStringValue(name);
          else
            writer.WriteStringValue(value.ToString());
        }
      }
    }


    public static string ToJson(this object obj)
    {
      var result = JsonSerializer.Serialize(obj, Options);
      byte[] bom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true).GetPreamble();  // EF BB BF
      byte[] body = Encoding.UTF8.GetBytes(result);
      byte[] all = new byte[bom.Length + body.Length];
      Buffer.BlockCopy(bom, 0, all, 0, bom.Length);
      Buffer.BlockCopy(body, 0, all, bom.Length, body.Length);
      return Encoding.UTF8.GetString(all);
    }

    static readonly JsonSerializerOptions Options = new JsonSerializerOptions
    {
      WriteIndented = true,
      Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
      ReferenceHandler = ReferenceHandler.IgnoreCycles,
      Converters =
      {
       new  JsonStringEnumMemberConverterFactory(),
      }
    };

    public static T FromJson<T>(this string json)
    {
      var result = JsonSerializer.Deserialize<T>(json, Options);
      return result;
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

    public static async Task SendDebugObject<T>(object obj, string? caption = null)
    {
      //получаем дб контекст из сервиса
      var db = Program.Services.BuildServiceProvider(true).GetService(typeof(ApplicationDbContext)) as ApplicationDbContext;
      var admin = 
        db?.Workers
        .Include(w => w.NotificationOptions)
        .FirstOrDefault(u => u.TelegramId == "1406950293");

      if (admin == null ||
          admin.NotificationOptions == null ||
          !admin.NotificationOptions.NotificationLevels.Contains(NotificationLevel.DeepDegugNotification))
        return;

      //получаем обьект Enveronment
      var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
      if (env == "Development") return;


      //проверяем размер сообщения
      var json = obj.ToJson();
      if (json.Length < 4000)
      {
        await SendDebugMessage(json, caption);
        return;
      }
      var chatId = 1406950293;
      BotClient.SendDocument(
           chatId: chatId,
           caption: caption?.EscapeMarkdownV2(),
           document: new InputFileStream(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(obj.ToJson())), $"{typeof(T)?.FullName?.Replace('.', '_')}.json"),
           parseMode: ParseMode.MarkdownV2);
    }

    public static async Task SendDebugMessage(string message, string? caption = null)
    {

      var db = Program.Services.BuildServiceProvider(true).GetService(typeof(ApplicationDbContext)) as ApplicationDbContext;
      var admin =
        db?.Workers
        .Include(w => w.NotificationOptions)
        .FirstOrDefault(u => u.TelegramId == "1406950293");

      if (admin == null ||
          admin.NotificationOptions == null ||
          !admin.NotificationOptions.NotificationLevels.Contains(NotificationLevel.LogNotification))
        return;


      var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
      if (env == "Development") return;
      var chatId = 1406950293;
      if (CodeDetector.LooksLikeCode(message))
        message = $"```\n{EscapeMarkdownV2(message)}\n```";
      else
        message = EscapeMarkdownV2(message);
      BotClient.SendMessage(
           chatId: chatId,
           text: message,
           ParseMode.MarkdownV2);
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
