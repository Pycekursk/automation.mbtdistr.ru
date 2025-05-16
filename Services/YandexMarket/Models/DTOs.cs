using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

using JsonConverter = Newtonsoft.Json.JsonConverter;
using JsonConverterAttribute = Newtonsoft.Json.JsonConverterAttribute;

namespace automation.mbtdistr.ru.Services.YandexMarket.Models
{

  #region OfferCardsContentStatus DTOs

  #region Request Models

  /// <summary>
  /// Модель запроса для получения информации о заполненности карточек магазина.
  /// </summary>
  public class YMOfferCardsContentStatusRequest
  {
    /// <summary>
    /// Фильтр по статусам карточек.
    /// </summary>
    [Display(Name = "Статусы карточек")]
    [JsonProperty("cardStatuses")]
    [JsonConverter(typeof(StringEnumConverter))]
    public List<YMOfferCardStatusType> CardStatuses { get; set; }

    /// <summary>
    /// Фильтр по категориям на Маркете.
    /// </summary>
    [Display(Name = "Категории на Маркете")]
    [JsonProperty("categoryIds")]
    public List<long> CategoryIds { get; set; }

    /// <summary>
    /// Идентификаторы товаров, информация о которых нужна.
    /// </summary>
    [Display(Name = "Идентификаторы товаров")]
    [JsonProperty("offerIds")]
    public List<string> OfferIds { get; set; }
  }

  #endregion

  #region Response Models

  /// <summary>
  /// Результат получения информации о заполненности карточек.
  /// </summary>
  public class YMOfferCardsContentStatusResult
  {
    /// <summary>
    /// Список товаров с информацией о состоянии карточек.
    /// </summary>
    [Display(Name = "Список карточек")]
    [JsonProperty("offerCards")]
    [Required]
    public List<YMOfferCard> OfferCards { get; set; }

    /// <summary>
    /// Идентификатор следующей страницы результатов.
    /// </summary>
    [Display(Name = "Пейджинг по результатам")]
    [JsonProperty("paging")]
    [Required]
    public YMForwardScrollingPager Paging { get; set; }
  }

  /// <summary>
  /// Информация о состоянии карточки товара.
  /// </summary>
  public class YMOfferCard
  {
    /// <summary>
    /// Ваш SKU — идентификатор товара в вашей системе.
    /// </summary>
    [Display(Name = "Ваш SKU — идентификатор товара в вашей системе")]
    [JsonProperty("offerId")]
    [Required]
    [MinLength(1)]
    [MaxLength(255)]
    public string OfferId { get; set; }

    /// <summary>
    /// Средний рейтинг карточки у товаров той категории.
    /// </summary>
    [Display(Name = "Средний рейтинг карточки")]
    [JsonProperty("averageContentRating")]
    public int AverageContentRating { get; set; }

    /// <summary>
    /// Статус карточки товара.
    /// </summary>
    [Display(Name = "Статус карточки")]
    [JsonProperty("cardStatus")]
    [JsonConverter(typeof(StringEnumConverter))]
    [Required]
    public YMOfferCardStatusType CardStatus { get; set; }

    /// <summary>
    /// Рейтинг карточки.
    /// </summary>
    [Display(Name = "Рейтинг карточки")]
    [JsonProperty("contentRating")]
    public int ContentRating { get; set; }

    /// <summary>
    /// Статус вычисления рейтинга карточки и рекомендаций.
    /// </summary>
    [Display(Name = "Статус вычисления рейтинга карточки")]
    [JsonProperty("contentRatingStatus")]
    [JsonConverter(typeof(StringEnumConverter))]
    [Required]
    public YMOfferCardContentStatusType ContentRatingStatus { get; set; }

    /// <summary>
    /// Ошибки в контенте, препятствующие размещению товара.
    /// </summary>
    [Display(Name = "Ошибки контента")]
    [JsonProperty("errors")]
    public List<YMOfferError> Errors { get; set; }

    /// <summary>
    /// Основная информация о карточке товара.
    /// </summary>
    [Display(Name = "Информация о карточке на Маркете")]
    [JsonProperty("mapping")]
    public YMGetMapping Mapping { get; set; }

    /// <summary>
    /// Список характеристик с их значениями.
    /// </summary>
    [Display(Name = "Список характеристик")]
    [JsonProperty("parameterValues")]
    public List<YMParameterValue> ParameterValues { get; set; }

    /// <summary>
    /// Список рекомендаций по заполнению карточки.
    /// </summary>
    [Display(Name = "Рекомендации по заполнению карточки")]
    [JsonProperty("recommendations")]
    public List<YMOfferCardRecommendation> Recommendations { get; set; }

