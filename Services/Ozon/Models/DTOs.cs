using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

using JsonConverter = Newtonsoft.Json.JsonConverter;
using JsonConverterAttribute = Newtonsoft.Json.JsonConverterAttribute;

namespace automation.mbtdistr.ru.Services.Ozon.Models
{
  /// <summary>  
  /// Запрос для проверки уведомлений Ozon.  
  /// </summary>  
  public class OzonNotify
  {
    /// <summary>  
    /// Тип сообщения, отправленного Ozon.  
    /// </summary>  
    [JsonPropertyName("message_type")]
    public OzonNotifyType? MessageType { get; set; }

    [JsonPropertyName("posting_number")]
    public string? PostingNumber { get; set; }

    [JsonPropertyName("products")]
    public List<object>? Products { get; set; }
    [JsonPropertyName("old_state")]
    public NotifyPostingStatus? OldState { get; set; }

    [JsonPropertyName("new_state")]
    public NotifyPostingStatus? NewState { get; set; }

    [JsonPropertyName("changed_state_date")]
    public DateTime? ChangedStateDate { get; set; }

    [JsonPropertyName("reason")]
    public Reason? Reason { get; set; }

    [JsonPropertyName("warehouse_id")]
    public long? WarehouseId { get; set; }

    [JsonPropertyName("seller_id")]
    public long? SellerId { get; set; }

    [JsonPropertyName("in_process_at")]
    public DateTime? InProcessAt { get; set; }

    [JsonPropertyName("new_cutoff_date")]
    public DateTime? NewCutoffDate { get; set; }

    [JsonPropertyName("old_cutoff_date")]
    public DateTime? OldCutoffDate { get; set; }

    /// <summary>  
    /// Время отправки сообщения.  
    /// </summary>  
    [JsonPropertyName("time")]
    public DateTime Time { get; set; }

    [JsonPropertyName("new_delivery_date_begin")]
    public DateTime? NewDeliveryDateBegin { get; set; }

    [JsonPropertyName("new_delivery_date_end")]
    public DateTime? NewDeliveryDateEnd { get; set; }

    [JsonPropertyName("old_delivery_date_begin")]
    public DateTime? OldDeliveryDateBegin { get; set; }

    [JsonPropertyName("old_delivery_date_end")]
    public DateTime? OldDeliveryDateEnd { get; set; }

    [JsonPropertyName("offer_id")]
    public string? OfferId { get; set; }

    [JsonPropertyName("product_id")]
    public int ProductId { get; set; }

    [JsonPropertyName("is_error")]
    public bool IsError { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    [JsonPropertyName("sku")]
    public int Sku { get; set; }

    [JsonPropertyName("price_index")]
    public int PriceIndex { get; set; }

    [JsonPropertyName("changed_at")]
    public DateTime? ChangedAt { get; set; }

    [JsonPropertyName("user")]
    public User? User { get; set; }

    [JsonPropertyName("data")]
    public List<string>? Data { get; set; }


  }
  /// <summary>
  /// Информация о пользователе, отправившем уведомление.
  /// </summary>
  public class User
  {
    /// <summary>
    /// Идентификатор пользователя в системе Ozon.
    /// </summary>
    [Display(Name = "Идентификатор пользователя")]
    [JsonProperty("id")]
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    /// <summary>
    /// Тип пользователя в системе.
    /// </summary>
    [Display(Name = "Тип пользователя")]
    [JsonProperty("type")]
    [JsonPropertyName("type")]
    public UserType Type { get; set; }
  }

  /// <summary>
  /// Причина отмены отправления.
  /// </summary>
  public class Reason
  {
    /// <summary>
    /// Идентификатор причины отмены.
    /// </summary>
    [Display(Name = "Идентификатор причины отмены")]
    [JsonProperty("id")]
    [JsonPropertyName("id")]
    public int Id { get; set; }

    /// <summary>
    /// Сообщение об ошибке.
    /// </summary>
    [Display(Name = "Сообщение об ошибке")]
    [JsonProperty("message")]
    [JsonPropertyName("message")]
    public string? Message { get; set; }
  }

  /// <summary>  
  /// Ответ на проверку уведомлений Ozon.  
  /// </summary>  
  public class OzonCheckResponse
  {
    /// <summary>  
    /// Версия API.  
    /// </summary>  
    [JsonPropertyName("version")]
    public string? Version { get; set; }

