using automation.mbtdistr.ru.Models;
using automation.mbtdistr.ru.Services.Ozon;
using automation.mbtdistr.ru.Services.YandexMarket.Models;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

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



    public async Task<ReturnsListResponse?> GetReturnsListAsync(Cabinet cabinet, Campaign campaign, YMFilter? filter = null, int limit = 500, long? lastId = null)
    {
      if (filter == null)
      {
        filter = new YMFilter
        {
          FromDate = DateTime.Now.AddDays(-20).ToString("yyyy-MM-dd"),
          ToDate = DateTime.Now.ToString("yyyy-MM-dd"),
          //Type = YMReturnType.Unredeemed,
          //Statuses = new List<YMRefundStatusType> { YMRefundStatusType.StartedByUser, YMRefundStatusType.RefundInProgress, YMRefundStatusType.RefundedWithBonuses, YMRefundStatusType.DecisionMade, YMRefundStatusType.RefundedByShop, YMRefundStatusType.WaitingForDecision }
        };
      }

      try
      {
        var response = await _yMApiHttpClient.SendRequestAsync(
            MarketApiRequestType.ReturnsList,
            cabinet,
            null,
            query: filter.ToQueryParams(),
            pathParams: new Dictionary<string, object>
            {
              { "campaignId", campaign.Id }
            }
        );
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        await Extensions.SendDebugMessage(json);
        var obj = Newtonsoft.Json.JsonConvert.DeserializeObject<ReturnsListResponse>(json, new JsonSerializerSettings()
        {
          Converters = new List<JsonConverter>
          {
            new StringEnumConverter()
          }
        });
        return obj;
      }
      catch (Exception ex)
      {
        await Extensions.SendDebugMessage($"Ошибка получения списка возвратов: {ex.Message}");
        return default;
      }
    }
  }
}
