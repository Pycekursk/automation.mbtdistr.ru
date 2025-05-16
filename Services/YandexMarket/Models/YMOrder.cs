using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.ComponentModel.DataAnnotations.Schema;

namespace automation.mbtdistr.ru.Services.YandexMarket.Models
{
  ///// <summary>
  ///// Заказ (OrderDTO).
  ///// </summary>
  //public class YMOrder
  //{
  //  [Display(Name = "Информация о покупателе")]
  //  [JsonProperty("buyer")]
  //  [Required]
  //  public YMOrderBuyer Buyer { get; set; }

  //  [Display(Name = "Стоимость товаров до скидок")]
  //  [JsonProperty("buyerItemsTotalBeforeDiscount")]
  //  public decimal BuyerItemsTotalBeforeDiscount { get; set; }

  //  [Display(Name = "Дата оформления заказа")]
  //  [JsonProperty("creationDate")]
  //  public DateTime CreationDate { get; set; }

  //  [Display(Name = "Валюта заказа")]
  //  [JsonProperty("currency")]
  //  public YMCurrencyType Currency { get; set; }

  //  [Display(Name = "Информация о доставке")]
  //  [JsonProperty("delivery")]
  //  public YMOrderDelivery Delivery { get; set; }

  //  [Display(Name = "Стоимость доставки")]
  //  [JsonProperty("deliveryTotal")]
  //  public decimal DeliveryTotal { get; set; }

  //  [Display(Name = "Тестовый заказ")]
  //  [JsonProperty("fake")]
  //  public bool Fake { get; set; }

  //  [Display(Name = "Идентификатор заказа")]
  //  [JsonProperty("id")]
  //  public long Id { get; set; }

  //  [Display(Name = "Список товаров в заказе")]
  //  [JsonProperty("items")]
  //  [Required]
  //  public List<YMOrderItem> Items { get; set; }

  //  [Display(Name = "Платеж покупателя")]
  //  [JsonProperty("itemsTotal")]
  //  public decimal ItemsTotal { get; set; }

  //  [Display(Name = "Способ оплаты")]
  //  [JsonProperty("paymentMethod")]
  //  public YMOrderPaymentMethodType PaymentMethod { get; set; }

  //  [Display(Name = "Тип оплаты")]
  //  [JsonProperty("paymentType")]
  //  public YMOrderPaymentType PaymentType { get; set; }

  //  [Display(Name = "Статус заказа")]
  //  [JsonProperty("status")]
  //  public YMOrderStatusType Status { get; set; }

  //  [Display(Name = "Подстатус заказа")]
  //  [JsonProperty("substatus")]
  //  public YMOrderSubstatusType Substatus { get; set; }

  //  [Display(Name = "Система налогообложения")]
  //  [JsonProperty("taxSystem")]
  //  public YMOrderTaxSystemType TaxSystem { get; set; }

  //  [Display(Name = "Дата и время последнего обновления")]
  //  [JsonProperty("updatedAt")]
  //  public DateTime UpdatedAt { get; set; }
  //}

  ///// <summary>
  ///// Информация о покупателе.
  ///// </summary>
  //public class YMOrderBuyer
  //{
  //  [Display(Name = "Тип покупателя")]
  //  [JsonProperty("type")]
  //  public YMOrderBuyerType Type { get; set; }

  //  [Display(Name = "Имя")]
  //  [JsonProperty("firstName")]
  //  public string FirstName { get; set; }

  //  [Display(Name = "Фамилия")]
  //  [JsonProperty("lastName")]
  //  public string LastName { get; set; }

  //  [Display(Name = "Отчество")]
  //  [JsonProperty("middleName")]
  //  public string MiddleName { get; set; }

  //  [Display(Name = "Идентификатор покупателя")]
  //  [JsonProperty("id")]
  //  public string Id { get; set; }
  //}



  #region EntityTypeConfigurations

  public class YMOrderConfiguration : IEntityTypeConfiguration<YMOrder>
  {
    public void Configure(EntityTypeBuilder<YMOrder> builder)
    {
      builder.ToTable("YMOrders");
      builder.HasKey(o => o.Id);
      builder.Property(o => o.Id).ValueGeneratedNever();

      builder.HasOne(o => o.Buyer)
          .WithOne(b => b.Order)
          .HasForeignKey<YMOrderBuyer>(b => b.OrderId)
          .OnDelete(DeleteBehavior.Cascade);

      builder.HasOne(o => o.Delivery)
          .WithOne(d => d.Order)
          .HasForeignKey<YMOrderDelivery>(d => d.OrderId)
          .OnDelete(DeleteBehavior.Cascade);

      builder.HasMany(o => o.Items)
          .WithOne(i => i.Order)
          .HasForeignKey(i => i.OrderId)
          .OnDelete(DeleteBehavior.Cascade);
    }
  }