    /// <summary>  
    /// Имя сервиса, отправившего ответ.  
    /// </summary>  
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>  
    /// Время формирования ответа.  
    /// </summary>  
    [JsonPropertyName("time")]
    public DateTime Time { get; set; }
  }

  /// <summary>  
  /// Ошибка, возвращаемая при пинге Ozon.  
  /// </summary>  
  public class OzonPingError
  {
    /// <summary>  
    /// Детали ошибки.  
    /// </summary>  
    [JsonPropertyName("error")]
    public OzonError? Error { get; set; }

    /// <summary>  
    /// Модель описания ошибки.  
    /// </summary>  
    public class OzonError
    {
      /// <summary>  
      /// Код ошибки.  
      /// </summary>  
      [JsonPropertyName("code")]
      public string? Code { get; set; }

      /// <summary>  
      /// Сообщение об ошибке.  
      /// </summary>  
      [JsonPropertyName("message")]
      public string? Message { get; set; }

      /// <summary>  
      /// Дополнительные детали ошибки.  
      /// </summary>  
      [JsonPropertyName("details")]
      public string? Details { get; set; }
    }
  }

  /// <summary>  
  /// Типы уведомлений, отправляемых Ozon.  
  /// </summary>  
  public enum OzonNotifyType
  {
    /// <summary>  
    /// Проверка статуса готовности сервиса.  
    /// </summary>  
    [Display(Name = "Пинг")]
    TYPE_PING,

    /// <summary>  
    /// Новое отправление.  
    /// </summary>  
    [Display(Name = "Новое отправление")]
    TYPE_NEW_POSTING,

    /// <summary>  
    /// Отмена отправления.  
    /// </summary>  
    [Display(Name = "Отмена отправления")]
    TYPE_POSTING_CANCELLED,

    /// <summary>  
    /// Изменение статуса отправления.  
    /// </summary>  
    [Display(Name = "Изменение статуса отправления")]
    TYPE_STATE_CHANGED,

    /// <summary>  
    /// Изменение даты отгрузки отправления.  
    /// </summary>  
    [Display(Name = "Изменение даты отгрузки")]
    TYPE_CUTOFF_DATE_CHANGED,

    /// <summary>  
    /// Изменение даты доставки отправления.  
    /// </summary>  
    [Display(Name = "Изменение даты доставки")]
    TYPE_DELIVERY_DATE_CHANGED,

    /// <summary>  
    /// Создание или обновление товара.  
    /// </summary>  
    [Display(Name = "Создание или обновление товара")]
    TYPE_CREATE_OR_UPDATE_ITEM,

    /// <summary>  
    /// Создание товара.  
    /// </summary>  
    [Display(Name = "Создание товара")]
    TYPE_CREATE_ITEM,

    /// <summary>  
    /// Обновление товара.  
    /// </summary>  
    [Display(Name = "Обновление товара")]
    TYPE_UPDATE_ITEM,

    /// <summary>  
    /// Изменение ценового индекса товара.  
    /// </summary>  
    [Display(Name = "Изменение ценового индекса")]
    TYPE_PRICE_INDEX_CHANGED,

    /// <summary>  
    /// Изменение остатков на складах продавца.  
    /// </summary>  
    [Display(Name = "Изменение остатков")]
    TYPE_STOCKS_CHANGED,

    /// <summary>  
    /// Новое сообщение в чате.  
    /// </summary>  
    [Display(Name = "Новое сообщение")]
    TYPE_NEW_MESSAGE,

    /// <summary>  
    /// Изменение сообщения в чате.  
    /// </summary>  
    [Display(Name = "Изменение сообщения")]
    TYPE_UPDATE_MESSAGE,

    /// <summary>  
    /// Сообщение прочитано.  
    /// </summary>  
    [Display(Name = "Сообщение прочитано")]
    TYPE_MESSAGE_READ,

    /// <summary>  
    /// Чат закрыт.  
    /// </summary>  
    [Display(Name = "Чат закрыт")]
    TYPE_CHAT_CLOSED
  }

  /// <summary>  
  /// Статусы отправлений, уведомления о которых отправляет Ozon.  
  /// </summary>  
  public enum NotifyPostingStatus
  {
    /// <summary>  
    /// Идёт приёмка.  
    /// </summary>  
    [Display(Name = "Идёт приёмка")]
    posting_acceptance_in_progress,

