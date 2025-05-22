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

    public async Task<(double lat, double lon)?> GetCoordinatesAsync(string address)
    {
      var url = $"https://geocode-maps.yandex.ru/1.x/?apikey={_apiKey}&geocode={Uri.EscapeDataString(address)}&format=json";
      var response = await _httpClient.GetAsync(url);
      response.EnsureSuccessStatusCode();

      var json = await response.Content.ReadAsStringAsync();

      using var doc = JsonDocument.Parse(json);
      var pos = doc
          .RootElement
          .GetProperty("response")
          .GetProperty("GeoObjectCollection")
          .GetProperty("featureMember")[0]
          .GetProperty("GeoObject")
          .GetProperty("Point")
          .GetProperty("pos")
          .GetString();

      // Пример ответа: "37.617635 55.755814"
      var parts = pos.Split(' ');
      if (parts.Length != 2)
        return null;

      double lon = double.Parse(parts[0], System.Globalization.CultureInfo.InvariantCulture);
      double lat = double.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture);

      return (lat, lon);
    }
  }
}
