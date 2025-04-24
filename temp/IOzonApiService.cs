using automation.mbtdistr.ru.Models;
using automation.mbtdistr.ru.Services.Ozon.Models;

namespace automation.mbtdistr.ru.temp
{
  public interface IOzonApiService
  {
    // уже существующие:
    Task<string> GetProductListAsync(Cabinet cabinet, int limit = 100, int offset = 0);

    // новые для возвратов:
    Task<string> CreateReturnReportAsync(CreateReturnReportRequest req);
    Task<ReportInfoResponse> GetReportInfoAsync(string reportId);
    Task<GiveoutListResponse> GetReturnShipmentsListAsync();
    Task<GiveoutInfoResponse> GetShipmentInfoAsync(long shipmentId);
    Task<bool> IsShipmentEnabledAsync(long shipmentId);
    Task<byte[]> GetShipmentPdfAsync(long shipmentId);
    Task<byte[]> GetShipmentPngAsync(long shipmentId);
    Task<string> ResetShipmentBarcodeAsync(long shipmentId);
    Task<ReturnsListResponse> GetReturnsListAsync(ReturnsListRequest req);
  }
}