  public class YMOrderBuyerConfiguration : IEntityTypeConfiguration<YMOrderBuyer>
  {
    public void Configure(EntityTypeBuilder<YMOrderBuyer> builder)
    {
      builder.ToTable("YMOrderBuyers");
      builder.HasKey(b => b.OrderId);
    }
  }

  public class YMOrderDeliveryConfiguration : IEntityTypeConfiguration<YMOrderDelivery>
  {
    public void Configure(EntityTypeBuilder<YMOrderDelivery> builder)
    {
      builder.ToTable("YMOrderDeliveries");
      builder.HasKey(d => d.OrderId);

      builder.OwnsOne(d => d.Dates, dates =>
      {
        dates.Property(p => p.FromDate).HasColumnName("FromDate");
        dates.Property(p => p.ToDate).HasColumnName("ToDate");
        dates.Property(p => p.FromTime).HasColumnName("FromTime");
        dates.Property(p => p.ToTime).HasColumnName("ToTime");
        dates.Property(p => p.RealDeliveryDate).HasColumnName("RealDeliveryDate");
      });

      builder.OwnsOne(d => d.Address, address =>
      {
        address.Property(a => a.Country).HasColumnName("Country");
        address.Property(a => a.City).HasColumnName("City");
        address.Property(a => a.Street).HasColumnName("Street");
        address.Property(a => a.House).HasColumnName("House");
        address.Property(a => a.Apartment).HasColumnName("Apartment");
        address.Property(a => a.Postcode).HasColumnName("Postcode");
      });
    }
  }

  public class YMOrderItemConfiguration : IEntityTypeConfiguration<YMOrderItem>
  {
    public void Configure(EntityTypeBuilder<YMOrderItem> builder)
    {
      builder.ToTable("YMOrderItems");
      builder.HasKey(i => i.Id);
    }
  }

  #endregion




  /// <summary>
  /// Заказ (OrderDTO).
  /// </summary>
  public class YMOrder
  {
    [Key]
    [Display(Name = "Идентификатор заказа")]
    [JsonProperty("id")]
    public long Id { get; set; }

    /// <summary>
    /// Информация о покупателе (FK 1:1).
    /// </summary>
    [Required]
    [Display(Name = "Информация о покупателе")]
    [JsonProperty("buyer")]
    public YMOrderBuyer Buyer { get; set; }

    [Display(Name = "Стоимость товаров до скидок")]
    [JsonProperty("buyerItemsTotalBeforeDiscount")]
    public decimal BuyerItemsTotalBeforeDiscount { get; set; }

    [Display(Name = "Дата оформления заказа")]
    [JsonProperty("creationDate")]
    public DateTime CreationDate { get; set; }

    [Display(Name = "Валюта заказа")]
    [JsonProperty("currency")]
    public YMCurrencyType Currency { get; set; }

    /// <summary>
    /// Информация о доставке (FK 1:1).
    /// </summary>
    [Display(Name = "Информация о доставке")]
    [JsonProperty("delivery")]
    public YMOrderDelivery Delivery { get; set; }

    [Display(Name = "Стоимость доставки")]
    [JsonProperty("deliveryTotal")]
    public decimal DeliveryTotal { get; set; }

    [Display(Name = "Тестовый заказ")]
    [JsonProperty("fake")]
    public bool Fake { get; set; }

    /// <summary>
    /// Список товаров (FK 1:N).
    /// </summary>
    [Required]
    [Display(Name = "Список товаров в заказе")]
    [JsonProperty("items")]
    public List<YMOrderItem> Items { get; set; }

    [Display(Name = "Платёж покупателя")]
    [JsonProperty("itemsTotal")]
    public decimal ItemsTotal { get; set; }

    [Display(Name = "Способ оплаты")]
    [JsonProperty("paymentMethod")]
    public YMOrderPaymentMethodType PaymentMethod { get; set; }

    [Display(Name = "Тип оплаты")]
    [JsonProperty("paymentType")]
    public YMOrderPaymentType PaymentType { get; set; }

