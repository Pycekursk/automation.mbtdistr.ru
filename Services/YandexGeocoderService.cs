using System.Text.Json;

namespace automation.mbtdistr.ru.Services
{
  public class YandexGeocoderService
  {
    private readonly string _apiKey;
    private readonly HttpClient _httpClient;

    public YandexGeocoderService(string apiKey)
    {
      _apiKey = apiKey;
      _httpClient = new HttpClient();
    }

    /// <summary>
    /// Возвращает заполненный объект Address (включая координаты и разобранные компоненты).
    /// </summary>
    public async Task<automation.mbtdistr.ru.Models.Address?> GetAddressAsync(string addressQuery, CancellationToken cancellation = default)
    {
      var url = $"https://geocode-maps.yandex.ru/1.x/?apikey={_apiKey}&geocode={Uri.EscapeDataString(addressQuery)}&format=json&kind=house";
      var response = await _httpClient.GetAsync(url, cancellation);
      response.EnsureSuccessStatusCode();

      using var stream = await response.Content.ReadAsStreamAsync(cancellation);
      using var doc = await JsonDocument.ParseAsync(stream, default, cancellation);

      // Навигация к первому GeoObject
      if (!doc.RootElement
             .GetProperty("response")
             .GetProperty("GeoObjectCollection")
             .GetProperty("featureMember")
             .EnumerateArray()
             .Any())
        return null;

      var geoObject = doc.RootElement
          .GetProperty("response")
          .GetProperty("GeoObjectCollection")
          .GetProperty("featureMember")[0]
          .GetProperty("GeoObject");

      // 1) Координаты
      var pos = geoObject
          .GetProperty("Point")
          .GetProperty("pos")
          .GetString()!; // формат "lon lat"
      var parts = pos.Split(' ');
      var lon = decimal.Parse(parts[0], System.Globalization.CultureInfo.InvariantCulture);
      var lat = decimal.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture);

      // 2) Полный адрес (текстовый)
      var meta = geoObject
          .GetProperty("metaDataProperty")
          .GetProperty("GeocoderMetaData");
      var fullAddress = meta.GetProperty("text").GetString();

      // 3) Компоненты разбора адреса
      //    находятся в meta -> Address -> Components
      var compRoot = meta
          .GetProperty("Address")
          .GetProperty("Components")
          .EnumerateArray();

      var result = new automation.mbtdistr.ru.Models.Address
      {
        FullAddress = fullAddress,
        Latitude = lat,
        Longitude = lon
      };

      foreach (var comp in compRoot)
      {
        var kind = comp.GetProperty("kind").GetString();
        var name = comp.GetProperty("name").GetString();

        switch (kind)
        {
          case "country":
            result.Country = name;
            break;
          case "province":   // область
                             // при желании можно сохранить или проигнорировать
            break;
          case "locality":   // город/нас. пункт
            result.City = name;
            break;
          case "street":
            result.Street = name;
            break;
          case "house":
            result.House = name;
            break;
          case "postal_code":
            result.ZipCode = name;
            break;
          // Yandex не отдаёт отдельного office — можно попытаться
          // вытянуть из полного адреса regex-ом, но обычно не требуется.
          default:
            break;
        }
      }

      return result;
    }
  }

}