    /// <summary>  
    /// Создано.  
    /// </summary>  
    [Display(Name = "Создано")]
    posting_created,

    /// <summary>  
    /// Передаётся в доставку.  
    /// </summary>  
    [Display(Name = "Передаётся в доставку")]
    posting_transferring_to_delivery,

    /// <summary>  
    /// В перевозке.  
    /// </summary>  
    [Display(Name = "В перевозке")]
    posting_in_carriage,

    /// <summary>  
    /// Не добавлен в перевозку.  
    /// </summary>  
    [Display(Name = "Не добавлен в перевозку")]
    posting_not_in_carriage,

    /// <summary>  
    /// Клиентский арбитраж доставки.  
    /// </summary>  
    [Display(Name = "Клиентский арбитраж доставки")]
    posting_in_client_arbitration,

    /// <summary>  
    /// На пути в город.  
    /// </summary>  
    [Display(Name = "На пути в город")]
    posting_on_way_to_city,

    /// <summary>  
    /// Передаётся курьеру.  
    /// </summary>  
    [Display(Name = "Передаётся курьеру")]
    posting_transferred_to_courier_service,

    /// <summary>  
    /// Курьер в пути.  
    /// </summary>  
    [Display(Name = "Курьер в пути")]
    posting_in_courier_service,

    /// <summary>  
    /// На пути в пункт выдачи.  
    /// </summary>  
    [Display(Name = "На пути в пункт выдачи")]
    posting_on_way_to_pickup_point,

    /// <summary>  
    /// В пункте выдачи.  
    /// </summary>  
    [Display(Name = "В пункте выдачи")]
    posting_in_pickup_point,

    /// <summary>  
    /// Условно доставлено.  
    /// </summary>  
    [Display(Name = "Условно доставлено")]
    posting_conditionally_delivered,

    /// <summary>  
    /// У водителя.  
    /// </summary>  
    [Display(Name = "У водителя")]
    posting_driver_pick_up,

    /// <summary>  
    /// Не принят на сортировочном центре.  
    /// </summary>  
    [Display(Name = "Не принят на сортировочном центре")]
    posting_not_in_sort_center,

    /// <summary>
    /// Доставлено.
    /// </summary>
    [Display(Name = "Доставлено")]
    posting_delivered,

    /// <summary>
    ///  Получено
    /// </summary>
    [Display(Name = "Получено"), EnumMember(Value = "posting_received")]
    posting_received,

    /// <summary>
    /// Отменено
    /// </summary>
    [Display(Name = "Отменено"), EnumMember(Value = "posting_cancelled")]
    posting_cancelled
  }

  /// <summary>
  /// Тип пользователя в системе.
  /// </summary>
  [JsonConverter(typeof(StringEnumConverter))]
  public enum UserType
  {
    /// <summary>
    /// Неизвестный тип пользователя.
    /// </summary>
    [EnumMember(Value = "Unknown")]
    [Display(Name = "Неизвестный")]
    Unknown = 0,

    /// <summary>
    /// Покупатель.
    /// </summary>
    [EnumMember(Value = "Customer")]
    [Display(Name = "Покупатель")]
    Customer,

    /// <summary>
    /// Поддержка.
    /// </summary>
    [EnumMember(Value = "Support")]
    [Display(Name = "Поддержка")]
    Support,

    /// <summary>
    /// Ozon.
    /// </summary>
    [EnumMember(Value = "NotificationUser")]
    [Display(Name = "Ozon")]
    NotificationUser,

    /// <summary>
    /// Чат-бот.
    /// </summary>
    [EnumMember(Value = "ChatBot"), Display(Name = "Бот")]
    ChatBot
  }

  /// <summary>
  /// Тип чата в системе.
  /// </summary>
  [JsonConverter(typeof(StringEnumConverter))]
  public enum ChatType
  {
    /// <summary>
    /// Чат с поддержкой.
    /// </summary>
    [EnumMember(Value = "SellerSupport")]
    [Display(Name = "Чат с поддержкой")]
    SellerSupport,

    /// <summary>
    /// Чат с покупателем.
    /// </summary>
    [EnumMember(Value = "BuyerSeller")]
    [Display(Name = "Чат с покупателем")]
    BuyerSeller,

    /// <summary>
    /// Уведомления Ozon.
    /// </summary>
    [EnumMember(Value = "SellerNotification")]
    [Display(Name = "Уведомления Ozon")]
    SellerNotification
  }
}