    [Display(Name = "Статус заказа")]
    [JsonProperty("status")]
    public YMOrderStatusType Status { get; set; }

    [Display(Name = "Подстатус заказа")]
    [JsonProperty("substatus")]
    public YMOrderSubstatusType Substatus { get; set; }

    [Display(Name = "Система налогообложения")]
    [JsonProperty("taxSystem")]
    public YMOrderTaxSystemType TaxSystem { get; set; }

    [Display(Name = "Обновлено в")]
    [JsonProperty("updatedAt")]
    public DateTime UpdatedAt { get; set; }
  }

  /// <summary>
  /// Информация о покупателе.
  /// </summary>
  public class YMOrderBuyer
  {
    /// <summary>
    /// FK к заказу.
    /// </summary>
    [JsonIgnore]
    public long OrderId { get; set; }

    [JsonIgnore]
    public YMOrder Order { get; set; }

    [Display(Name = "Тип покупателя")]
    [JsonProperty("type")]
    public YMOrderBuyerType Type { get; set; }

    [Display(Name = "Имя")]
    [JsonProperty("firstName")]
    [MaxLength(255)]
    public string FirstName { get; set; }

    [Display(Name = "Фамилия")]
    [JsonProperty("lastName")]
    [MaxLength(255)]
    public string LastName { get; set; }

    [Display(Name = "Отчество")]
    [JsonProperty("middleName")]
    [MaxLength(255)]
    public string MiddleName { get; set; }

    [Display(Name = "Идентификатор покупателя в Маркете")]
    [JsonProperty("id")]
    public string Id { get; set; }
  }

  /// <summary>
  /// Информация о доставке заказа.
  /// </summary>
  public class YMOrderDelivery
  {
    /// <summary>
    /// FK к заказу.
    /// </summary>
    [JsonIgnore]
    public long OrderId { get; set; }

    [JsonIgnore]
    public YMOrder Order { get; set; }

    [Display(Name = "Диапазон дат доставки")]
    [JsonProperty("dates")]
    public YMOrderDeliveryDates Dates { get; set; }

    [Display(Name = "Тип сотрудничества со службой доставки")]
    [JsonProperty("deliveryPartnerType")]
    public YMOrderDeliveryPartnerType DeliveryPartnerType { get; set; }

    [Display(Name = "Идентификатор службы доставки")]
    [JsonProperty("deliveryServiceId")]
    public long? DeliveryServiceId { get; set; }

    [Display(Name = "Способ доставки")]
    [JsonProperty("type")]
    public YMOrderDeliveryType Type { get; set; }

    [Display(Name = "Способ отгрузки")]
    [JsonProperty("dispatchType")]
    public YMOrderDeliveryDispatchType? DispatchType { get; set; }

    [Display(Name = "Адрес доставки")]
    [JsonProperty("address")]
    public YMOrderDeliveryAddress Address { get; set; }

    [Display(Name = "Стоимость подъема")]
    [JsonProperty("liftPrice")]
    public decimal? LiftPrice { get; set; }

    [Display(Name = "Тип подъема")]
    [JsonProperty("liftType")]
    public YMOrderLiftType? LiftType { get; set; }

    [Display(Name = "Ориентировочная дата доставки")]
    [JsonProperty("estimated")]
    public bool? Estimated { get; set; }

    [Display(Name = "Стоимость доставки")]
    [JsonProperty("price")]
    public decimal? Price { get; set; }
  }

  /// <summary>
  /// Диапазон дат доставки.
  /// </summary>
  public class YMOrderDeliveryDates
  {
    [Display(Name = "Дата от")]
    [JsonProperty("fromDate")]
    public DateTime FromDate { get; set; }

    [Display(Name = "Дата до")]
    [JsonProperty("toDate")]
    public DateTime ToDate { get; set; }

    [Display(Name = "Время от")]
    [JsonProperty("fromTime")]
    public TimeSpan? FromTime { get; set; }

    [Display(Name = "Время до")]
    [JsonProperty("toTime")]
    public TimeSpan? ToTime { get; set; }

    [Display(Name = "Фактическая дата доставки")]
    [JsonProperty("realDeliveryDate")]
    public DateTime? RealDeliveryDate { get; set; }
  }

