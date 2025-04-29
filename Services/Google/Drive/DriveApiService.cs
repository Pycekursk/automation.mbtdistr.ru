using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;

namespace automation.mbtdistr.ru.Services.Google.Drive
{
  public class DriveApiService
  {

    private readonly DriveService _drive;

    public DriveApiService(IConfiguration config)
    {
      var credential = GoogleCredential
               .FromFile(config["GoogleApi:Drive:ServiceAccountKeyFile"])
               .CreateScoped(DriveService.ScopeConstants.Drive);
      _drive = new DriveService(new BaseClientService.Initializer
      {
        HttpClientInitializer = credential,
        ApplicationName = config["GoogleApi:Drive:ApplicationName"]
      });

    }
    public async Task<string> UploadFileAsync(string filePath, string mimeType, string? folderId = null)
    {
      //var metadata = new Google.Drive.Drive.v3.Data.File
      //{
      //  Name = Path.GetFileName(filePath),
      //  Parents = folderId != null ? new List<string> { folderId } : null
      //};
      //using var stream = new FileStream(filePath, FileMode.Open);
      //var request = _drive.Files.Create(metadata, stream, mimeType);
      //request.Fields = "id,webViewLink";
      //await request.UploadAsync();
      //return request.ResponseBody?.Id ?? throw new InvalidOperationException("Не удалось загрузить файл");
      return string.Empty;
    }

    // TODO: добавить методы ListFilesAsync, DownloadFileAsync и т.д.
  }
}
