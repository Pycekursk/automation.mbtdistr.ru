using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;

namespace automation.mbtdistr.ru.Services.Google.Sheets
{

  public class SheetsApiService
  {
    private readonly SheetsService _sheets;
    private readonly DriveService _drive;

    public SheetsApiService(IConfiguration config)
    {
      var credential = GoogleCredential
          .FromFile(config["GoogleApi:Sheets:ServiceAccountKeyFile"])
          .CreateScoped(new[]
          {
            SheetsService.Scope.Spreadsheets,
            DriveService.Scope.Drive
          });

      _sheets = new SheetsService(new BaseClientService.Initializer
      {
        HttpClientInitializer = credential,
        ApplicationName = config["GoogleApi:Drive:ApplicationName"]
      });

      _drive = new DriveService(new BaseClientService.Initializer
      {
        HttpClientInitializer = credential,
        ApplicationName = config["GoogleApi:Drive:ApplicationName"]
      });
    }

    public async Task<Spreadsheet> GetSpreadsheetByIdAsync(string spreadsheetId)
    {
      var request = _sheets.Spreadsheets.Get(spreadsheetId);
      request.Fields = "spreadsheetId,spreadsheetUrl";
      var spreadsheet = await request.ExecuteAsync();
      return spreadsheet as Spreadsheet ?? throw new InvalidOperationException("Не удалось получить таблицу");
    }

    //перезагрузка метода для получения на входе только ссылки на таблицу
    public async Task<Spreadsheet> GetSpreadsheetByUrlAsync(string spreadsheetUrl)
    {
      var request = _sheets.Spreadsheets.Get(spreadsheetUrl);
      request.Fields = "spreadsheetId,spreadsheetUrl";
      var spreadsheet = await request.ExecuteAsync();
      return spreadsheet as Spreadsheet ?? throw new InvalidOperationException("Не удалось получить таблицу");
    }



    /// <summary>
    /// Дает доступ «по ссылке» к документу (role = "reader" или "writer").
    /// </summary>
    public async Task ShareByLinkAsync(string spreadsheetId, string role = "reader")
    {
      var permission = new Permission
      {
        Type = "anyone",
        Role = role
      };
      await _drive.Permissions
        .Create(permission, spreadsheetId)
        .ExecuteAsync()
        .ConfigureAwait(false);
    }

    /// <summary>
    /// Создает новую таблицу и сразу делает её доступной по ссылке.
    /// </summary>
    public async Task<Spreadsheet> CreateAndShareAsync(
      string title,
      IEnumerable<string>? sheetTitles = null,
      string role = "reader")
    {
      // 1) создаём Spreadsheet
      var newSpreadsheet = new Spreadsheet
      {
        Properties = new SpreadsheetProperties { Title = title },
        Sheets = sheetTitles?
          .Select(t => new Sheet
          {
            Properties = new SheetProperties { Title = t }
          })
          .ToList()
      };
      var createReq = _sheets.Spreadsheets.Create(newSpreadsheet);
      createReq.Fields = "spreadsheetId,spreadsheetUrl";
      var sheet = await createReq.ExecuteAsync().ConfigureAwait(false);

      // 2) делаем его публичным «по ссылке»
      await ShareByLinkAsync(sheet.SpreadsheetId, role).ConfigureAwait(false);

      return sheet;
    }

    public async Task<IList<IList<object>>> ReadAsync(string spreadsheetId, string range)
    {
      var request = _sheets.Spreadsheets.Values.Get(spreadsheetId, range);
      var response = await request.ExecuteAsync();
      return response.Values;
    }

    public async Task UpdateAsync(string spreadsheetId, string range, IList<IList<object>> values)
    {
      var body = new ValueRange { Values = values };
      var request = _sheets.Spreadsheets.Values.Update(body, spreadsheetId, range);
      request.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.RAW;
      await request.ExecuteAsync();
    }


    // Новый метод: создание табличного файла
    public async Task<Spreadsheet> CreateSpreadsheetAsync(
        string title,
        IEnumerable<string>? sheetTitles = null)
    {
      // Формируем описание новой таблицы
      var newSpreadsheet = new Spreadsheet
      {
        Properties = new SpreadsheetProperties { Title = title },
        Sheets = sheetTitles?
              .Select(t => new Sheet
              {
                Properties = new SheetProperties { Title = t }
              })
              .ToList()
      };

      // Отправляем запрос на создание
      var request = _sheets.Spreadsheets.Create(newSpreadsheet);
      // Возвращаем из ответа только идентификатор и URL
      request.Fields = "spreadsheetId,spreadsheetUrl";
      var response = await request.ExecuteAsync();

      return response;
    }
    // TODO: AppendAsync, CreateSpreadsheetAsync и т.д.
  }
}