  /// <summary>
  /// Адрес доставки.
  /// </summary>
  public class YMOrderDeliveryAddress
  {
    [Display(Name = "Страна")]
    [JsonProperty("country")]
    public string Country { get; set; }

    [Display(Name = "Регион")]
    [JsonProperty("city")]
    public string City { get; set; }

    [Display(Name = "Улица")]
    [JsonProperty("street")]
    public string Street { get; set; }

    [Display(Name = "Дом")]
    [JsonProperty("house")]
    public string House { get; set; }

    [Display(Name = "Квартира/офис")]
    [JsonProperty("apartment")]
    public string Apartment { get; set; }

    [Display(Name = "Почтовый индекс")]
    [JsonProperty("postcode")]
    public string Postcode { get; set; }
  }

  /// <summary>
  /// Товар в заказе.
  /// </summary>
  public class YMOrderItem
  {
    public long OrderId { get; set; }

    [JsonIgnore]
    public YMOrder Order { get; set; }

    [Display(Name = "Идентификатор товара в заказе")]
    [JsonProperty("id")]
    public long Id { get; set; }

    [Display(Name = "Идентификатор оффера")]
    [JsonProperty("offerId")]
    [Required]
    [MaxLength(255)]
    public string OfferId { get; set; }

    [Display(Name = "Название товара")]
    [JsonProperty("offerName")]
    public string OfferName { get; set; }

    [Display(Name = "Количество")]
    [JsonProperty("count")]
    [Range(1, int.MaxValue)]
    public int Count { get; set; }

    [Display(Name = "Цена")]
    [JsonProperty("price")]
    public decimal Price { get; set; }

    [Display(Name = "НДС")]
    [JsonProperty("vat")]
    public YMOrderVatType Vat { get; set; }
  }

  /// <summary>
  /// Тип покупателя.
  /// </summary>
  [JsonConverter(typeof(StringEnumConverter))]
  public enum YMOrderBuyerType
  {
    /// <summary>Физическое лицо.</summary>
    [Display(Name = "Физическое лицо")]
    [EnumMember(Value = "PERSON")]
    Person,

    /// <summary>Юридическое лицо.</summary>
    [Display(Name = "Юридическое лицо")]
    [EnumMember(Value = "BUSINESS")]
    Business,



    [Display(Name = "Неизвестно")]
    [EnumMember(Value = "UNKNOWN")]
    Unknown = 0
  }


  /// <summary>
  /// Тип оплаты заказа.
  /// </summary>
  [JsonConverter(typeof(StringEnumConverter))]
  public enum YMOrderPaymentType
  {
    /// <summary>Предоплата.</summary>
    [Display(Name = "Предоплата")]
    [EnumMember(Value = "PREPAID")]
    Prepaid,

    /// <summary>Постоплата.</summary>
    [Display(Name = "Постоплата")]
    [EnumMember(Value = "POSTPAID")]
    Postpaid,

    /// <summary>Неизвестно.</summary>
    [Display(Name = "Неизвестно")]
    [EnumMember(Value = "UNKNOWN")]
    Unknown = 0
  }

  /// <summary>
  /// Система налогообложения (СНО).
  /// </summary>
  [JsonConverter(typeof(StringEnumConverter))]
  public enum YMOrderTaxSystemType
  {
    /// <summary>Общая система налогообложения.</summary>
    [Display(Name = "ОСН")]
    [EnumMember(Value = "OSN")]
    Osn,

    /// <summary>УСН «доходы».</summary>
    [Display(Name = "УСН «доходы»")]
    [EnumMember(Value = "USN")]
    Usn,

    /// <summary>УСН «доходы минус расходы».</summary>
    [Display(Name = "УСН «доходы-расходы»")]
    [EnumMember(Value = "USN_MINUS_COST")]
    UsnMinusCost,

    /// <summary>ЕНВД.</summary>
    [Display(Name = "ЕНВД")]
    [EnumMember(Value = "ENVD")]
    Envd,

    /// <summary>ЕСХН.</summary>
    [Display(Name = "ЕСХН")]
    [EnumMember(Value = "ECHN")]
    Echn,

    /// <summary>Патентная система.</summary>
    [Display(Name = "ПСН")]
    [EnumMember(Value = "PSN")]
    Psn,

    /// <summary>Налог на профессиональный доход.</summary>
    [Display(Name = "НПД")]
    [EnumMember(Value = "NPD")]
    Npd,

