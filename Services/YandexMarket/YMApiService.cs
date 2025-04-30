using automation.mbtdistr.ru.Models;
using automation.mbtdistr.ru.Services.Ozon;

using static automation.mbtdistr.ru.Services.YandexMarket.Models.DTOs;

namespace automation.mbtdistr.ru.Services.YandexMarket
{
  public class YMApiService
  {
    private readonly YMApiHttpClient _yMApiHttpClient;
    public YMApiService(YMApiHttpClient yMApiHttpClient)
    {
      _yMApiHttpClient = yMApiHttpClient;
    }

    public async Task<CampaignsResponse> GetCampaignsAsync(Cabinet cabinet)
    {
      try
      {
        var response = await _yMApiHttpClient.SendRequestAsync(
            MarketApiRequestType.Campaigns,
            cabinet
        );
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        var obj = json.FromJson<CampaignsResponse>();
        return obj;
      }
      catch (Exception)
      {
        throw;
      }
    }



    public async Task<ReturnsListResponse> GetReturnsListAsync(Cabinet cabinet, Campaign campaign, Filter? filter = null, int limit = 500, long? lastId = null)
    {
      var queryParams = new Dictionary<string, object>
      {
        { "limit", limit.ToString() },
        { "fromDate", DateTime.Now.AddDays(-40).ToString("yyyy-MM-dd")},
        { "toDate", DateTime.Now.AddDays(-40).ToString("yyyy-MM-dd") }
      };

      try
      {
        var response = await _yMApiHttpClient.SendRequestAsync(
            MarketApiRequestType.ReturnsList,
            cabinet,
            null,
            query: queryParams,
            pathParams: new Dictionary<string, object>
            {
              { "campaignId", campaign.Id }
            }
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
