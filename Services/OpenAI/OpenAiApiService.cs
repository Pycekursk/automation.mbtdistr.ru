using System.ClientModel;
using System.ClientModel.Primitives;
using System.IO.Pipelines;
using System.Net.Http;

using OpenAI;
using OpenAI.Audio;
//using OpenAI.Audio;


namespace automation.mbtdistr.ru.Services.LLM
{
  public class OpenAiApiService : OpenAIClient
  {
    private string _apiKey = string.Empty;
    private string _proxyString = string.Empty;

    public OpenAiApiService(string apiKey, string proxyString)
        : base(
            // Передаём креденшелы
            new ApiKeyCredential(apiKey),
            // Настраиваем транспорт с прокси
            new OpenAIClientOptions
            {
              Transport = new HttpClientPipelineTransport(
                    // Здесь ваш фабричный метод создаёт HttpClient с прокси
                    new Models.Internal.HttpProxyClientFactory(proxyString).CreateClient()
                )
            }
          )
    {
      this._apiKey = apiKey;
      this._proxyString = proxyString;
      // Конструктор пуст, всё сделано в инициализации базового класса
    }


    /// <summary>
    /// Транскрибирует аудиопоток с помощью модели Whisper и возвращает полученный текст.
    /// </summary>
    public async Task<string> TranscribeAsync(Stream audioStream, string fileName, CancellationToken cancellationToken = default)
    {

      var apiKeyCredential = new ApiKeyCredential(_apiKey);
      string model = "whisper-1";

      OpenAI.Audio.AudioClient audioClient = new OpenAI.Audio.AudioClient(model, apiKeyCredential, new OpenAIClientOptions()
      {
        Transport = new HttpClientPipelineTransport(
            new Models.Internal.HttpProxyClientFactory(_proxyString).CreateClient()
        )
      });

      AudioTranscriptionOptions audioTranscriptionOptions = new AudioTranscriptionOptions()
      {
        Language = "ru",
        Prompt = "Транскрибируй аудио",
        ResponseFormat = AudioTranscriptionFormat.Simple
      };

      var response = audioClient.TranscribeAudio(audioStream, fileName, audioTranscriptionOptions, cancellationToken);

      var transcription = response.Value;

      if (transcription != null)
      {
        return transcription.Text;
      }
      else
      {
        throw new Exception("Transcription failed");
      }
    }
  }
}