    /// <summary>
    /// Предупреждения в контенте, не препятствующие размещению товара.
    /// </summary>
    [Display(Name = "Предупреждения контента")]
    [JsonProperty("warnings")]
    public List<YMOfferError> Warnings { get; set; }
  }

  /// <summary>
  /// Сообщение об ошибке, связанной с размещением товара.
  /// </summary>
  public class YMOfferError
  {
    /// <summary>
    /// Пояснение.
    /// </summary>
    [Display(Name = "Пояснение")]
    [JsonProperty("comment")]
    [Required]
    public string Comment { get; set; }

    /// <summary>
    /// Тип ошибки.
    /// </summary>
    [Display(Name = "Тип ошибки")]
    [JsonProperty("message")]
    [Required]
    public string Message { get; set; }
  }

  /// <summary>
  /// Основная информация о карточке товара на Маркете.
  /// </summary>
  public class YMGetMapping
  {
    /// <summary>
    /// Идентификатор категории на Маркете.
    /// </summary>
    [Display(Name = "Идентификатор категории на Маркете")]
    [JsonProperty("marketCategoryId")]
    [Required]
    [Range(1, long.MaxValue)]
    public long MarketCategoryId { get; set; }

    /// <summary>
    /// Название категории карточки.
    /// </summary>
    [Display(Name = "Название категории карточки")]
    [JsonProperty("marketCategoryName")]
    [Required]
    public string MarketCategoryName { get; set; }

    /// <summary>
    /// Идентификатор модели на Маркете.
    /// </summary>
    [Display(Name = "Идентификатор модели на Маркете")]
    [JsonProperty("marketModelId")]
    public long? MarketModelId { get; set; }

    /// <summary>
    /// Название модели карточки.
    /// </summary>
    [Display(Name = "Название модели карточки")]
    [JsonProperty("marketModelName")]
    public string MarketModelName { get; set; }

    /// <summary>
    /// Идентификатор карточки на Маркете.
    /// </summary>
    [Display(Name = "Идентификатор карточки")]
    [JsonProperty("marketSku")]
    public long? MarketSku { get; set; }

    /// <summary>
    /// Название карточки товара.
    /// </summary>
    [Display(Name = "Название карточки товара")]
    [JsonProperty("marketSkuName")]
    public string MarketSkuName { get; set; }
  }

  /// <summary>
  /// Значение характеристики товара.
  /// </summary>
  public class YMParameterValue
  {
    /// <summary>
    /// Идентификатор характеристики.
    /// </summary>
    [Display(Name = "Идентификатор характеристики")]
    [JsonProperty("parameterId")]
    [Required]
    [Range(1, long.MaxValue)]
    public long ParameterId { get; set; }

    /// <summary>
    /// Идентификатор единицы измерения.
    /// </summary>
    [Display(Name = "Идентификатор единицы измерения")]
    [JsonProperty("unitId")]
    public long? UnitId { get; set; }

    /// <summary>
    /// Значение характеристики.
    /// </summary>
    [Display(Name = "Значение характеристики")]
    [JsonProperty("value")]
    public string Value { get; set; }

    /// <summary>
    /// Идентификатор значения характеристики.
    /// </summary>
    [Display(Name = "Идентификатор значения характеристики")]
    [JsonProperty("valueId")]
    public long? ValueId { get; set; }
  }

  /// <summary>
  /// Рекомендация по заполнению карточки товара.
  /// </summary>
  public class YMOfferCardRecommendation
  {
    /// <summary>
    /// Идентификатор характеристики.
    /// </summary>
    [Display(Name = "Идентификатор характеристики")]
    [JsonProperty("parameterId")]
    [Required]
    [Range(1, long.MaxValue)]
    public long ParameterId { get; set; }

    /// <summary>
    /// Идентификатор единицы измерения.
    /// </summary>
    [Display(Name = "Идентификатор единицы измерения")]
    [JsonProperty("unitId")]
    public long? UnitId { get; set; }

    /// <summary>
    /// Значение рекомендации.
    /// </summary>
    [Display(Name = "Значение")]
    [JsonProperty("value")]
    public string Value { get; set; }

    /// <summary>
    /// Идентификатор значения рекомендации.
    /// </summary>
    [Display(Name = "Идентификатор значения")]
    [JsonProperty("valueId")]
    public long? ValueId { get; set; }

