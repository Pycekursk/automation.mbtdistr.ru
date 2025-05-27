using automation.mbtdistr.ru.Data;
using automation.mbtdistr.ru.Models;
using automation.mbtdistr.ru.Services.Wildberries.Models;

using Microsoft.EntityFrameworkCore;

using Newtonsoft.Json;

using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace automation.mbtdistr.ru.Services.Wildberries
{
  public class WildberriesApiService
  {
    private readonly WBApiHttpClient _wbApiHttpClient;

    public WildberriesApiService(WBApiHttpClient wbApiHttpClient)
    {
      _wbApiHttpClient = wbApiHttpClient;


    }

    public async Task<string> GetSellerInfoAsync(Cabinet cabinet)
    {
      // Вытаскиваем токен из БД
      var response = await _wbApiHttpClient.SendRequestAsync(
          MarketApiRequestType.SellerInfo,
          cabinet
      );
      response.EnsureSuccessStatusCode();
      var json = await response.Content.ReadAsStringAsync();
      return json;
    }

    public async Task<string> PingAsync(Cabinet cabinet)
    {
      // Вытаскиваем токен из БД
      var response = await _wbApiHttpClient.SendRequestAsync(
          MarketApiRequestType.Ping,
          cabinet
      );
      response.EnsureSuccessStatusCode();
      var json = await response.Content.ReadAsStringAsync();
      return json;
    }

    internal async Task<ReturnsListResponse?> GetReturnsListAsync(Cabinet cabinet, bool archive = false)
    {
      try
      {
        var response = await _wbApiHttpClient.SendRequestAsync(
        MarketApiRequestType.ReturnsList,
        cabinet,
         queryParams: new Dictionary<string, string>
         {
           { "is_archive", archive.ToString().ToLowerInvariant() },
           { "limit", "200" }
         }
        );
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var obj = json.FromJson<ReturnsListResponse>();
        return obj;
      }
      catch (Exception ex)
      {
        await Extensions.SendDebugMessage($"internal async Task<ReturnsListResponse?> GetReturnsListAsync(Cabinet cabinet, bool archive = false)\n\n{ex.Message}\n\n{ex.InnerException?.Message}");
        throw;
      }
    }

    //метод получения списка всех заказов
    public async Task<WBOrdersListResponse?> GetOrdersListAsync(Cabinet cabinet, DateTime? dateFrom = null, int next = 1, int limit = 1000)
    {
      try
      {
        Dictionary<string, string> queryParams = new Dictionary<string, string>
        {
          { "next", next.ToString() },
          { "limit", limit.ToString() }
        };

        if (dateFrom.HasValue)
          queryParams.Add("dateFrom", dateFrom.Value.ToString("yyyy-MM-ddTHH:mm:ss"));
        else
          queryParams.Add("dateFrom", DateTime.Now.AddDays(-90).ToString("yyyy-MM-ddTHH:mm:ss"));

        var response = await _wbApiHttpClient.SendRequestAsync(
          MarketApiRequestType.Orders,
          cabinet,
          queryParams: queryParams
        );
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        var obj = json.FromJson<WBOrdersListResponse>();
        return obj;
      }
      catch (Exception ex)
      {
        await Extensions.SendDebugMessage($"public async Task<OrdersListResponse?> GetOrdersListAsync(Cabinet cabinet, int page = 1, int limit = 100)\n\n{ex.Message}\n\n{ex.InnerException?.Message}");
        throw;
      }
    }
  }


}
