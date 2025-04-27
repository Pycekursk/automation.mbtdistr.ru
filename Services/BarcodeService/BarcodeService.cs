//using System.Drawing;
//using System.Drawing.Imaging;
using ZXing;
using System.Drawing;
using Telegram.Bot;
using Telegram.Bot.Types;

using ZXing;
using System.Drawing.Imaging;

namespace automation.mbtdistr.ru.Services.BarcodeService
{
  public class BarcodeService
  {
    private readonly ITelegramBotClient _botClient;
    private readonly ILogger<BarcodeService> _logger;

    public BarcodeService(ITelegramBotClient botClient, ILogger<BarcodeService> logger)
    {
      _botClient = botClient;
      _logger = logger;
    }

    public async Task HandlePhotoAsync(Update update)
    {
      if (update.Message?.Photo == null || !update.Message.Photo.Any())
      {
        _logger.LogWarning("Получено обновление без фото");
        return;
      }

      try
      {
        // Берём фото самого большого размера
        var photo = update.Message.Photo.OrderByDescending(p => p.FileSize).FirstOrDefault();
        if (photo == null)
        {
          _logger.LogWarning("Не удалось выбрать фото");
          return;
        }

        // Получаем файл
        var file = await _botClient.GetFile(photo.FileId);

        await using var memoryStream = new MemoryStream();
        await _botClient.DownloadFile(file.FilePath, memoryStream);
        memoryStream.Position = 0;

        // Декодируем штрихкод
        var barcode = DecodeBarcodeFromStream(memoryStream);

        if (!string.IsNullOrEmpty(barcode))
        {
          await _botClient.SendMessage(
              chatId: update.Message.Chat.Id,
              text: $"Штрихкод: {barcode}"
          );
        }
        else
        {
          await _botClient.SendMessage(
              chatId: update.Message.Chat.Id,
              text: "Не удалось распознать штрихкод на фото."
          );
        }
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Ошибка при обработке фото с штрихкодом");
        await _botClient.SendMessage(
            chatId: update.Message?.Chat.Id ?? 0,
            text: "Произошла ошибка при обработке фото."
        );
      }
    }

    private string? DecodeBarcodeFromStream(Stream imageStream)
    {
      try
      {
        using var bitmap = new Bitmap(imageStream);
        var reader = new BarcodeReaderGeneric
        {
          AutoRotate = true,
          TryInverted = true,
          Options = new ZXing.Common.DecodingOptions
          {
            TryHarder = true,
            PossibleFormats = new List<BarcodeFormat>
                        {
                            BarcodeFormat.EAN_13,
                            BarcodeFormat.CODE_128,
                            BarcodeFormat.QR_CODE,
                            BarcodeFormat.DATA_MATRIX
                        }
          }
        };

        // Преобразуем Bitmap в массив RGB
        var width = bitmap.Width;
        var height = bitmap.Height;
        var rgbData = GetRGBBytes(bitmap);

        var luminanceSource = new RGBLuminanceSource(rgbData, width, height);
        var result = reader.Decode(luminanceSource);
        return result?.Text;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Ошибка при декодировании штрихкода");
        return null;
      }
    }

    private byte[] GetRGBBytes(Bitmap bitmap)
    {
      var rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
      var bitmapData = bitmap.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

      try
      {
        int bytes = Math.Abs(bitmapData.Stride) * bitmap.Height;
        byte[] rgbValues = new byte[bytes];
        System.Runtime.InteropServices.Marshal.Copy(bitmapData.Scan0, rgbValues, 0, bytes);
        return rgbValues;
      }
      finally
      {
        bitmap.UnlockBits(bitmapData);
      }
    }
  }
}
