namespace automation.mbtdistr.ru.Services.Ozon
{
  using automation.mbtdistr.ru.Services.Ozon.Models;
  using automation.mbtdistr.ru.temp;

  using System.Text.Json;

  public class OzonApiService
  {
    private readonly OzonSellerApiHttpClient _ozonSellerApiHttpClient;
    private readonly ILogger<OzonApiService> _logger;
    private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
      PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
      PropertyNameCaseInsensitive = true
    };

    public OzonApiService(OzonSellerApiHttpClient ozonSellerApiHttpClient, ILogger<OzonApiService> logger)
    {
      _ozonSellerApiHttpClient = ozonSellerApiHttpClient;
      _logger = logger;
    }

    public async Task<ReturnsListResponse> GetReturnsListAsync(int cabinetId, Filter filter, int limit = 500, long? lastId = null)
    {
      var requestBody = new ReturnsListRequest
      {
        Filter = filter,
        Limit = limit,
        LastId = lastId
      };

      var response = await _ozonSellerApiHttpClient.SendRequestAsync(
          OzonApiRequestType.ReturnsList,
          requestBody,
          cabinetId
      );

      response.EnsureSuccessStatusCode();
      var stream = await response.Content.ReadAsStreamAsync();
      var result = await JsonSerializer.DeserializeAsync<ReturnsListResponse>(stream, _jsonOptions);
      return result!;
    }

    public async Task RequestProductListAsync(int cabinetId)
    {
      var body = new
      {
        limit = 10,
        offset = 0,
        withDeleted = false
      };

      var response = await _ozonSellerApiHttpClient.SendRequestAsync(
        OzonApiRequestType.ProductList,
        body,
        cabinetId
      );

      if (!response.IsSuccessStatusCode)
      {
        _logger.LogError($"Ошибка получения списка товаров: {await response.Content.ReadAsStringAsync()}");
      }
    }
  }
}