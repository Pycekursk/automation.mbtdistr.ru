using automation.mbtdistr.ru.Data;
using automation.mbtdistr.ru.Models;
using automation.mbtdistr.ru.Services.Wildberries.Models;
using automation.mbtdistr.ru.temp;

using Microsoft.EntityFrameworkCore;

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
           { "is_archive", archive.ToString().ToLowerInvariant() }
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
