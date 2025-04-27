using System.Text.Json.Serialization;

namespace automation.mbtdistr.ru.Services.Wildberries.Models
{
  public class ReturnsListResponse
  {
    [JsonPropertyName("claims")]
    public List<Claim> Claims { get; set; } = new();
    [JsonPropertyName("total")]
    public int Total { get; set; }
  }
}

