using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace automation.mbtdistr.ru.Services.YandexMarket.Models
{
  #region Enums

  //  /// <summary>
  //  /// Этап обработки заказа (если он имеет статус PROCESSING) или причина отмены заказа (если он имеет статус CANCELLED).
  //  /// </summary>
  //  [Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
  //  public enum YMOrderSubstatusType
  //  {
  //    /// <summary>Неизвестно</summary>
  //    [EnumMember(Value = "UNKNOWN")]
  //    [Display(Name = "Неизвестно")]
  //    Unknown = 0,

  //    /// <summary>Заказ подтвержден, его можно начать обрабатывать.</summary>
  //    [EnumMember(Value = "STARTED")]
  //    [Display(Name = "Начат")]
  //    Started,

  //    /// <summary>Заказ собран и готов к отправке.</summary>
  //    [EnumMember(Value = "READY_TO_SHIP")]
  //    [Display(Name = "Готов к отправке")]
  //    ReadyToShip,

  //    /// <summary>Заказ готов к доставке.</summary>
  //    [EnumMember(Value = "READY_FOR_DELIVERY")]
  //    [Display(Name = "Готов к доставке")]
  //    ReadyForDelivery,

  //    /// <summary>Заказ доставляется.</summary>
  //    [EnumMember(Value = "DELIVERING")]
  //    [Display(Name = "Доставляется")]
  //    Delivering,

  //    /// <summary>Доставлен службой доставки.</summary>
  //    [EnumMember(Value = "DELIVERY_SERVICE_DELIVERED")]
  //    [Display(Name = "Доставлено службой доставки")]
  //    DeliveryServiceDelivered,

  //    /// <summary>Истёк срок хранения в ПВЗ.</summary>
  //    [EnumMember(Value = "PICKUP_EXPIRED")]
  //    [Display(Name = "Истек срок хранения")]
  //    PickupExpired,

  //    /// <summary>Покупатель не завершил оформление зарезервированного заказа в течение 10 минут.</summary>
  //    [EnumMember(Value = "RESERVATION_EXPIRED")]
  //    [Display(Name = "Резерв истек")]
  //    ReservationExpired,

  //    /// <summary>Покупатель не оплатил заказ вовремя (PREPAID).</summary>
  //    [EnumMember(Value = "USER_NOT_PAID")]
  //    [Display(Name = "Не оплачен")]
  //    UserNotPaid,

  //    /// <summary>Покупатель отменил заказ по личным причинам.</summary>
  //    [EnumMember(Value = "USER_CHANGED_MIND")]
  //    [Display(Name = "Покупатель передумал")]
  //    UserChangedMind,

  //    /// <summary>Покупатель не отвечает на звонки.</summary>
  //    [EnumMember(Value = "USER_UNREACHABLE")]
  //    [Display(Name = "Покупатель недоступен")]
  //    UserUnreachable,

  //    /// <summary>Покупателя не устроили условия доставки.</summary>
  //    [EnumMember(Value = "USER_REFUSED_DELIVERY")]
  //    [Display(Name = "Отказ от доставки")]
  //    UserRefusedDelivery,

  //    /// <summary>Покупателю не подошел товар.</summary>
  //    [EnumMember(Value = "USER_REFUSED_PRODUCT")]
  //    [Display(Name = "Отказ от товара")]
  //    UserRefusedProduct,

  //    /// <summary>Покупателя не устроило качество товара.</summary>
  //    [EnumMember(Value = "USER_REFUSED_QUALITY")]
  //    [Display(Name = "Отказ из-за качества")]
  //    UserRefusedQuality,

  //    /// <summary>Заказ доставлялся слишком долго.</summary>
  //    [EnumMember(Value = "TOO_LONG_DELIVERY")]
  //    [Display(Name = "Слишком долгая доставка")]
  //    TooLongDelivery,

  //    /// <summary>Заказ переносили слишком много раз.</summary>
  //    [EnumMember(Value = "TOO_MANY_DELIVERY_DATE_CHANGES")]
  //    [Display(Name = "Слишком много переносов")]
  //    TooManyDeliveryDateChanges,

  //    /// <summary>Заказ отменен из-за ошибки магазина.</summary>
  //    [EnumMember(Value = "SHOP_FAILED")]
  //    [Display(Name = "Ошибка магазина")]
  //    ShopFailed,

  //    /// <summary>Магазин не смог подтвердить бронь товара.</summary>
  //    [EnumMember(Value = "SHOP_FAILED_TO_RESERVE")]
  //    [Display(Name = "Магазин не подтвердил бронь")]
  //    ShopFailedToReserve,

  //    /// <summary>Заказ истёк в статусе PROCESSING.</summary>
  //    [EnumMember(Value = "PROCESSING_EXPIRED")]
  //    [Display(Name = "Истек срок обработки")]
  //    ProcessingExpired,

  //    /// <summary>Заказ заменен на новый.</summary>
  //    [EnumMember(Value = "REPLACING_ORDER")]
  //    [Display(Name = "Замена заказа")]
  //    ReplacingOrder,

  //    /// <summary>Техническая ошибка на стороне Маркета.</summary>
  //    [EnumMember(Value = "TECHNICAL_ERROR")]
  //    [Display(Name = "Техническая ошибка")]
  //    TechnicalError,

  //      /// <summary>Покупатель нашел дешевле.</summary>
  //[EnumMember(Value = "USER_BOUGHT_CHEAPER")]
  //    [Display(Name = "Покупатель нашел дешевле")]
  //    UserBoughtCheaper,

  //  }



  /// <summary>
  /// Способ оплаты заказа.
  /// </summary>
  [JsonConverter(typeof(StringEnumConverter))]
  public enum YMOrderPaymentMethodType
  {
    // Значения для оплаты при оформлении заказа (PREPAID)
    /// <summary>Банковской картой.</summary>
    [EnumMember(Value = "YANDEX")]
    [Display(Name = "банковской картой")]
    YANDEX,

    /// <summary>Apple Pay.</summary>
    [EnumMember(Value = "APPLE_PAY")]
    [Display(Name = "Apple Pay")]
    APPLE_PAY,

    /// <summary>Google Pay.</summary>
    [EnumMember(Value = "GOOGLE_PAY")]
    [Display(Name = "Google Pay")]
    GOOGLE_PAY,

    /// <summary>В кредит.</summary>
    [EnumMember(Value = "CREDIT")]
    [Display(Name = "в кредит")]
    CREDIT,

    /// <summary>В кредит в Тинькофф Банке.</summary>
    [EnumMember(Value = "TINKOFF_CREDIT")]
    [Display(Name = "в кредит в Тинькофф Банке")]
    TINKOFF_CREDIT,

    /// <summary>Рассрочка в Тинькофф Банке.</summary>
    [EnumMember(Value = "TINKOFF_INSTALLMENTS")]
    [Display(Name = "рассрочка в Тинькофф Банке")]
    TINKOFF_INSTALLMENTS,

    /// <summary>Подарочным сертификатом (например, из приложения «Сбербанк Онлайн»).</summary>
    [EnumMember(Value = "EXTERNAL_CERTIFICATE")]
    [Display(Name = "подарочным сертификатом")]
    EXTERNAL_CERTIFICATE,

    /// <summary>Через систему быстрых платежей.</summary>
    [EnumMember(Value = "SBP")]
    [Display(Name = "через систему быстрых платежей")]
    SBP,

    /// <summary>Заказ оплачивает организация (аванс).</summary>
    [EnumMember(Value = "B2B_ACCOUNT_PREPAYMENT")]
    [Display(Name = "заказ оплачивает организация")]
    B2B_ACCOUNT_PREPAYMENT,

    // Значения для оплаты при получении заказа (POSTPAID)
    /// <summary>Банковской картой при получении.</summary>
    [EnumMember(Value = "CARD_ON_DELIVERY")]
    [Display(Name = "банковской картой при получении")]
    CARD_ON_DELIVERY,

    /// <summary>Привязанной картой при получении.</summary>
    [EnumMember(Value = "BOUND_CARD_ON_DELIVERY")]
    [Display(Name = "привязанной картой при получении")]
    BOUND_CARD_ON_DELIVERY,

    /// <summary>Супер Сплитом.</summary>
    [EnumMember(Value = "BNPL_BANK_ON_DELIVERY")]
    [Display(Name = "супер Сплитом")]
    BNPL_BANK_ON_DELIVERY,

    /// <summary>Сплитом.</summary>
    [EnumMember(Value = "BNPL_ON_DELIVERY")]
    [Display(Name = "Сплитом")]
    BNPL_ON_DELIVERY,

    /// <summary>Наличными при получении.</summary>
    [EnumMember(Value = "CASH_ON_DELIVERY")]
    [Display(Name = "наличными при получении")]
    CASH_ON_DELIVERY,

    /// <summary>Заказ оплачивает организация после доставки.</summary>
    [EnumMember(Value = "B2B_ACCOUNT_POSTPAYMENT")]
    [Display(Name = "заказ оплачивает организация после доставки")]
    B2B_ACCOUNT_POSTPAYMENT,

    /// <summary>Неизвестный тип.</summary>
    [EnumMember(Value = "UNKNOWN")]
    [Display(Name = "неизвестный тип")]
    UNKNOWN = 0
  }



  /// <summary>
  /// Подстатусы заказа
  /// </summary>
  [JsonConverter(typeof(StringEnumConverter))]
  public enum YMOrderSubstatusType
  {
    /// <summary>Резервирование истекло</summary>
    [EnumMember(Value = "RESERVATION_EXPIRED")]
    [Display(Name = "Резервирование истекло")]
    RESERVATION_EXPIRED,

    /// <summary>Покупатель не оплатил</summary>
    [EnumMember(Value = "USER_NOT_PAID")]
    [Display(Name = "Покупатель не оплатил")]
    USER_NOT_PAID,

    /// <summary>Покупатель недоступен</summary>
    [EnumMember(Value = "USER_UNREACHABLE")]
    [Display(Name = "Покупатель недоступен")]
    USER_UNREACHABLE,

    /// <summary>Покупатель передумал</summary>
    [EnumMember(Value = "USER_CHANGED_MIND")]
    [Display(Name = "Покупатель передумал")]
    USER_CHANGED_MIND,

    /// <summary>Покупатель отказался от доставки</summary>
    [EnumMember(Value = "USER_REFUSED_DELIVERY")]
    [Display(Name = "Покупатель отказался от доставки")]
    USER_REFUSED_DELIVERY,

    /// <summary>Покупатель отказался от товара</summary>
    [EnumMember(Value = "USER_REFUSED_PRODUCT")]
    [Display(Name = "Покупатель отказался от товара")]
    USER_REFUSED_PRODUCT,

    /// <summary>Сбой магазина</summary>
    [EnumMember(Value = "SHOP_FAILED")]
    [Display(Name = "Сбой магазина")]
    SHOP_FAILED,

    /// <summary>Покупатель отказался из-за качества</summary>
    [EnumMember(Value = "USER_REFUSED_QUALITY")]
    [Display(Name = "Покупатель отказался из-за качества")]
    USER_REFUSED_QUALITY,

    /// <summary>Заказ заменяется</summary>
    [EnumMember(Value = "REPLACING_ORDER")]
    [Display(Name = "Заказ заменяется")]
    REPLACING_ORDER,

    /// <summary>Обработка истекла</summary>
    [EnumMember(Value = "PROCESSING_EXPIRED")]
    [Display(Name = "Обработка истекла")]
    PROCESSING_EXPIRED,

    /// <summary>Ожидание истекло</summary>
    [EnumMember(Value = "PENDING_EXPIRED")]
    [Display(Name = "Ожидание истекло")]
    PENDING_EXPIRED,

    /// <summary>Отмена ожидания истекла</summary>
    [EnumMember(Value = "PENDING_CANCELLED")]
    [Display(Name = "Отмена ожидания истекла")]
    PENDING_CANCELLED,

    /// <summary>Магазин отменила ожидание</summary>
    [EnumMember(Value = "SHOP_PENDING_CANCELLED")]
    [Display(Name = "Магазин отменила ожидание")]
    SHOP_PENDING_CANCELLED,

    /// <summary>Пользователь признан мошенником</summary>
    [EnumMember(Value = "USER_FRAUD")]
    [Display(Name = "Пользователь признан мошенником")]
    USER_FRAUD,

    /// <summary>Резервирование не удалось</summary>
    [EnumMember(Value = "RESERVATION_FAILED")]
    [Display(Name = "Резервирование не удалось")]
    RESERVATION_FAILED,

    /// <summary>Пользователь оформил другой заказ</summary>
    [EnumMember(Value = "USER_PLACED_OTHER_ORDER")]
    [Display(Name = "Пользователь оформил другой заказ")]
    USER_PLACED_OTHER_ORDER,

    /// <summary>Пользователь купил дешевле</summary>
    [EnumMember(Value = "USER_BOUGHT_CHEAPER")]
    [Display(Name = "Пользователь купил дешевле")]
    USER_BOUGHT_CHEAPER,

    /// <summary>Товара не хватает</summary>
    [EnumMember(Value = "MISSING_ITEM")]
    [Display(Name = "Товара не хватает")]
    MISSING_ITEM,

    /// <summary>Товар поврежден</summary>
    [EnumMember(Value = "BROKEN_ITEM")]
    [Display(Name = "Товар поврежден")]
    BROKEN_ITEM,

    /// <summary>Неверный товар</summary>
    [EnumMember(Value = "WRONG_ITEM")]
    [Display(Name = "Неверный товар")]
    WRONG_ITEM,

    /// <summary>Срок самовывоза истек</summary>
    [EnumMember(Value = "PICKUP_EXPIRED")]
    [Display(Name = "Срок самовывоза истек")]
    PICKUP_EXPIRED,

    /// <summary>Проблемы с доставкой</summary>
    [EnumMember(Value = "DELIVERY_PROBLEMS")]
    [Display(Name = "Проблемы с доставкой")]
    DELIVERY_PROBLEMS,

    /// <summary>Поздний контакт</summary>
    [EnumMember(Value = "LATE_CONTACT")]
    [Display(Name = "Поздний контакт")]
    LATE_CONTACT,

    /// <summary>Пользовательские данные</summary>
    [EnumMember(Value = "CUSTOM")]
    [Display(Name = "Пользовательские данные")]
    CUSTOM,

    /// <summary>Ошибка службы доставки</summary>
    [EnumMember(Value = "DELIVERY_SERVICE_FAILED")]
    [Display(Name = "Ошибка службы доставки")]
    DELIVERY_SERVICE_FAILED,

    /// <summary>Склад не смог отправить</summary>
    [EnumMember(Value = "WAREHOUSE_FAILED_TO_SHIP")]
    [Display(Name = "Склад не смог отправить")]
    WAREHOUSE_FAILED_TO_SHIP,

    /// <summary>Служба доставки не доставила (опечатка)</summary>
    [EnumMember(Value = "DELIVERY_SERIVCE_UNDELIVERED")]
    [Display(Name = "Служба доставки не доставила (опечатка)")]
    DELIVERY_SERIVCE_UNDELIVERED,

    /// <summary>Служба доставки не доставила</summary>
    [EnumMember(Value = "DELIVERY_SERVICE_UNDELIVERED")]
    [Display(Name = "Служба доставки не доставила")]
    DELIVERY_SERVICE_UNDELIVERED,

    /// <summary>Предзаказ</summary>
    [EnumMember(Value = "PREORDER")]
    [Display(Name = "Предзаказ")]
    PRE​ORDER,

    /// <summary>Ожидание подтверждения</summary>
    [EnumMember(Value = "AWAIT_CONFIRMATION")]
    [Display(Name = "Ожидание подтверждения")]
    AWAIT_CONFIRMATION,

    /// <summary>Начато</summary>
    [EnumMember(Value = "STARTED")]
    [Display(Name = "Начато")]
    STARTED,

    /// <summary>Упаковка</summary>
    [EnumMember(Value = "PACKAGING")]
    [Display(Name = "Упаковка")]
    PACKAGING,

    /// <summary>Готов к отправке</summary>
    [EnumMember(Value = "READY_TO_SHIP")]
    [Display(Name = "Готов к отправке")]
    READY_TO_SHIP,

    /// <summary>Отправлен</summary>
    [EnumMember(Value = "SHIPPED")]
    [Display(Name = "Отправлен")]
    SHIPPED,

    /// <summary>Асинхронная обработка</summary>
    [EnumMember(Value = "ASYNC_PROCESSING")]
    [Display(Name = "Асинхронная обработка")]
    ASYNC_PROCESSING,

    /// <summary>Пользователь отказался предоставить данные</summary>
    [EnumMember(Value = "USER_REFUSED_TO_PROVIDE_PERSONAL_DATA")]
    [Display(Name = "Пользователь отказался предоставить данные")]
    USER_REFUSED_TO_PROVIDE_PERSONAL_DATA,

    /// <summary>Ожидание ввода от пользователя</summary>
    [EnumMember(Value = "WAITING_USER_INPUT")]
    [Display(Name = "Ожидание ввода от пользователя")]
    WAITING_USER_INPUT,

    /// <summary>Ожидание решения банка</summary>
    [EnumMember(Value = "WAITING_BANK_DECISION")]
    [Display(Name = "Ожидание решения банка")]
    WAITING_BANK_DECISION,

    /// <summary>Банк отверг кредитное предложение</summary>
    [EnumMember(Value = "BANK_REJECT_CREDIT_OFFER")]
    [Display(Name = "Банк отверг кредитное предложение")]
    BANK_REJECT_CREDIT_OFFER,

    /// <summary>Покупатель отверг кредитное предложение</summary>
    [EnumMember(Value = "CUSTOMER_REJECT_CREDIT_OFFER")]
    [Display(Name = "Покупатель отверг кредитное предложение")]
    CUSTOMER_REJECT_CREDIT_OFFER,

    /// <summary>Ошибка кредитного предложения</summary>
    [EnumMember(Value = "CREDIT_OFFER_FAILED")]
    [Display(Name = "Ошибка кредитного предложения")]
    CREDIT_OFFER_FAILED,

    /// <summary>Ожидание подтверждения дат доставки</summary>
    [EnumMember(Value = "AWAIT_DELIVERY_DATES_CONFIRMATION")]
    [Display(Name = "Ожидание подтверждения дат доставки")]
    AWAIT_DELIVERY_DATES_CONFIRMATION,

    /// <summary>Сбой сервиса</summary>
    [EnumMember(Value = "SERVICE_FAULT")]
    [Display(Name = "Сбой сервиса")]
    SERVICE_FAULT,

    /// <summary>Служба доставки приняла</summary>
    [EnumMember(Value = "DELIVERY_SERVICE_RECEIVED")]
    [Display(Name = "Служба доставки приняла")]
    DELIVERY_SERVICE_RECEIVED,

    /// <summary>Пользователь получил</summary>
    [EnumMember(Value = "USER_RECEIVED")]
    [Display(Name = "Пользователь получил")]
    USER_RECEIVED,

    /// <summary>Ожидание запасов</summary>
    [EnumMember(Value = "WAITING_FOR_STOCKS")]
    [Display(Name = "Ожидание запасов")]
    WAITING_FOR_STOCKS,

    /// <summary>В составе мультизаказа</summary>
    [EnumMember(Value = "AS_PART_OF_MULTI_ORDER")]
    [Display(Name = "В составе мультизаказа")]
    AS_PART_OF_MULTI_ORDER,

    /// <summary>Готов для последней мили</summary>
    [EnumMember(Value = "READY_FOR_LAST_MILE")]
    [Display(Name = "Готов для последней мили")]
    READY_FOR_LAST_MILE,

    /// <summary>Последняя миля начата</summary>
    [EnumMember(Value = "LAST_MILE_STARTED")]
    [Display(Name = "Последняя миля начата")]
    LAST_MILE_STARTED,

    /// <summary>Антифрод</summary>
    [EnumMember(Value = "ANTIFRAUD")]
    [Display(Name = "Антифрод")]
    ANTIFRAUD,

    /// <summary>Доставлено, пользователь не получил</summary>
    [EnumMember(Value = "DELIVERY_USER_NOT_RECEIVED")]
    [Display(Name = "Доставлено, пользователь не получил")]
    DELIVERY_USER_NOT_RECEIVED,

    /// <summary>Служба доставки доставила</summary>
    [EnumMember(Value = "DELIVERY_SERVICE_DELIVERED")]
    [Display(Name = "Служба доставки доставила")]
    DELIVERY_SERVICE_DELIVERED,

    /// <summary>Доставлено - пользователь не получил</summary>
    [EnumMember(Value = "DELIVERED_USER_NOT_RECEIVED")]
    [Display(Name = "Доставлено - пользователь не получил")]
    DELIVERED_USER_NOT_RECEIVED,

    /// <summary>Покупатель хотел другой способ оплаты</summary>
    [EnumMember(Value = "USER_WANTED_ANOTHER_PAYMENT_METHOD")]
    [Display(Name = "Покупатель хотел другой способ оплаты")]
    USER_WANTED_ANOTHER_PAYMENT_METHOD,

    /// <summary>Пользователь получил - техническая ошибка</summary>
    [EnumMember(Value = "USER_RECEIVED_TECHNICAL_ERROR")]
    [Display(Name = "Пользователь получил - техническая ошибка")]
    USER_RECEIVED_TECHNICAL_ERROR,

    /// <summary>Пользователь забыл использовать бонус</summary>
    [EnumMember(Value = "USER_FORGOT_TO_USE_BONUS")]
    [Display(Name = "Пользователь забыл использовать бонус")]
    USER_FORGOT_TO_USE_BONUS,

    /// <summary>Принят на распределительном центре</summary>
    [EnumMember(Value = "RECEIVED_ON_DISTRIBUTION_CENTER")]
    [Display(Name = "Принят на распределительном центре")]
    RECEIVED_ON_DISTRIBUTION_CENTER,

    /// <summary>Служба доставки не приняла</summary>
    [EnumMember(Value = "DELIVERY_SERVICE_NOT_RECEIVED")]
    [Display(Name = "Служба доставки не приняла")]
    DELIVERY_SERVICE_NOT_RECEIVED,

    /// <summary>Служба доставки потеряла</summary>
    [EnumMember(Value = "DELIVERY_SERVICE_LOST")]
    [Display(Name = "Служба доставки потеряла")]
    DELIVERY_SERVICE_LOST,

    /// <summary>Отправлено неправильной службе доставки</summary>
    [EnumMember(Value = "SHIPPED_TO_WRONG_DELIVERY_SERVICE")]
    [Display(Name = "Отправлено неправильной службе доставки")]
    SHIPPED_TO_WRONG_DELIVERY_SERVICE,

    /// <summary>Доставлено - пользователь получил</summary>
    [EnumMember(Value = "DELIVERED_USER_RECEIVED")]
    [Display(Name = "Доставлено - пользователь получил")]
    DELIVERED_USER_RECEIVED,

    /// <summary>Ожидание решения Тинькофф</summary>
    [EnumMember(Value = "WAITING_TINKOFF_DECISION")]
    [Display(Name = "Ожидание решения Тинькофф")]
    WAITING_TINKOFF_DECISION,

    /// <summary>Поиск курьера</summary>
    [EnumMember(Value = "COURIER_SEARCH")]
    [Display(Name = "Поиск курьера")]
    COURIER_SEARCH,

    /// <summary>Курьер найден</summary>
    [EnumMember(Value = "COURIER_FOUND")]
    [Display(Name = "Курьер найден")]
    COURIER_FOUND,

    /// <summary>Курьер в пути к отправителю</summary>
    [EnumMember(Value = "COURIER_IN_TRANSIT_TO_SENDER")]
    [Display(Name = "Курьер в пути к отправителю")]
    COURIER_IN_TRANSIT_TO_SENDER,

    /// <summary>Курьер прибыл к отправителю</summary>
    [EnumMember(Value = "COURIER_ARRIVED_TO_SENDER")]
    [Display(Name = "Курьер прибыл к отправителю")]
    COURIER_ARRIVED_TO_SENDER,

    /// <summary>Курьер получил заказ</summary>
    [EnumMember(Value = "COURIER_RECEIVED")]
    [Display(Name = "Курьер получил заказ")]
    COURIER_RECEIVED,
    /// <summary>Курьер не найден</summary>
    [EnumMember(Value = "COURIER_NOT_FOUND")]
    [Display(Name = "Курьер не найден")]
    COURIER_NOT_FOUND,

    /// <summary>Курьер не доставил заказ</summary>
    [EnumMember(Value = "COURIER_NOT_DELIVER_ORDER")]
    [Display(Name = "Курьер не доставил заказ")]
    COURIER_NOT_DELIVER_ORDER,

    /// <summary>Курьер возвращает заказ</summary>
    [EnumMember(Value = "COURIER_RETURNS_ORDER")]
    [Display(Name = "Курьер возвращает заказ")]
    COURIER_RETURNS_ORDER,

    /// <summary>Курьер вернул заказ</summary>
    [EnumMember(Value = "COURIER_RETURNED_ORDER")]
    [Display(Name = "Курьер вернул заказ")]
    COURIER_RETURNED_ORDER,

    /// <summary>Ожидание ввода доставки пользователем</summary>
    [EnumMember(Value = "WAITING_USER_DELIVERY_INPUT")]
    [Display(Name = "Ожидание ввода доставки пользователем")]
    WAITING_USER_DELIVERY_INPUT,

    /// <summary>Служба самовывоза приняла</summary>
    [EnumMember(Value = "PICKUP_SERVICE_RECEIVED")]
    [Display(Name = "Служба самовывоза приняла")]
    PICKUP_SERVICE_RECEIVED,

    /// <summary>Самовывоз получен покупателем</summary>
    [EnumMember(Value = "PICKUP_USER_RECEIVED")]
    [Display(Name = "Самовывоз получен покупателем")]
    PICKUP_USER_RECEIVED,

    /// <summary>Отмена: курьер не найден</summary>
    [EnumMember(Value = "CANCELLED_COURIER_NOT_FOUND")]
    [Display(Name = "Отмена: курьер не найден")]
    CANCELLED_COURIER_NOT_FOUND,

    /// <summary>Курьер не приехал за заказом</summary>
    [EnumMember(Value = "COURIER_NOT_COME_FOR_ORDER")]
    [Display(Name = "Курьер не приехал за заказом")]
    COURIER_NOT_COME_FOR_ORDER,

    /// <summary>Регион не обслуживается</summary>
    [EnumMember(Value = "DELIVERY_NOT_MANAGED_REGION")]
    [Display(Name = "Регион не обслуживается")]
    DELIVERY_NOT_MANAGED_REGION,

    /// <summary>Неполные контактные данные</summary>
    [EnumMember(Value = "INCOMPLETE_CONTACT_INFORMATION")]
    [Display(Name = "Неполные контактные данные")]
    INCOMPLETE_CONTACT_INFORMATION,

    /// <summary>Неполный мультизаказ</summary>
    [EnumMember(Value = "INCOMPLETE_MULTI_ORDER")]
    [Display(Name = "Неполный мультизаказ")]
    INCOMPLETE_MULTI_ORDER,

    /// <summary>Неподходящий вес/размер</summary>
    [EnumMember(Value = "INAPPROPRIATE_WEIGHT_SIZE")]
    [Display(Name = "Неподходящий вес/размер")]
    INAPPROPRIATE_WEIGHT_SIZE,

    /// <summary>Техническая ошибка</summary>
    [EnumMember(Value = "TECHNICAL_ERROR")]
    [Display(Name = "Техническая ошибка")]
    TECHNICAL_ERROR,

    /// <summary>Потеряно на сортировочном центре</summary>
    [EnumMember(Value = "SORTING_CENTER_LOST")]
    [Display(Name = "Потеряно на сортировочном центре")]
    SORTING_CENTER_LOST,

    /// <summary>Поиск курьера не начат</summary>
    [EnumMember(Value = "COURIER_SEARCH_NOT_STARTED")]
    [Display(Name = "Поиск курьера не начат")]
    COURIER_SEARCH_NOT_STARTED,

    /// <summary>Потеряно</summary>
    [EnumMember(Value = "LOST")]
    [Display(Name = "Потеряно")]
    LOST,

    /// <summary>Ожидание оплаты</summary>
    [EnumMember(Value = "AWAIT_PAYMENT")]
    [Display(Name = "Ожидание оплаты")]
    AWAIT_PAYMENT,

    /// <summary>Ожидание резерва лавки</summary>
    [EnumMember(Value = "AWAIT_LAVKA_RESERVATION")]
    [Display(Name = "Ожидание резерва лавки")]
    AWAIT_LAVKA_RESERVATION,

    /// <summary>Пользователь хочет изменить адрес</summary>
    [EnumMember(Value = "USER_WANTS_TO_CHANGE_ADDRESS")]
    [Display(Name = "Пользователь хочет изменить адрес")]
    USER_WANTS_TO_CHANGE_ADDRESS,

    /// <summary>Полностью не выкупил</summary>
    [EnumMember(Value = "FULL_NOT_RANSOM")]
    [Display(Name = "Полностью не выкупил")]
    FULL_NOT_RANSOM,

    /// <summary>Несоответствие рецепта</summary>
    [EnumMember(Value = "PRESCRIPTION_MISMATCH")]
    [Display(Name = "Несоответствие рецепта")]
    PRESCRIPTION_MISMATCH,

    /// <summary>Потеря дропоффа</summary>
    [EnumMember(Value = "DROPOFF_LOST")]
    [Display(Name = "Потеря дропоффа")]
    DROPOFF_LOST,

    /// <summary>Дропофф закрыт</summary>
    [EnumMember(Value = "DROPOFF_CLOSED")]
    [Display(Name = "Дропофф закрыт")]
    DROPOFF_CLOSED,

    /// <summary>Начата доставка в магазин</summary>
    [EnumMember(Value = "DELIVERY_TO_STORE_STARTED")]
    [Display(Name = "Начата доставка в магазин")]
    DELIVERY_TO_STORE_STARTED,

    /// <summary>Пользователь хочет изменить дату доставки</summary>
    [EnumMember(Value = "USER_WANTS_TO_CHANGE_DELIVERY_DATE")]
    [Display(Name = "Пользователь хочет изменить дату доставки")]
    USER_WANTS_TO_CHANGE_DELIVERY_DATE,

    /// <summary>Неправильный товар доставлен</summary>
    [EnumMember(Value = "WRONG_ITEM_DELIVERED")]
    [Display(Name = "Неправильный товар доставлен")]
    WRONG_ITEM_DELIVERED,

    /// <summary>Повреждена коробка</summary>
    [EnumMember(Value = "DAMAGED_BOX")]
    [Display(Name = "Повреждена коробка")]
    DAMAGED_BOX,

    /// <summary>Ожидание дат доставки</summary>
    [EnumMember(Value = "AWAIT_DELIVERY_DATES")]
    [Display(Name = "Ожидание дат доставки")]
    AWAIT_DELIVERY_DATES,

    /// <summary>Поиск курьера для последней мили</summary>
    [EnumMember(Value = "LAST_MILE_COURIER_SEARCH")]
    [Display(Name = "Поиск курьера для последней мили")]
    LAST_MILE_COURIER_SEARCH,

    /// <summary>Пункт самовывоза закрыт</summary>
    [EnumMember(Value = "PICKUP_POINT_CLOSED")]
    [Display(Name = "Пункт самовывоза закрыт")]
    PICKUP_POINT_CLOSED,

    /// <summary>Изменены юридические данные</summary>
    [EnumMember(Value = "LEGAL_INFO_CHANGED")]
    [Display(Name = "Изменены юридические данные")]
    LEGAL_INFO_CHANGED,

    /// <summary>У пользователя нет времени на самовывоз</summary>
    [EnumMember(Value = "USER_HAS_NO_TIME_TO_PICKUP_ORDER")]
    [Display(Name = "У пользователя нет времени на самовывоз")]
    USER_HAS_NO_TIME_TO_PICKUP_ORDER,

    /// <summary>Товар прибыл в таможню</summary>
    [EnumMember(Value = "DELIVERY_CUSTOMS_ARRIVED")]
    [Display(Name = "Товар прибыл в таможню")]
    DELIVERY_CUSTOMS_ARRIVED,

    /// <summary>Товар прошел таможню</summary>
    [EnumMember(Value = "DELIVERY_CUSTOMS_CLEARED")]
    [Display(Name = "Товар прошел таможню")]
    DELIVERY_CUSTOMS_CLEARED,

    /// <summary>Служба первой мили приняла</summary>
    [EnumMember(Value = "FIRST_MILE_DELIVERY_SERVICE_RECEIVED")]
    [Display(Name = "Служба первой мили приняла")]
    FIRST_MILE_DELIVERY_SERVICE_RECEIVED,

    /// <summary>Ожидание автоматических дат доставки</summary>
    [EnumMember(Value = "AWAIT_AUTO_DELIVERY_DATES")]
    [Display(Name = "Ожидание автоматических дат доставки")]
    AWAIT_AUTO_DELIVERY_DATES,
    /// <summary>Ожидание персональных данных от пользователя</summary>
    [EnumMember(Value = "AWAIT_USER_PERSONAL_DATA")]
    [Display(Name = "Ожидание персональных данных от пользователя")]
    AWAIT_USER_PERSONAL_DATA,

    /// <summary>Истек срок предоставления персональных данных</summary>
    [EnumMember(Value = "NO_PERSONAL_DATA_EXPIRED")]
    [Display(Name = "Истек срок предоставления персональных данных")]
    NO_PERSONAL_DATA_EXPIRED,

    /// <summary>Проблемы с таможней</summary>
    [EnumMember(Value = "CUSTOMS_PROBLEMS")]
    [Display(Name = "Проблемы с таможней")]
    CUSTOMS_PROBLEMS,

    /// <summary>Ожидание кассира</summary>
    [EnumMember(Value = "AWAIT_CASHIER")]
    [Display(Name = "Ожидание кассира")]
    AWAIT_CASHIER,

    /// <summary>Ожидание резервирования постоплаты</summary>
    [EnumMember(Value = "WAITING_POSTPAID_BUDGET_RESERVATION")]
    [Display(Name = "Ожидание резервирования постоплаты")]
    WAITING_POSTPAID_BUDGET_RESERVATION,

    /// <summary>Ожидание подтверждения по сервисному обслуживанию</summary>
    [EnumMember(Value = "AWAIT_SERVICEABLE_CONFIRMATION")]
    [Display(Name = "Ожидание подтверждения по сервисному обслуживанию")]
    AWAIT_SERVICEABLE_CONFIRMATION,

    /// <summary>Не удалось зарезервировать постоплату</summary>
    [EnumMember(Value = "POSTPAID_BUDGET_RESERVATION_FAILED")]
    [Display(Name = "Не удалось зарезервировать постоплату")]
    POSTPAID_BUDGET_RESERVATION_FAILED,

    /// <summary>Ожидание подтверждения индивидуальной цены</summary>
    [EnumMember(Value = "AWAIT_CUSTOM_PRICE_CONFIRMATION")]
    [Display(Name = "Ожидание подтверждения индивидуальной цены")]
    AWAIT_CUSTOM_PRICE_CONFIRMATION,

    /// <summary>Готов к самовывозу</summary>
    [EnumMember(Value = "READY_FOR_PICKUP")]
    [Display(Name = "Готов к самовывозу")]
    READY_FOR_PICKUP,

    /// <summary>Слишком много изменений даты доставки</summary>
    [EnumMember(Value = "TOO_MANY_DELIVERY_DATE_CHANGES")]
    [Display(Name = "Слишком много изменений даты доставки")]
    TOO_MANY_DELIVERY_DATE_CHANGES,

    /// <summary>Очень долгий срок доставки</summary>
    [EnumMember(Value = "TOO_LONG_DELIVERY")]
    [Display(Name = "Очень долгий срок доставки")]
    TOO_LONG_DELIVERY,

    /// <summary>Отложенный платёж</summary>
    [EnumMember(Value = "DEFERRED_PAYMENT")]
    [Display(Name = "Отложенный платёж")]
    DEFERRED_PAYMENT,

    /// <summary>Неизвестный статус</summary>
    [EnumMember(Value = "UNKNOWN")]
    [Display(Name = "Неизвестный статус")]
    UNKNOWN = 0
  }

  #endregion

}

