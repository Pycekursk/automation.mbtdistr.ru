using automation.mbtdistr.ru.Data;

using Microsoft.EntityFrameworkCore;

namespace automation.mbtdistr.ru.Services.Wildberries
{
  // Добавляем токен из БД в каждый запрос
  public class WildberriesAuthHandler : DelegatingHandler
  {
    private readonly ApplicationDbContext _db;
    public WildberriesAuthHandler(ApplicationDbContext db) => _db = db;

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage req, CancellationToken ct)
    {
      var cabinet = await _db.Cabinets
          .Include(c => c.Settings)
            .ThenInclude(s => s.ConnectionParameters)
          .FirstOrDefaultAsync(c => c.Name == "Wildberries", ct);

      var token = cabinet?.Settings.ConnectionParameters
          .First(p => p.Key == "Token").Value;

      if (!string.IsNullOrEmpty(token))
      {
        req.Headers.Remove("Authorization");
        req.Headers.Add("Authorization", token);
      }

      return await base.SendAsync(req, ct);
    }
  }
}
