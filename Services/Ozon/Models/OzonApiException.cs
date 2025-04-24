using System.Runtime.Serialization;

namespace automation.mbtdistr.ru.Services.Ozon.Models
{
  // Update the OzonApiException class to inherit from System.Exception  
  public class OzonApiException : Exception, ISerializable
  {
    public string V { get; }
    public Exception Ex { get; }

    // Constructor to initialize the non-nullable property 'V'
    public OzonApiException(string message, string v, Exception ex) : base(message, ex)
    {
      V = v ?? throw new ArgumentNullException(nameof(v));
      Ex = ex;
    }

    public OzonApiException(string? message, string v) : base(message)
    {
      V = v ?? throw new ArgumentNullException(nameof(v));
    }

    public OzonApiException(string message, Exception innerException) : base(message, innerException) { }

    public OzonApiException(string? message) : base(message)
    {
      V = string.Empty; // Default value to satisfy non-nullable property
    }
  }
}
