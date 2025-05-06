using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using automation.mbtdistr.ru.Models;

using Microsoft.EntityFrameworkCore;

/// <summary>
/// Общая информация по возврату (ReturnMainInfo).
/// </summary>
/// <summary>
/// Детальная информация о возврате (общие поля для WB и Ozon).
/// </summary>
public class ReturnMainInfo
{
  [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
  public int Id { get; set; }  // Внутренний PK для этой таблицы

  [ForeignKey(nameof(Return))]
  public int ReturnId { get; set; }  // Внешний ключ к таблице Return
  public Return Return { get; set; }

  /// <summary>
  /// Статус возврата в объединённой системе.
  /// Заполняется при конвертации из статусных полей WB/Ozon.
  /// </summary>
  public ReturnStatus ReturnStatus { get; set; } = ReturnStatus.Unknown;

  /// <summary>
  /// Название причины возврата (Источник: Ozon.ReturnReasonName).
  /// </summary>
  public string? ReturnReasonName { get; set; }

  /// <summary>
  /// ID возврата в системе Ozon (Ozon.ReturnInfo.Id).
  /// </summary>
  public long? ReturnInfoId { get; set; }

  /// <summary>
  /// ID заявки (претензии) в системе Wildberries (Claim.Id).
  /// </summary>
  public string? ClaimId { get; set; }

  /// <summary>
  /// Артикул (nm_id) товара в Wildberries.
  /// </summary>
  public long? NmId { get; set; }

  /// <summary>
  /// Артикул (SKU) товара в Ozon (Ozon.ProductInfo.Sku).
  /// </summary>
  public List<long>? ProductsSku { get; set; }

  /// <summary>
  /// ID заказа (Источник: Ozon.ReturnInfo.OrderId).
  /// </summary>
  public long? OrderId { get; set; }

  /// <summary>
  /// Номер заказа (Источник: Ozon.ReturnInfo.OrderNumber).
  /// </summary>
  public string? OrderNumber { get; set; }

  /// <summary>
  /// Тип возврата (Источник: Ozon.ReturnInfo.Type).
  /// </summary>
  public string? Type { get; set; }

  /// <summary>
  /// Схема возврата (Источник: Ozon.ReturnInfo.Schema).
  /// </summary>
  public string? Schema { get; set; }

  /// <summary>
  /// Номер отправления/накладной Ozon (Источник: Ozon.ReturnInfo.PostingNumber).
  /// </summary>
  public string? PostingNumber { get; set; }

  /// <summary>
  /// Фактическая цена товара (с учётом скидок) (Источник: WB.Claim.Price).
  /// </summary>
  public decimal? Price { get; set; }

  /// <summary>
  /// Код валюты цены (ISO-код) (Источник: WB.Claim.CurrencyCode).
  /// </summary>
  public string? CurrencyCode { get; set; }
}
/// <summary>
/// Логистический пункт выдачи (точка возврата товара).
/// </summary>
public class LogisticPickupPoint
{
  [Key]
  public long Id { get; set; } // ID пункта выдачи

  public string Name { get; set; } // Название пункта выдачи

  public string Instruction { get; set; } // Инструкция для возврата (только Яндекс)

  public string Type { get; set; } // Тип пункта (например, WAREHOUSE) (только Яндекс)

  public long? LogisticPartnerId { get; set; } // ID логистического партнера (только Яндекс)

  // Адрес пункта выдачи (вложенный объект)
  public Address Address { get; set; }
}

/// <summary>
/// Адрес (вложенный объект).
/// </summary>
[Owned]
public class Address
{
  public string Country { get; set; } // Страна
  public string City { get; set; } // Город / регион
  public string Street { get; set; } // Улица
  public string House { get; set; } // Дом
  public string Postcode { get; set; } // Почтовый индекс
}

/// <summary>
/// Информация о товаре в возврате.
/// </summary>
public class ReturnItem
{
  [Key]
  public int Id { get; set; }

  public long MarketSku { get; set; } // Артикул товара (market SKU) (только Яндекс)

  public string ShopSku { get; set; } // Магазинный артикул товара

  public int Count { get; set; } // Количество товара

  public string Status { get; set; } // Статус товара (например, PICKED) (только Яндекс)

  // Навигация на родительский возврат
  [ForeignKey(nameof(ReturnMainInfo))]
  public long ReturnMainInfoId { get; set; }
  public ReturnMainInfo ReturnMainInfo { get; set; }

  // Список треков для этого товара
  public List<ReturnTrack> Tracks { get; set; } = new List<ReturnTrack>();
}

/// <summary>
/// Трек возврата для товара.
/// </summary>
public class ReturnTrack
{
  [Key]
  public int Id { get; set; }

  public string TrackCode { get; set; } // Код отслеживания (только Яндекс)

  // Навигация на товар возврата
  [ForeignKey(nameof(ReturnItem))]
  public int ReturnItemId { get; set; }
  public ReturnItem ReturnItem { get; set; }
}
