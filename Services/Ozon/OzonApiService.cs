namespace automation.mbtdistr.ru.Services.Ozon
{
  using automation.mbtdistr.ru.Models;
  using automation.mbtdistr.ru.Services;
  using automation.mbtdistr.ru.Services.Ozon.Models;

  using System.Text.Json;

  public class OzonApiService
  {
    private readonly OzonSellerApiHttpClient _ozonSellerApiHttpClient;
    private readonly ILogger<OzonApiService> _logger;
   
    public OzonApiService(OzonSellerApiHttpClient ozonSellerApiHttpClient, ILogger<OzonApiService> logger)
    {
      _ozonSellerApiHttpClient = ozonSellerApiHttpClient;
      _logger = logger;
    }

    public async Task<ReturnsListResponse> GetReturnsListAsync(Cabinet cabinet, Filter? filter = null, int limit = 500, long? lastId = null)
    {
      if(filter == null)
      {
        filter = new Services.Ozon.Models.Filter();
        filter.LogisticReturnDate = new Services.Ozon.Models.DateRange
        {
          From = DateTime.UtcNow.AddDays(-40),
          To = DateTime.UtcNow
        };
      }

      var requestBody = new ReturnsListRequest
      {
        Filter = filter,
        Limit = limit,
        LastId = lastId,
      };

      try
      {
        var response = await _ozonSellerApiHttpClient.SendRequestAsync(
            MarketApiRequestType.ReturnsList,
            cabinet,
            requestBody
        );

        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        var obj = json.FromJson<ReturnsListResponse>();
        return obj;
      }
      catch (Exception)
      {
        throw;
      }
    }
  }
}