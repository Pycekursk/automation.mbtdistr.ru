using System.ClientModel;
using System.ClientModel.Primitives;
using System.Net.Http;

using OpenAI;


namespace automation.mbtdistr.ru.Services.LLM
{
  public class OpenAiApiService : OpenAIClient
  {
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
      // Конструктор пуст, всё сделано в инициализации базового класса
    }
  }
}
