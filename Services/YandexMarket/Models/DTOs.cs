using System.Text.Json.Serialization;

namespace automation.mbtdistr.ru.Services.YandexMarket.Models
{
  public class DTOs
  {
    public class ReturnsListResponse
    {
      [JsonPropertyName("status")]
      public string Status { get; set; } = string.Empty;
      [JsonPropertyName("result")]
      public ReturnsListResult Result { get; set; } = new();
    }

    public class ReturnsListResult
    {
      [JsonPropertyName("paging")]
      public Paging Paging { get; set; } = new();
      [JsonPropertyName("returns")]
      public List<Return> Returns { get; set; } = new();
    }

    public class Paging
    {
      [JsonPropertyName("total")]
      public int Total { get; set; }
      [JsonPropertyName("from")]
      public int From { get; set; }
      [JsonPropertyName("to")]
      public int To { get; set; }
    }

    public class Return
    {
      [JsonPropertyName("id")]
      public long Id { get; set; }
      [JsonPropertyName("orderId")]
      public long OrderId { get; set; }
      [JsonPropertyName("creationDate")]
      public DateTime CreationDate { get; set; }
      [JsonPropertyName("updateDate")]
      public DateTime UpdateDate { get; set; }
      [JsonPropertyName("logisticPickupPoint")]
      public LogisticPickupPoint LogisticPickupPoint { get; set; } = new();
      [JsonPropertyName("shipmentRecipientType")]
      public string ShipmentRecipientType { get; set; } = string.Empty;
      [JsonPropertyName("shipmentStatus")]
      public string ShipmentStatus { get; set; } = string.Empty;
      [JsonPropertyName("refundAmount")]
      public int RefundAmount { get; set; }
      [JsonPropertyName("amount")]
      public Amount Amount { get; set; } = new();
      [JsonPropertyName("items")]
      public List<Item> Items { get; set; } = new();
    }

    public class LogisticPickupPoint
    {
      [JsonPropertyName("id")]
      public long Id { get; set; }
      [JsonPropertyName("name")]
      public string Name { get; set; } = string.Empty;
      [JsonPropertyName("address")]
      public Address Address { get; set; } = new();
      [JsonPropertyName("instruction")]
      public string Instruction { get; set; } = string.Empty;
      [JsonPropertyName("type")]
      public string Type { get; set; } = string.Empty;
      [JsonPropertyName("logisticPartnerId")]
      public long LogisticPartnerId { get; set; }
    }

    public class Address
    {
      [JsonPropertyName("country")]
      public string Country { get; set; } = string.Empty;
      [JsonPropertyName("city")]
      public string City { get; set; } = string.Empty;
      [JsonPropertyName("street")]
      public string Street { get; set; } = string.Empty;
      [JsonPropertyName("house")]
      public string House { get; set; } = string.Empty;
      [JsonPropertyName("postcode")]
      public string Postcode { get; set; } = string.Empty;
    }

    public class Amount
    {
      [JsonPropertyName("value")]
      public decimal Value { get; set; }
      [JsonPropertyName("currencyId")]
      public string CurrencyId { get; set; }
    }

    public class Item
    {
      [JsonPropertyName("marketSku")]
      public long MarketSku { get; set; }
      [JsonPropertyName("shopSku")]
      public string ShopSku { get; set; } = string.Empty;
      [JsonPropertyName("count")]
      public int Count { get; set; }
      [JsonPropertyName("instances")]
      public List<Instance> Instances { get; set; } = new();
      [JsonPropertyName("tracks")]
      public List<Track> Tracks { get; set; } = new();
    }

    public class Instance
    {
      [JsonPropertyName("status")]
      public string Status { get; set; } = string.Empty;
    }

    public class Track
    {
      [JsonPropertyName("trackCode")]
      public string TrackCode { get; set; } = string.Empty;
    }
    public class ReturnsListRequest
    {
      [JsonPropertyName("filter")]
      public Filter Filter { get; set; } = new();
      [JsonPropertyName("limit")]
      public int Limit { get; set; }
      [JsonPropertyName("lastId")]
      public long? LastId { get; set; }
    }

