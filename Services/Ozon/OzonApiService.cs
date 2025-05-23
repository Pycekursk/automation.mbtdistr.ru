namespace automation.mbtdistr.ru.Services.Ozon
{
  using automation.mbtdistr.ru.Models;
  using automation.mbtdistr.ru.Services;
  using automation.mbtdistr.ru.Services.Ozon.Models;

  using Newtonsoft.Json;

  using System.Linq;
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
      if (filter == null)
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

    /// <summary>
    /// Получает список идентификаторов предложений (offer_id) по фильтру.
    /// </summary>
    /// <remarks>
    /// Фильтр можно строить по offer_id, product_id и видимости. 
    /// Документация: метод /v3/product/list :contentReference[oaicite:0]{index=0}.
    /// </remarks>
    public async Task<IReadOnlyList<string>> GetOfferIdsAsync(
        Cabinet cabinet,
        IEnumerable<string>? offerIds = null,
        IEnumerable<long>? productIds = null,
        string visibility = "ALL",
        int limit = 1000,
        string lastId = "")
    {
      var filter = new Dictionary<string, object> { ["visibility"] = visibility };
      if (offerIds != null) filter["offer_id"] = offerIds;
      if (productIds != null) filter["product_id"] = productIds;

      var body = new { filter, last_id = lastId, limit };
      var response = await _ozonSellerApiHttpClient.SendRequestAsync(MarketApiRequestType.ProductList, cabinet, body);
      var json = await response.Content.ReadAsStringAsync();
      var wrapper = JsonConvert.DeserializeObject<ProductListResponse>(json)!;
      return wrapper.Result.Items.Select(i => i.OfferId).ToList();
    }

    /// <summary>
    /// Получает информацию о товарах: название и первый штрихкод по списку offer_id.
    /// </summary>
    /// <remarks>
    /// Возвращает объекты OZProduct с заполненными полями OfferId, Name и Barcode.
    /// Документация: метод /v3/product/info/list :contentReference[oaicite:1]{index=1}.
    /// </remarks>
    public async Task<List<Product>> GetProductsInfoAsync(
        Cabinet cabinet,
        IEnumerable<string> offerIds,
        int limit = 1000,
        string lastId = "")
    {
      var body = new
      {
        offer_id = offerIds,
        last_id = lastId,
        limit
      };
      var response = await _ozonSellerApiHttpClient.SendRequestAsync(MarketApiRequestType.ProductInfo, cabinet, body);
      var json = await response.Content.ReadAsStringAsync();
      var wrapper = JsonConvert.DeserializeObject<ProductInfoResult>(json)!;
      return wrapper.Items
               .Select(item => new Product
               {
                 OfferId = item.OfferId,
                 Name = item.Name,
                 Barcodes = item.Barcodes != null
                       ? item.Barcodes.Select(b => new ProductBarcode
                       {
                         Barcode = b
                       }).ToList()
                       : new List<ProductBarcode>()
               })
               .ToList();
    }

    #region ProductInfoDTOs

    private class ProductListResponse
    {
      [JsonProperty("result")]
      public ProductListResult Result { get; set; } = default!;
    }

    private class ProductInfoItem
    {
      [JsonProperty("offer_id")]
      public string OfferId { get; set; } = default!;

      [JsonProperty("name")]
      public string Name { get; set; } = default!;

      [JsonProperty("barcodes")]
      public List<string>? Barcodes { get; set; }
    }

    private class ProductInfoResult
    {
      [JsonProperty("items")]
      public List<ProductInfoItem> Items { get; set; } = new();
    }

    private class ProductInfoResponse
    {
      [JsonProperty("result")]
      public ProductInfoResult Result { get; set; } = default!;
    }

    private class ProductListResult
    {
      [JsonProperty("items")]
      public List<ProductListItem> Items { get; set; } = new();
    }
    private class ProductListItem
    {
      [JsonProperty("offer_id")]
      public string OfferId { get; set; } = default!;
    }
    #endregion
  }
}