    /// <summary>
    /// Тип рекомендации.
    /// </summary>
    [Display(Name = "Тип рекомендации")]
    [JsonProperty("type")]
    [JsonConverter(typeof(StringEnumConverter))]
    [Required]
    public YMOfferCardRecommendationType Type { get; set; }

    /// <summary>
    /// Процент выполнения рекомендации.
    /// </summary>
    [Display(Name = "Процент выполнения")]
    [JsonProperty("percent")]
    public int? Percent { get; set; }

    /// <summary>
    /// Максимальное количество баллов рейтинга, которые можно получить.
    /// </summary>
    [Display(Name = "Оставшиеся баллы рейтинга")]
    [JsonProperty("remainingRatingPoints")]
    public int? RemainingRatingPoints { get; set; }
  }

  #endregion

  #region Enums

  /// <summary>
  /// Статус карточки товара.
  /// </summary>
  [JsonConverter(typeof(StringEnumConverter))]
  public enum YMOfferCardStatusType
  {
    /// <summary>Карточка Маркета.</summary>
    [EnumMember(Value = "HAS_CARD_CAN_NOT_UPDATE")]
    [Display(Name = "Карточка Маркета")]
    HAS_CARD_CAN_NOT_UPDATE,

    /// <summary>Можно дополнить.</summary>
    [EnumMember(Value = "HAS_CARD_CAN_UPDATE")]
    [Display(Name = "Можно дополнить")]
    HAS_CARD_CAN_UPDATE,

    /// <summary>Изменения не приняты.</summary>
    [EnumMember(Value = "HAS_CARD_CAN_UPDATE_ERRORS")]
    [Display(Name = "Изменения не приняты")]
    HAS_CARD_CAN_UPDATE_ERRORS,

    /// <summary>Изменения на проверке.</summary>
    [EnumMember(Value = "HAS_CARD_CAN_UPDATE_PROCESSING")]
    [Display(Name = "Изменения на проверке")]
    HAS_CARD_CAN_UPDATE_PROCESSING,

    /// <summary>Создайте карточку.</summary>
    [EnumMember(Value = "NO_CARD_NEED_CONTENT")]
    [Display(Name = "Создайте карточку")]
    NO_CARD_NEED_CONTENT,

    /// <summary>Создаст Маркет.</summary>
    [EnumMember(Value = "NO_CARD_MARKET_WILL_CREATE")]
    [Display(Name = "Создаст Маркет")]
    NO_CARD_MARKET_WILL_CREATE,

    /// <summary>Не создана из-за ошибки.</summary>
    [EnumMember(Value = "NO_CARD_ERRORS")]
    [Display(Name = "Не создана из-за ошибки")]
    NO_CARD_ERRORS,

    /// <summary>Проверяем данные.</summary>
    [EnumMember(Value = "NO_CARD_PROCESSING")]
    [Display(Name = "Проверяем данные")]
    NO_CARD_PROCESSING,

    /// <summary>Разместите товар в магазине.</summary>
    [EnumMember(Value = "NO_CARD_ADD_TO_CAMPAIGN")]
    [Display(Name = "Разместите товар в магазине")]
    NO_CARD_ADD_TO_CAMPAIGN
  }

  /// <summary>
  /// Статус вычисления рейтинга карточки и рекомендаций.
  /// </summary>
  [JsonConverter(typeof(StringEnumConverter))]
  public enum YMOfferCardContentStatusType
  {
    /// <summary>Рейтинг обновляется.</summary>
    [EnumMember(Value = "UPDATING")]
    [Display(Name = "Рейтинг обновляется")]
    UPDATING,

    /// <summary>Рейтинг актуальный.</summary>
    [EnumMember(Value = "ACTUAL")]
    [Display(Name = "Рейтинг актуальный")]
    ACTUAL
  }

  /// <summary>
  /// Тип рекомендации по заполнению карточки товара.
  /// </summary>
  [JsonConverter(typeof(StringEnumConverter))]
  public enum YMOfferCardRecommendationType
  {
    /// <summary>Есть видео.</summary>
    [EnumMember(Value = "HAS_VIDEO")]
    [Display(Name = "Есть видео")]
    HAS_VIDEO,

    /// <summary>Напишите название производителя так, как его пишет сам производитель.</summary>
    [EnumMember(Value = "RECOGNIZED_VENDOR")]
    [Display(Name = "Напишите название производителя так, как его пишет сам производитель")]
    RECOGNIZED_VENDOR,

    /// <summary>Заполните ключевые характеристики товара.</summary>
    [EnumMember(Value = "MAIN")]
    [Display(Name = "Заполните ключевые характеристики товара")]
    MAIN,