    public class Filter
    {
      [JsonPropertyName("logisticReturnDate")]
      public DateRange LogisticReturnDate { get; set; } = new();
      [JsonPropertyName("shipmentStatus")]
      public string ShipmentStatus { get; set; } = string.Empty;
      [JsonPropertyName("returnType")]
      public ReturnType ReturnType { get; set; }
    }

    public enum ReturnType
    {
      UNREDEEMED,
      RETURN
    }

    //статус возврата денег
    public enum RefundStatusType
    {
      STARTED_BY_USER,
      REFUND_IN_PROGRESS,
      REFUNDED,
      FAILED,
      WAITING_FOR_DECISION,
      DECISION_MADE,
      REFUNDED_WITH_BONUSES,
      REFUNDED_BY_SHOP,
      COMPLETE_WITHOUT_REFUND,
      CANCELLED,
      UNKNOWN
    }

    //CurrencyType Enum: RUR, USD, EUR, UAH, AUD, GBP, BYR, BYN, DKK, ISK, KZT, CAD, CNY, NOK, XDR, SGD, TRY, SEK, CHF, JPY, AZN, ALL, DZD, AOA, ARS, AMD, AFN, BHD, BGN, BOB, BWP, BND, BRL, BIF, HUF, VEF, KPW, VND, GMD, GHS, GNF, HKD, GEL, AED, EGP, ZMK, ILS, INR, IDR, JOD, IQD, IRR, YER, QAR, KES, KGS, COP, CDF, CRC, KWD, CUP, LAK, LVL, SLL, LBP, LYD, SZL, LTL, MUR, MRO, MKD, MWK, MGA, MYR, MAD, MXN, MZN, MDL, MNT, NPR, NGN, NIO, NZD, OMR, PKR, PYG, PEN, PLN, KHR, SAR, RON, SCR, SYP, SKK, SOS, SDG, SRD, TJS, THB, TWD, BDT, TZS, TND, TMM, UGX, UZS, UYU, PHP, DJF, XAF, XOF, HRK, CZK, CLP, LKR, EEK, ETB, RSD, ZAR, KRW, NAD, TL, UE

    public enum CurrencyType
    {
      RUR,
      USD,
      EUR,
      UAH,
      AUD,
      GBP,
      BYR,
      BYN,
      DKK,
      ISK,
      KZT,
      CAD,
      CNY,
      NOK,
      XDR,
      SGD,
      TRY,
      SEK,
      CHF,
      JPY,
      AZN,
    }

    public class DateRange
    {
      [JsonPropertyName("from")]
      public string From { get; set; }
      [JsonPropertyName("to")]
      public string To { get; set; }
    }

    public class CampaignsResponse
    {
      [JsonPropertyName("campaigns")]
      public List<Campaign> Campaigns { get; set; } = new();
      [JsonPropertyName("pager")]
      public Pager Pager { get; set; } = new();
    }
    public class Campaign
    {
      [JsonPropertyName("domain")]
      public string Domain { get; set; } = string.Empty;
      [JsonPropertyName("id")]
      public long Id { get; set; }
      [JsonPropertyName("clientId")]
      public long ClientId { get; set; }
      [JsonPropertyName("business")]
      public Business Business { get; set; } = new();
      [JsonPropertyName("placementType")]
      public string PlacementType { get; set; } = string.Empty;
    }
    public class Business
    {
      [JsonPropertyName("id")]
      public long Id { get; set; }
      [JsonPropertyName("name")]
      public string Name { get; set; } = string.Empty;
    }
    public class Pager
    {
      [JsonPropertyName("total")]
      public int Total { get; set; }
      [JsonPropertyName("from")]
      public int From { get; set; }
      [JsonPropertyName("to")]
      public int To { get; set; }
      [JsonPropertyName("currentPage")]
      public int CurrentPage { get; set; }
      [JsonPropertyName("pagesCount")]
      public int PagesCount { get; set; }
      [JsonPropertyName("pageSize")]
      public int PageSize { get; set; }
    }
  }
}