    /// <summary>Неизвестное значение.</summary>
    [Display(Name = "Неизвестно")]
    [EnumMember(Value = "UNKNOWN_VALUE")]
    UnknownValue = 0
  }

  /// <summary>
  /// Тип сотрудничества со службой доставки.
  /// </summary>
  [JsonConverter(typeof(StringEnumConverter))]
  public enum YMOrderDeliveryPartnerType
  {
    /// <summary>Доставка силами магазина.</summary>
    [Display(Name = "Магазин")]
    [EnumMember(Value = "SHOP")]
    Shop,

    /// <summary>Доставка через Яндекс Маркет.</summary>
    [Display(Name = "Яндекс Маркет")]
    [EnumMember(Value = "YANDEX_MARKET")]
    YandexMarket,

    /// <summary>Неизвестно.</summary>
    [Display(Name = "Неизвестно")]
    [EnumMember(Value = "UNKNOWN")]
    Unknown = 0
  }

  /// <summary>
  /// Способ доставки.
  /// </summary>
  [JsonConverter(typeof(StringEnumConverter))]
  public enum YMOrderDeliveryType
  {
    /// <summary>Курьерская доставка.</summary>
    [Display(Name = "Курьерская доставка")]
    [EnumMember(Value = "DELIVERY")]
    Delivery,

    /// <summary>Самовывоз.</summary>
    [Display(Name = "Самовывоз")]
    [EnumMember(Value = "PICKUP")]
    Pickup,

    /// <summary>Доставка почтой.</summary>
    [Display(Name = "Почта")]
    [EnumMember(Value = "POST")]
    Post,

    /// <summary>Цифровая доставка (e-goods).</summary>
    [Display(Name = "Цифровая доставка")]
    [EnumMember(Value = "DIGITAL")]
    Digital,

    /// <summary>Неизвестно.</summary>
    [Display(Name = "Неизвестно")]
    [EnumMember(Value = "UNKNOWN")]
    Unknown = 0
  }

  /// <summary>
  /// Тип подъёма на этаж.
  /// </summary>
  [JsonConverter(typeof(StringEnumConverter))]
  public enum YMOrderLiftType
  {
    /// <summary>Подъём не требуется.</summary>
    [Display(Name = "Не требуется")]
    [EnumMember(Value = "NOT_NEEDED")]
    NotNeeded,

    /// <summary>Подъём вручную.</summary>
    [Display(Name = "Ручной подъём")]
    [EnumMember(Value = "MANUAL")]
    Manual,

    /// <summary>Подъём на пассажирском лифте.</summary>
    [Display(Name = "Пассажирский лифт")]
    [EnumMember(Value = "ELEVATOR")]
    Elevator,

    /// <summary>Подъём на грузовом лифте.</summary>
    [Display(Name = "Грузовой лифт")]
    [EnumMember(Value = "CARGO_ELEVATOR")]
    CargoElevator,

    /// <summary>Подъём бесплатный.</summary>
    [Display(Name = "Бесплатно")]
    [EnumMember(Value = "FREE")]
    Free,

    /// <summary>Неизвестно.</summary>
    [Display(Name = "Неизвестно")]
    [EnumMember(Value = "UNKNOWN")]
    Unknown = 0
  }

  /// <summary>
  /// Тип кода подтверждения ЭАПП.
  /// </summary>
  [JsonConverter(typeof(StringEnumConverter))]
  public enum YMOrderDeliveryEacType
  {
    /// <summary>Код, который продавец сообщает курьеру.</summary>
    [Display(Name = "От продавца курьеру")]
    [EnumMember(Value = "MERCHANT_TO_COURIER")]
    MerchantToCourier,

    /// <summary>Код, который курьер сообщает продавцу.</summary>
    [Display(Name = "От курьера продавцу")]
    [EnumMember(Value = "COURIER_TO_MERCHANT")]
    CourierToMerchant,

    /// <summary>Проверка кода продавцом.</summary>
    [Display(Name = "Проверка продавцом")]
    [EnumMember(Value = "CHECKING_BY_MERCHANT")]
    CheckingByMerchant,

    [Display(Name = "Неизвестно")]
    [EnumMember(Value = "UNKNOWN")]
    Unknown = 0
  }

  /// <summary>
  /// Тип региона.
  /// </summary>
  [JsonConverter(typeof(StringEnumConverter))]
  public enum YMRegionType
  {
    /// <summary>Другое.</summary>
    [Display(Name = "Другое")]
    [EnumMember(Value = "OTHER")]
    Other,

