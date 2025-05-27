using Newtonsoft.Json;

using System.ComponentModel.DataAnnotations;

namespace automation.mbtdistr.ru.Services.Wildberries.Models
{
  /// <summary>
  /// ДТО элемента заказа Wildberries.
  /// </summary>
  public class WBOrder
  {
    /// <summary>Дата заказа.</summary>
    [Display(Name = "Дата заказа")]
    [JsonProperty("date")]
    public DateTime Date { get; set; }

    /// <summary>Дата последнего изменения заказа.</summary>
    [Display(Name = "Дата последнего изменения")]
    [JsonProperty("lastChangeDate")]
    public DateTime LastChangeDate { get; set; }

    /// <summary>Название склада.</summary>
    [Display(Name = "Название склада")]
    [JsonProperty("warehouseName")]
    public string WarehouseName { get; set; }

    /// <summary>Тип склада.</summary>
    [Display(Name = "Тип склада")]
    [JsonProperty("warehouseType")]
    public string WarehouseType { get; set; }

    /// <summary>Название страны.</summary>
    [Display(Name = "Страна")]
    [JsonProperty("countryName")]
    public string CountryName { get; set; }

    /// <summary>Название федерального округа.</summary>
    [Display(Name = "Федеральный округ")]
    [JsonProperty("oblastOkrugName")]
    public string OblastOkrugName { get; set; }

    /// <summary>Название региона.</summary>
    [Display(Name = "Регион")]
    [JsonProperty("regionName")]
    public string RegionName { get; set; }

    /// <summary>Артикул поставщика.</summary>
    [Display(Name = "Артикул поставщика")]
    [JsonProperty("supplierArticle")]
    public string SupplierArticle { get; set; }

    /// <summary>Идентификатор товара (NM ID).</summary>
    [Display(Name = "NM ID")]
    [JsonProperty("nmId")]
    public int NmId { get; set; }

    /// <summary>Штрих-код товара.</summary>
    [Display(Name = "Штрих-код")]
    [JsonProperty("barcode")]
    public string Barcode { get; set; }

    /// <summary>Категория товара.</summary>
    [Display(Name = "Категория")]
    [JsonProperty("category")]
    public string Category { get; set; }

    /// <summary>Наименование товара (subject).</summary>
    [Display(Name = "Наименование")]
    [JsonProperty("subject")]
    public string Subject { get; set; }

    /// <summary>Бренд товара.</summary>
    [Display(Name = "Бренд")]
    [JsonProperty("brand")]
    public string Brand { get; set; }

    /// <summary>Технический размер товара.</summary>
    [Display(Name = "Технический размер")]
    [JsonProperty("techSize")]
    public string TechSize { get; set; }

    /// <summary>Идентификатор поступления.</summary>
    [Display(Name = "ID поступления")]
    [JsonProperty("incomeID")]
    public long IncomeID { get; set; }

    /// <summary>Признак поставки.</summary>
    [Display(Name = "Поставка")]
    [JsonProperty("isSupply")]
    public bool IsSupply { get; set; }

    /// <summary>Признак реализации.</summary>
    [Display(Name = "Реализация")]
    [JsonProperty("isRealization")]
    public bool IsRealization { get; set; }

    /// <summary>Общая цена до скидки.</summary>
    [Display(Name = "Общая цена")]
    [JsonProperty("totalPrice")]
    public decimal TotalPrice { get; set; }

    /// <summary>Процент скидки.</summary>
    [Display(Name = "Процент скидки")]
    [JsonProperty("discountPercent")]
    public int DiscountPercent { get; set; }

    /// <summary>СПП (комиссия маркетплейса).</summary>
    [Display(Name = "СПП")]
    [JsonProperty("spp")]
    public int Spp { get; set; }

    /// <summary>Цена после вычета СПП.</summary>
    [Display(Name = "Цена после СПП")]
    [JsonProperty("finishedPrice")]
    public decimal FinishedPrice { get; set; }

    /// <summary>Цена со скидкой.</summary>
    [Display(Name = "Цена со скидкой")]
    [JsonProperty("priceWithDisc")]
    public decimal PriceWithDisc { get; set; }

    /// <summary>Признак отмены.</summary>
    [Display(Name = "Отменен")]
    [JsonProperty("isCancel")]
    public bool IsCancel { get; set; }

    /// <summary>Дата отмены.</summary>
    [Display(Name = "Дата отмены")]
    [JsonProperty("cancelDate")]
    public DateTime CancelDate { get; set; }

    /// <summary>Наклейка (sticker).</summary>
    [Display(Name = "Наклейка")]
    [JsonProperty("sticker")]
    public string Sticker { get; set; }

    /// <summary>G-номер (логистический номер).</summary>
    [Display(Name = "G-номер")]
    [JsonProperty("gNumber")]
    public string GNumber { get; set; }

    /// <summary>SRID позиции заказа.</summary>
    [Display(Name = "SRID")]
    [JsonProperty("srid")]
    [Key]
    public string Srid { get; set; }
  }
}
