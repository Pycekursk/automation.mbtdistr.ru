namespace automation.mbtdistr.ru.Services.Ozon
{
  using automation.mbtdistr.ru.Models;
  using automation.mbtdistr.ru.Data;
  using Microsoft.EntityFrameworkCore;
  using System.Text.Json;
  using System.Text;
  using System.Net.Http.Headers;
  using automation.mbtdistr.ru.Services.YandexMarket;
  using automation.mbtdistr.ru.Services;
  using Newtonsoft.Json;

  public class OzonSellerApiHttpClient
  {
    internal record EndpointDefinition(string Template, HttpMethod Method);
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "https://api-seller.ozon.ru";
    private readonly Dictionary<MarketApiRequestType, EndpointDefinition> _apiEndpoints =
      new()
      {
                { MarketApiRequestType.ReturnsList,
                  new EndpointDefinition("/v1/returns/list", HttpMethod.Post) },
                { MarketApiRequestType.ProductList,
                  new EndpointDefinition("/v3/product/list", HttpMethod.Post) },
                { MarketApiRequestType.ProductInfo,
                  new EndpointDefinition("/v3/product/info/list", HttpMethod.Post) },
      };

    public OzonSellerApiHttpClient()
    {
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

    public async Task<HttpResponseMessage> SendRequestAsync(MarketApiRequestType requestType, Cabinet cabinet, object? body = null, Dictionary<string, object>? queryParams = null, Dictionary<string, object>? pathParams = null)
    {
      if (!_apiEndpoints.TryGetValue(requestType, out var endpoint))
        throw new Exception($"API endpoint for {requestType} not configured.");

      if (cabinet?.Settings == null)
        throw new Exception("Cabinet not found");

      var url = BuildUrl(endpoint, pathParams);

      var request = new HttpRequestMessage(endpoint.Method, url);
      request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
      foreach (var param in cabinet.Settings.ConnectionParameters)
      {
        request.Headers.TryAddWithoutValidation(param.Key, param.Value);
      }

      //  request.Headers.Add("Content-Type", "application/json");

      if (body != null)
      {
        request.Content = new StringContent(body.ToJson(), Encoding.UTF8, "application/json");

      }

      try
      {
        var response = await _httpClient.SendAsync(request);
        var json = await response.Content.ReadAsStringAsync();
        return response;
      }
      catch (Exception)
      {
        throw;
      }
    }


  
  }
}
