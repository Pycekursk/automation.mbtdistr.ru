using automation.mbtdistr.ru.Data;

using Microsoft.EntityFrameworkCore;

namespace automation.mbtdistr.ru.Services.Wildberries
{
  public class WildberriesApiService
  {
    private readonly HttpClient _httpClient;
    private readonly ApplicationDbContext _db;

    public WildberriesApiService(HttpClient httpClient, ApplicationDbContext db)
    {
      _httpClient = httpClient;
      _db = db;
    }

    public async Task<string> GetSellerInfoAsync(int cabinetId)
    {
      // Вытаскиваем токен из БД
      var token = await _db.CabinetSettings
          .Where(s => s.CabinetId == cabinetId)
          .SelectMany(s => s.ConnectionParameters)
          .Where(p => p.Key == "Token")
          .Select(p => p.Value)
          .FirstOrDefaultAsync();

      using var req = new HttpRequestMessage(
          HttpMethod.Get,
          "https://common-api.wildberries.ru/api/v1/seller-info"
      );
      req.Headers.Add("Authorization", token);

      var resp = await _httpClient.SendAsync(req);
      resp.EnsureSuccessStatusCode();
      return await resp.Content.ReadAsStringAsync();
    }

    internal async Task<dynamic> GetReturnsListAsync(int id)
    {
      throw new NotImplementedException();
    }

    // …другие методы Wildberries (GetReturnsAsync, ProcessReturnAsync и т.п.), все с cabinetId
  }
}
