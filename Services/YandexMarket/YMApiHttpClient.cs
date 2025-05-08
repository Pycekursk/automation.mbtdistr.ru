using automation.mbtdistr.ru.Models;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using static System.Net.WebRequestMethods;

namespace automation.mbtdistr.ru.Services.YandexMarket
{
  internal record EndpointDefinition(string Template, HttpMethod Method);

  public class YMApiHttpClient
  {
    private const string BaseUrl = "https://api.partner.market.yandex.ru";
    private readonly HttpClient _httpClient;
    private readonly DriveService _drive;  // для управления правами, если понадобится
    private readonly Dictionary<MarketApiRequestType, EndpointDefinition> _apiEndpoints =
        new()
        {
                { MarketApiRequestType.Campaigns,
                  new EndpointDefinition("campaigns", HttpMethod.Get) },
                { MarketApiRequestType.ReturnsList,
                  new EndpointDefinition("campaigns/{campaignId}/returns", HttpMethod.Get) },
                { MarketApiRequestType.Orders,
                  new EndpointDefinition("campaigns/{campaignId}/orders", HttpMethod.Get) }
        };

    public YMApiHttpClient(IConfiguration config)
    {
      // если в дальнейшем понадобится менять права — сразу инициализируем DriveService
      var credential = GoogleCredential
          .FromFile(config["GoogleApi:Sheets:ServiceAccountKeyFile"])
          .CreateScoped(new[]
          {
                    DriveService.Scope.Drive,
            // здесь же можно добавить другие scope’ы, если нужно
          });

      _drive = new DriveService(new BaseClientService.Initializer
      {
        HttpClientInitializer = credential,
        ApplicationName = config["GoogleApi:Drive:ApplicationName"]
      });

      _httpClient = new HttpClient();
    }

    private string BuildUrl(EndpointDefinition def, IDictionary<string, object>? pathParams)
    {
      var relative = def.Template;
      if (pathParams != null)
      {
        foreach (var kv in pathParams)
        {
          relative = relative.Replace($"{{{kv.Key}}}", kv.Value.ToString());
        }
      }
      return $"{BaseUrl.TrimEnd('/')}/{relative}";
    }

    public async Task<HttpResponseMessage> SendRequestAsync(
        MarketApiRequestType requestType,
        Cabinet cabinet,
         object? body = null,
        Dictionary<string, object>? query = null,
        Dictionary<string, object>? pathParams = null)
    {
      if (!_apiEndpoints.TryGetValue(requestType, out var def))
        throw new Exception($"API endpoint for {requestType} not configured.");

      if (cabinet?.Settings == null)
        throw new Exception("Cabinet not found");

      // 1) строим URL
      var url = BuildUrl(def, pathParams);

      // 2) добавляем query-string, если есть
      if (query?.Any() == true)
        url += "?" + string.Join("&", query.Select(kvp => $"{kvp.Key}={kvp.Value}"));

      var request = new HttpRequestMessage(def.Method, url);
      request.Headers.Accept.Add(
          new MediaTypeWithQualityHeaderValue("application/json"));

      // 3) заголовки авторизации из cabinet.Settings
      foreach (var param in cabinet.Settings.ConnectionParameters)
        request.Headers.TryAddWithoutValidation(param.Key, param.Value);

      // 4) тело запроса
      if (body != null)
      {
        var json = JsonSerializer.Serialize(body);
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");
      }

      return await _httpClient.SendAsync(request);
    }
  }
}
