using automation.mbtdistr.ru.Models;
using automation.mbtdistr.ru.temp;

using System.Net.Security;
using System.Text;
using System.Text.Json;

namespace automation.mbtdistr.ru.Services.Wildberries
{
  public class WBApiHttpClient
  {
    private readonly Dictionary<MarketApiRequestType, (string url, HttpMethod method)> _apiEndpoints =
      new Dictionary<MarketApiRequestType, (string url, HttpMethod method)>
      {
        { MarketApiRequestType.ReturnsList, ("https://returns-api.wildberries.ru/api/v1/claims", HttpMethod.Get) },
        { MarketApiRequestType.SellerInfo, ("https://common-api.wildberries.ru/api/v1/seller-info", HttpMethod.Get) },
        { MarketApiRequestType.Ping, ("https://api.wildberries.ru/api/v1/ping", HttpMethod.Get) },
      };


    private readonly HttpClient _httpClient;


    public WBApiHttpClient()
    {

      var handler = new HttpClientHandler
      {
        ServerCertificateCustomValidationCallback = (request, cert, chain, errors) =>
        {
          // Разрешаем RemoteCertificateNameMismatch, но требуем отсутствия других ошибок
          if (errors == SslPolicyErrors.RemoteCertificateNameMismatch)
            return true;
          return errors == SslPolicyErrors.None;
        }
      };
      _httpClient = new HttpClient(handler);

    }

    public async Task<HttpResponseMessage> SendRequestAsync(MarketApiRequestType requestType, Cabinet cabinet, object? body = null, Dictionary<string, string>? queryParams = null)
    {
      if (!_apiEndpoints.TryGetValue(requestType, out var endpoint))
        throw new Exception($"API endpoint for {requestType} not configured.");
      if (cabinet == null)
        throw new Exception("Cabinet not found");
      try
      {
        if (queryParams != null && queryParams.Count > 0)
        {
          var queryString = string.Join("&", queryParams.Select(kvp => $"{kvp.Key}={kvp.Value}"));
          endpoint.url += "?" + queryString;
        }
        var request = new HttpRequestMessage(endpoint.method, endpoint.url);
        foreach (var param in cabinet.Settings.ConnectionParameters)
        {
          //request.Headers.Add(param.Key, param.Value);
          request.Headers.TryAddWithoutValidation(param.Key, param.Value);
        }
        if (body != null)
        {
          //var json = JsonSerializer.Serialize(body);
          var json = body.ToJson();
          request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }
        var response = await _httpClient.SendAsync(request);
        return response;
      }
      catch (Exception ex)
      {
        Extensions.SendDebugObject<Exception>(ex);
        throw new Exception($"Error in WBApiHttpClient: {ex.Message}", ex);
      }
    }
  }
}
