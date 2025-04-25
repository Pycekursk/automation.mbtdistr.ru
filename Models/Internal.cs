using System.ComponentModel.DataAnnotations;
using System.Net;

namespace automation.mbtdistr.ru.Models
{
  public static class Internal
  {
    public static string GetEnumDisplayName(Enum enumValue)
    {
      var display = enumValue.GetType()
          .GetField(enumValue.ToString())
          ?.GetCustomAttributes(typeof(DisplayAttribute), false)
          .FirstOrDefault() as DisplayAttribute;

      return display?.Name ?? enumValue.ToString();
    }

    public static string EscapeHtml(this string input) =>
     input.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");

    /// <summary>
    /// Фабрика для создания клиента HTTP-прокси.
    /// </summary>
    public class HttpProxyClientFactory : IHttpClientFactory
    {
      private string _proxyString;

      public HttpProxyClientFactory(string proxyString)
      {
        _proxyString = proxyString;
      }

      /// <summary>
      /// Создает экземпляр HttpClient с настройками прокси.
      /// </summary>
      /// <param name="name">Имя клиента.</param>
      /// <returns>Экземпляр HttpClient с настройками прокси.</returns>
      public HttpClient CreateClient(string name)
      {
        try
        {
          //var proxyString = builder.Build().GetSection("Proxy:String").Value;
          var login = _proxyString?.Split("@")[0].Split(":")[0];
          var password = _proxyString?.Split("@")[0].Split(":")[1];
          var address = _proxyString?.Split("@")[1].Split(":")[0];
          var port = _proxyString?.Split("@")[1].Split(":")[1];

          var proxyCredentials = new NetworkCredential(login, password);
          var proxy = new WebProxy($"{address}:{port}")
          {
            Credentials = proxyCredentials
          };
          var httpClientHandler = new HttpClientHandler()
          {
            Proxy = proxy,
            UseProxy = true
          };
          return new HttpClient(httpClientHandler);
        }
        catch
        {
          throw;
        }
      }
    }
  }
}
