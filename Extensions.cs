using automation.mbtdistr.ru.Data;
using automation.mbtdistr.ru.Models;

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
      //получаем обьект Enveronment
      var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
      if (env == "Development") return;


      //проверяем размер сообщения
      var json = obj.ToJson();
      if (json.Length < 4000)
      {
        await SendDebugMessage(@$"{json}\n\n\SendDebugObject:165", caption);
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
      var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
      if (env == "Development") return;
      var chatId = 1406950293;

      var parseMode = ParseMode.MarkdownV2;

      if (CodeDetector.LooksLikeCode(message))
      {
        message = $"```\n{EscapeMarkdownV2(message)}\n\n\nCodeDetector.LooksLikeCode(message):186```";
        parseMode = ParseMode.MarkdownV2;
      }
      else
        message += $"\n\n\nNot fixed, not a code";

      BotClient.SendMessage(
           chatId: chatId,
           text: message,
           parseMode);
    }

    public static class CodeDetector
    {
      // 1. Проверка на валидный JSON
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

      // 2. Проверка на валидный XML/HTML
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

      // 3. Общие «кодовые» токены и ключевые слова
      private static readonly string[] CodeSignatures = new[]
      {
        "{", "}", ";", "(", ")", "=>", "->",      // общий синтаксис
        "class ", "interface ", "public ",        // C#/Java
        "function ", "var ", "let ", "const ",    // JS/TS
        "#include", "using ",                     // C/C++, C#
        "SELECT ", "INSERT ", "UPDATE ", "DELETE ", // SQL
        "<html", "<body", "<div", "</",           // HTML
    };

      // 4. Регекс для SQL-запросов
      private static readonly Regex SqlRegex =
          new Regex(@"\b(SELECT|INSERT|UPDATE|DELETE|CREATE|DROP)\s", RegexOptions.IgnoreCase);

      /// <summary>
      /// Основная функция: если что-либо «похоже на код», возвращает true.
      /// </summary>
      public static bool LooksLikeCode(string s)
      {
        if (string.IsNullOrWhiteSpace(s))
          return false;

        // —– 1. JSON или XML
        if (IsJson(s) || IsXml(s))
          return true;

        // —– 2. SQL
        if (SqlRegex.IsMatch(s))
          return true;

        // —– 3. Наличие характерных токенов
        if (CodeSignatures.Any(token => s.Contains(token, StringComparison.OrdinalIgnoreCase)))
          return true;

        // —– 4. Многострочный текст с отступами
        if (s.Contains("\n"))
        {
          var lines = s.Split('\n');
          // если хотя бы половина строк начинается с 2+ пробелов или таба
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
