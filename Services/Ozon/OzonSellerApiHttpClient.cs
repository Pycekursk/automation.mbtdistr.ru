using automation.mbtdistr.ru.temp;

namespace automation.mbtdistr.ru.Services.Ozon
{
  using automation.mbtdistr.ru.Models;
  using automation.mbtdistr.ru.Data;
  using Microsoft.EntityFrameworkCore;
  using System.Text.Json;
  using System.Text;
  using System.Net.Http.Headers;

  public class OzonSellerApiHttpClient
  {
    private readonly HttpClient _httpClient;

    private readonly Dictionary<MarketApiRequestType, (string url, HttpMethod method)> _apiEndpoints =
      new Dictionary<MarketApiRequestType, (string url, HttpMethod method)>
      {

        { MarketApiRequestType.ReturnsList, ("https://api-seller.ozon.ru/v1/returns/list", HttpMethod.Post) }
        // можно добавлять дальше
      };

    public OzonSellerApiHttpClient()
    {
      _httpClient = new HttpClient();
    }

    public async Task<HttpResponseMessage> SendRequestAsync(MarketApiRequestType requestType, object body, Cabinet cabinet)
    {
      if (!_apiEndpoints.TryGetValue(requestType, out var endpoint))
        throw new Exception($"API endpoint for {requestType} not configured.");

      if (cabinet?.Settings == null)
        throw new Exception("Cabinet not found");

      var request = new HttpRequestMessage(endpoint.method, endpoint.url);
      request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
      foreach (var param in cabinet.Settings.ConnectionParameters)
      {
        //string headerKey = param.Key;
        //if (request.Headers.GetType().GetProperty(headerKey) is var header && header != null)
        //{


        // // header.SetValue(request.Headers, param.Value);
        //}

        request.Headers.Add(param.Key, param.Value);
      }

    //  request.Headers.Add("Content-Type", "application/json");

      if (body != null)
      {
        var json = JsonSerializer.Serialize(body);
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");
      }

      return await _httpClient.SendAsync(request);
    }
  }
}
