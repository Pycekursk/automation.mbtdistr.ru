using automation.mbtdistr.ru.Services.Wildberries.Models;
using automation.mbtdistr.ru.Services.YandexMarket.Models;

using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace automation.mbtdistr.ru.Models
{
  public class Order
  {
    [System.ComponentModel.DataAnnotations.Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public string? ExternalId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public DateTime? CanceledAt { get; set; }

    public double TotalPrice { get; set; }

    public double FinishedPrice { get; set; }

    public SellScheme SellScheme { get; set; } = SellScheme.Unknown;

    public YMReturnShipmentStatusType ShipmentStatus { get; set; } = YMReturnShipmentStatusType.Unknown;

    public YMOrderStatusType OrderStatus { get; set; } = YMOrderStatusType.UNKNOWN;

    [ForeignKey(nameof(Return))]
    public int? ReturnId { get; set; }

    [JsonIgnore, Newtonsoft.Json.JsonIgnore]
    public Return? Return { get; set; }

    [ForeignKey(nameof(Cabinet))]
    public int CabinetId { get; set; }

    [JsonIgnore, Newtonsoft.Json.JsonIgnore]
    public Cabinet Cabinet { get; set; }

    public static Order Parse<T>(object orderObject, int cabinetId)
    {
      Order order = new Order();
      var type = typeof(T);

      switch (type.Name)
      {
        case nameof(WBOrder):
          var wbObj = (WBOrder)orderObject;
          ParseWbOrder(ref order, wbObj);
          break;
        case nameof(YMOrder):
          var ymObj = (YMOrder)orderObject;
          ParseYMOrder(ref order, ymObj);

          break;

        default:
          throw new NotImplementedException($"Неизвестный тип заказа: {type.Name}");
      }
      order.CabinetId = cabinetId;
      return order;
    }


    private static void ParseWbOrder(ref Order order, WBOrder wbOrder)
    {
      order.ExternalId = wbOrder.Srid;
      order.CreatedAt = wbOrder.Date;
      order.UpdatedAt = wbOrder.LastChangeDate;
      order.TotalPrice = (double)wbOrder.TotalPrice;
      order.FinishedPrice = (double)wbOrder.FinishedPrice;
      order.SellScheme = wbOrder.WarehouseType == "Склад продавца" ? SellScheme.FBS : wbOrder.WarehouseType == "Склад WB" ? SellScheme.FBO : SellScheme.Unknown;
      order.CanceledAt = wbOrder.CancelDate == DateTime.MinValue ? null : wbOrder.CancelDate;
    }

    private static void ParseYMOrder(ref Order order, YMOrder ymOrder)
    {
      order.ExternalId = ymOrder.Id.ToString();
      order.CreatedAt = ymOrder.CreationDate;
      order.UpdatedAt = ymOrder.UpdatedAt;
      order.TotalPrice = ymOrder.BuyerItemsTotalBeforeDiscount;
      order.FinishedPrice = ymOrder.BuyerTotal;
      order.OrderStatus = ymOrder.Status;

      if (ymOrder?.Items is List<YMOrderItem> ymOrderItems && ymOrderItems.Count > 0)
        foreach (var ymOrderItem in ymOrderItems)
        {
          //TODO: Здесь необходимо добавить логику для обработки каждого продукта в заказе, а именно приведение у типу Models.Product
        }
    }
  }
}