    /// <summary>Заполните дополнительные характеристики товара.</summary>
    [EnumMember(Value = "ADDITIONAL")]
    [Display(Name = "Заполните дополнительные характеристики товара")]
    ADDITIONAL,

    /// <summary>Заполните характеристики, которыми отличаются варианты товара.</summary>
    [EnumMember(Value = "DISTINCTIVE")]
    [Display(Name = "Заполните характеристики, которыми отличаются варианты товара")]
    DISTINCTIVE,

    /// <summary>Колонка устарела.</summary>
    [EnumMember(Value = "FILTERABLE")]
    [Display(Name = "Колонка устарела")]
    FILTERABLE,

    /// <summary>Добавьте изображения.</summary>
    [EnumMember(Value = "PICTURE_COUNT")]
    [Display(Name = "Добавьте изображения")]
    PICTURE_COUNT,

    /// <summary>Добавьте описание.</summary>
    [EnumMember(Value = "HAS_DESCRIPTION")]
    [Display(Name = "Добавьте описание")]
    HAS_DESCRIPTION,

    /// <summary>Добавьте штрихкод.</summary>
    [EnumMember(Value = "HAS_BARCODE")]
    [Display(Name = "Добавьте штрихкод")]
    HAS_BARCODE,

    /// <summary>Замените первое изображение более крупным.</summary>
    [EnumMember(Value = "FIRST_PICTURE_SIZE")]
    [Display(Name = "Замените первое изображение более крупным")]
    FIRST_PICTURE_SIZE,

    /// <summary>Измените название товара.</summary>
    [EnumMember(Value = "TITLE_LENGTH")]
    [Display(Name = "Измените название товара")]
    TITLE_LENGTH,

    /// <summary>Дополните описание.</summary>
    [EnumMember(Value = "DESCRIPTION_LENGTH")]
    [Display(Name = "Дополните описание")]
    DESCRIPTION_LENGTH,

    /// <summary>Замените изображения на высокого качества.</summary>
    [EnumMember(Value = "AVERAGE_PICTURE_SIZE")]
    [Display(Name = "Замените изображения на высокого качества")]
    AVERAGE_PICTURE_SIZE,

    /// <summary>Замените первое видео на высокого качества.</summary>
    [EnumMember(Value = "FIRST_VIDEO_SIZE")]
    [Display(Name = "Замените первое видео на высокого качества")]
    FIRST_VIDEO_SIZE,

    /// <summary>Добавьте первое видео рекомендуемой длины.</summary>
    [EnumMember(Value = "FIRST_VIDEO_LENGTH")]
    [Display(Name = "Добавьте первое видео рекомендуемой длины")]
    FIRST_VIDEO_LENGTH,

    /// <summary>Замените все видео на высокого качества.</summary>
    [EnumMember(Value = "AVERAGE_VIDEO_SIZE")]
    [Display(Name = "Замените все видео на высокого качества")]
    AVERAGE_VIDEO_SIZE,

    /// <summary>Добавьте хотя бы одно видео.</summary>
    [EnumMember(Value = "VIDEO_COUNT")]
    [Display(Name = "Добавьте хотя бы одно видео")]
    VIDEO_COUNT
  }

  #endregion

  #endregion


  /// <summary>
  /// Экземпляр товара.
  /// </summary>
  public class Instance
  {
    /// <summary>
    /// Статус экземпляра.
    /// </summary>
    [JsonPropertyName("status")]
    [JsonProperty("status")]
    public string Status { get; set; } = string.Empty;
  }

  /// <summary>
  /// Трек доставки.
  /// </summary>
  public class Track
  {
    /// <summary>
    /// Трек-код.
    /// </summary>
    [JsonPropertyName("trackCode")]
    [JsonProperty("trackCode")]
    public string TrackCode { get; set; } = string.Empty;
  }

  /// <summary>
  /// Запрос на получение списка возвратов.
  /// </summary>
  public class ReturnsListRequest
  {
    /// <summary>
    /// Фильтр запроса.
    /// </summary>
    [JsonPropertyName("filter")]
    [JsonProperty("filter")]
    public YMFilter Filter { get; set; } = new();

    /// <summary>
    /// Ограничение количества результатов.
    /// </summary>
    [JsonPropertyName("limit")]
    [JsonProperty("limit")]
    public int Limit { get; set; }

    /// <summary>
    /// Последний идентификатор возврата (для пагинации).
    /// </summary>
    [JsonPropertyName("lastId")]
    [JsonProperty("lastId")]
    public long? LastId { get; set; }
  }

}
