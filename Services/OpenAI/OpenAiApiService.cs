using System.ClientModel;
using System.ClientModel.Primitives;
using System.IO.Pipelines;
using System.Net.Http;

using Newtonsoft.Json;

using OpenAI;
using OpenAI.Audio;
using OpenAI.Chat;

using Org.BouncyCastle.Utilities.Encoders;
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

    /// <summary>
    /// Переводит произвольный текст на указан-ный язык через GPT-4.1.
    /// Вся настройка (модель, параметры, прокси, промпт) — локально внутри метода.
    /// </summary>
    public async Task<string> TranslateText(string text, string targetLanguage)
    {
      // 1.  Подготовка клиент-транспорта через прокси
      var apiKeyCredential = new ApiKeyCredential(_apiKey);
      var chatClient = new OpenAI.Chat.ChatClient("gpt-4.1", apiKeyCredential, new OpenAIClientOptions
      {
        Transport = new HttpClientPipelineTransport(
                new Models.Internal.HttpProxyClientFactory(_proxyString).CreateClient())
      });

      // 2.  Системный промпт с минимальным отраслевым глоссарием
      string systemPrompt =
          $@"You are a professional technical translator into {targetLanguage}.
1. Keep original layout (lists, tables, line breaks).
2. Use the following glossary consistently:
   动力 → Силовые сети
   消防 → Пожарная автоматика
   电讯 → Слаботочные системы
   给排水 → Водоснабжение и канализация
   暖通 → ОВК
3. Do NOT break words into characters.
4. Return **only** the translated text, no explanations.";
      ChatMessage systemMessage = ChatMessage.CreateAssistantMessage(systemPrompt);

      ChatMessage userChatMessage = ChatMessage.CreateUserMessage(text);

      ChatCompletionOptions chatCompletionOptions = new ChatCompletionOptions()
      {
        Temperature = 0.15f, // Температура генерации текста
        TopP = 0.9f, // Вероятностный срез
        MaxOutputTokenCount = 2000, // Максимальное количество токенов в ответе
      };

      var answer = chatClient.CompleteChat(new ChatMessage[] { systemMessage, userChatMessage }, chatCompletionOptions);

      return answer.Value.Content.ToString();
    }


    /* Отправляет ОДНУ партию в GPT-4 и возвращает массив переводов */
    public async Task<string[]> TranslateBatchAsync(string[] batch, string targetLanguage, CancellationToken ct = default)
    {
      var apiKeyCredential = new ApiKeyCredential(_apiKey);
      var client = new OpenAI.Chat.ChatClient("gpt-4.1", apiKeyCredential, new OpenAIClientOptions
      {
        Transport = new HttpClientPipelineTransport(
              new Models.Internal.HttpProxyClientFactory(_proxyString).CreateClient())
      });

      string systemMsg =
          $"You are a professional translator into {targetLanguage}. " +
          "Return ONLY a JSON array that contains the translations " +
          "in exactly the same order. No explanations.";

      string userMsg = JsonConvert.SerializeObject(batch);

      var answer = await client.CompleteChatAsync(
          new ChatMessage[]
          {
            ChatMessage.CreateSystemMessage(systemMsg),
            ChatMessage.CreateUserMessage(userMsg)
          },
          new ChatCompletionOptions
          {
            Temperature = 0.15f,
            MaxOutputTokenCount = 2000
          }, ct);

      var content = answer.Value.Content;

      return JsonConvert.DeserializeObject<string[]>(content[0].Text)!;
    }
  }
}
