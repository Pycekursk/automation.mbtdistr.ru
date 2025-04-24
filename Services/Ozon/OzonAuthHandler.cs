using automation.mbtdistr.ru.Data;
using automation.mbtdistr.ru.Services.Ozon.Models;

using Microsoft.EntityFrameworkCore;

namespace automation.mbtdistr.ru.Services.Ozon
{
  public class OzonAuthHandler : DelegatingHandler
  {
    private readonly ApplicationDbContext _db;
    private readonly ILogger<OzonAuthHandler> _logger;

    public OzonAuthHandler(ApplicationDbContext db, ILogger<OzonAuthHandler> logger)
    {
      _db = db;
      _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage req, CancellationToken ct)
    {
      try
      {
        var cabinet = await _db.Cabinets
            .Include(c => c.Settings)
                .ThenInclude(s => s.ConnectionParameters)
            .FirstOrDefaultAsync(c => c.Name == "Ozon", ct)
            .ConfigureAwait(false);

        if (cabinet?.Settings?.ConnectionParameters is null)
        {
          throw new OzonApiException("Ozon cabinet settings not found in database");
        }

        var cp = cabinet.Settings.ConnectionParameters;
        var clientId = cp.First(p => p.Key == "ClientId").Value;
        var apiKey = cp.First(p => p.Key == "ApiKey").Value;

        req.Headers.Remove("Client-Id");
        req.Headers.Remove("Api-Key");
        req.Headers.Add("Client-Id", clientId);
        req.Headers.Add("Api-Key", apiKey);

        return await base.SendAsync(req, ct).ConfigureAwait(false);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error in OzonAuthHandler");
        throw;
      }
    }
  }
}
