namespace automation.mbtdistr.ru.temp
{
  using automation.mbtdistr.ru.Models;
  using automation.mbtdistr.ru.Data;
  using Microsoft.EntityFrameworkCore;
  using System.Text.Json;
  using System.Text;

  public class OzonSellerApiHttpClient
  {
    private readonly ApplicationDbContext _dbContext;
    private readonly HttpClient _httpClient;

    private readonly Dictionary<OzonApiRequestType, (string url, HttpMethod method)> _apiEndpoints =
      new Dictionary<OzonApiRequestType, (string url, HttpMethod method)>
      {
        { OzonApiRequestType.ExampleMethod, ("https://api-seller.ozon.ru/v1/example/method", HttpMethod.Post) },
        { OzonApiRequestType.ProductList, ("https://api-seller.ozon.ru/v2/product/list", HttpMethod.Post) },
        { OzonApiRequestType.ProductInfo, ("https://api-seller.ozon.ru/v2/product/info", HttpMethod.Post) },
        { OzonApiRequestType.CreatePosting, ("https://api-seller.ozon.ru/v1/posting/create", HttpMethod.Post) },
        { OzonApiRequestType.ReturnsList, ("https://api-seller.ozon.ru/v1/returns/list", HttpMethod.Post) }
        // можно добавлять дальше
      };

    public OzonSellerApiHttpClient()
    {
      _dbContext = new ApplicationDbContext(new DbContextOptions<ApplicationDbContext>());
      _httpClient = new HttpClient();
    }

    public async Task<HttpResponseMessage> SendRequestAsync(OzonApiRequestType requestType, object body, int cabinetId)
    {
      if (!_apiEndpoints.TryGetValue(requestType, out var endpoint))
        throw new Exception($"API endpoint for {requestType} not configured.");

      var cabinet = await _dbContext.Cabinets
        .Include(c => c.Settings)
          .ThenInclude(s => s.ConnectionParameters)
        .FirstOrDefaultAsync(c => c.Id == cabinetId);

      if (cabinet == null)
        throw new Exception("Cabinet not found");

      var clientId = cabinet.Settings.ConnectionParameters
        .FirstOrDefault(p => p.Key == "ClientId")?.Value;

      var apiKey = cabinet.Settings.ConnectionParameters
        .FirstOrDefault(p => p.Key == "ApiKey")?.Value;

      if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(apiKey))
        throw new Exception("Connection parameters missing");

      var request = new HttpRequestMessage(endpoint.method, endpoint.url);
      request.Headers.Add("Client-Id", clientId);
      request.Headers.Add("Api-Key", apiKey);
      request.Headers.Add("Content-Type", "application/json");

      if (body != null)
      {
        var json = JsonSerializer.Serialize(body);
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");
      }

      return await _httpClient.SendAsync(request);
    }
  }
}