    /// <summary>Континент.</summary>
    [Display(Name = "Континент")]
    [EnumMember(Value = "CONTINENT")]
    Continent,

    /// <summary>Регион / область.</summary>
    [Display(Name = "Регион")]
    [EnumMember(Value = "REGION")]
    Region,

    /// <summary>Страна.</summary>
    [Display(Name = "Страна")]
    [EnumMember(Value = "COUNTRY")]
    Country,

    /// <summary>Округ внутри страны.</summary>
    [Display(Name = "Округ страны")]
    [EnumMember(Value = "COUNTRY_DISTRICT")]
    CountryDistrict,

    /// <summary>Республика.</summary>
    [Display(Name = "Республика")]
    [EnumMember(Value = "REPUBLIC")]
    Republic,

    /// <summary>Город.</summary>
    [Display(Name = "Город")]
    [EnumMember(Value = "CITY")]
    City,

    /// <summary>Посёлок / село.</summary>
    [Display(Name = "Населённый пункт")]
    [EnumMember(Value = "VILLAGE")]
    Village,

    /// <summary>Район города.</summary>
    [Display(Name = "Район города")]
    [EnumMember(Value = "CITY_DISTRICT")]
    CityDistrict,

    /// <summary>Станция метро.</summary>
    [Display(Name = "Станция метро")]
    [EnumMember(Value = "SUBWAY_STATION")]
    SubwayStation,

    /// <summary>Район республики.</summary>
    [Display(Name = "Район республики")]
    [EnumMember(Value = "REPUBLIC_AREA")]
    RepublicArea,

    [Display(Name = "Неизвестно")]
    [EnumMember(Value = "UNKNOWN")]
    Unknown = 0
  }

  /// <summary>
  /// Ставка НДС.
  /// </summary>
  [JsonConverter(typeof(StringEnumConverter))]
  public enum YMOrderVatType
  {
    /// <summary>Без НДС.</summary>
    [Display(Name = "Без НДС")]
    [EnumMember(Value = "NO_VAT")]
    NoVat,

    /// <summary>НДС 0 %.</summary>
    [Display(Name = "НДС 0 %")]
    [EnumMember(Value = "VAT_0")]
    Vat0,

    /// <summary>НДС 10 %.</summary>
    [Display(Name = "НДС 10 %")]
    [EnumMember(Value = "VAT_10")]
    Vat10,

    /// <summary>НДС 10 / 110.</summary>
    [Display(Name = "НДС 10 / 110")]
    [EnumMember(Value = "VAT_10_110")]
    Vat10_110,

    /// <summary>НДС 20 %.</summary>
    [Display(Name = "НДС 20 %")]
    [EnumMember(Value = "VAT_20")]
    Vat20,

    /// <summary>НДС 20 / 120.</summary>
    [Display(Name = "НДС 20 / 120")]
    [EnumMember(Value = "VAT_20_120")]
    Vat20_120,

    /// <summary>Неизвестная ставка.</summary>
    [Display(Name = "Неизвестно")]
    [EnumMember(Value = "UNKNOWN_VALUE")]
    UnknownValue = 0
  }

  /// <summary>
  /// Тип маркировки единицы товара.
  /// </summary>
  [JsonConverter(typeof(StringEnumConverter))]
  public enum YMOrderItemInstanceType
  {
    /// <summary>Честный ЗНАК (CIS).</summary>
    [Display(Name = "Код Честный ЗНАК (CIS)")]
    [EnumMember(Value = "CIS")]
    Cis,

    /// <summary>Необязательный CIS.</summary>
    [Display(Name = "CIS (опционально)")]
    [EnumMember(Value = "CIS_OPTIONAL")]
    CisOptional,

    /// <summary>Unique Identification Number.</summary>
    [Display(Name = "UIN")]
    [EnumMember(Value = "UIN")]
    Uin,

    /// <summary>Регистрационный номер партии товара (РНПТ).</summary>
    [Display(Name = "РНПТ")]
    [EnumMember(Value = "RNPT")]
    Rnpt,

    /// <summary>ГТД / таможенная декларация.</summary>
    [Display(Name = "ГТД")]
    [EnumMember(Value = "GTD")]
    Gtd,

    [Display(Name = "Неизвестно")]
    [EnumMember(Value = "UNKNOWN")]
    Unknown = 0
  }
}