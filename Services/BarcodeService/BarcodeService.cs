using ZXing;
using System.Drawing;
using Telegram.Bot;
using Telegram.Bot.Types;

using ZXing;
using System.Drawing.Imaging;
using automation.mbtdistr.ru.Data;
using Microsoft.EntityFrameworkCore;
using SkiaSharp;
using ZXing.Common;
using ZXing.SkiaSharp;

namespace automation.mbtdistr.ru.Services.BarcodeService
{
  /// <summary>
  /// Сервис для обработки изображений с штрихкодами, полученных через Telegram-бота.
  /// Выполняет декодирование штрихкодов с помощью ZXing и SkiaSharp, а также поиск товаров по штрихкоду в базе данных.
  /// </summary>
  public class BarcodeService
  {
    private readonly ITelegramBotClient _botClient;
    private readonly ILogger<BarcodeService> _logger;
    private static readonly IBarcodeReader<SKBitmap> _reader;

    /// <summary>
    /// Конструктор сервиса штрихкодов.
    /// </summary>
    /// <param name="botClient">Клиент Telegram-бота.</param>
    /// <param name="logger">Логгер для ведения журнала событий.</param>
    public BarcodeService(ITelegramBotClient botClient, ILogger<BarcodeService> logger)
    {
      _botClient = botClient;
      _logger = logger;
    }

    /// <summary>
    /// Статический конструктор для инициализации декодера штрихкодов.
    /// </summary>
    static BarcodeService()
    {
      _reader = new BarcodeReader // из пакета ZXing.Net.Bindings.SkiaSharp
      {
        AutoRotate = true,
        Options = new DecodingOptions
        {
          TryInverted = true,
          TryHarder = true,
          PureBarcode = false,
          CharacterSet = "UTF-8",
          PossibleFormats = new[]
              {
                      BarcodeFormat.EAN_13,
                      BarcodeFormat.EAN_8,
                      BarcodeFormat.CODE_128,
                      BarcodeFormat.UPC_A,
                      BarcodeFormat.UPC_E,
                      BarcodeFormat.QR_CODE,
                      BarcodeFormat.DATA_MATRIX,
                      BarcodeFormat.AZTEC,
                      BarcodeFormat.PDF_417
                  }
        }
      };
    }

    /// <summary>
    /// Обрабатывает фото, полученное от пользователя Telegram, пытается распознать штрихкод и найти товар в базе.
    /// </summary>
    /// <param name="update">Объект обновления Telegram, содержащий фото.</param>
    public async Task HandlePhotoAsync(Update update)
    {
      if (update.Message?.Photo is null || update.Message.Photo.Length == 0)
      {
        _logger.LogWarning("Получено обновление без фото");
        return;
      }

      var photoSize = update.Message.Photo.OrderByDescending(p => p.FileSize).First();

      try
      {
        var file = await _botClient.GetFile(photoSize.FileId);

        await using var mem = new MemoryStream();
        await _botClient.DownloadFile(file.FilePath, mem);
        mem.Position = 0;

        var barcode = DecodeBarcode(mem);
        if (barcode is null)
        {
          await _botClient.SendMessage(update.Message.Chat.Id, "Не удалось распознать штрихкод на фото.");
          return;
        }

        await using var db = new ApplicationDbContext(new DbContextOptions<ApplicationDbContext>());
        var product = await db.Products
            .Include(p => p.Barcodes)
            .FirstOrDefaultAsync(p => p.Barcodes.Any(b => b.Barcode == barcode));

        var reply = product is null ? $"Товар ({barcode}) не найден в базе." : $"{product.Name} ({barcode})";
        await _botClient.SendMessage(update.Message.Chat.Id, reply);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Ошибка при обработке фото");
        await _botClient.SendMessage(update.Message.Chat.Id, "Произошла ошибка при обработке фото.");
      }
    }

    /// <summary>
    /// Пытается декодировать штрихкод из потока изображения, применяя несколько стратегий предобработки.
    /// </summary>
    /// <param name="imageStream">Поток с изображением.</param>
    /// <returns>Текст штрихкода или null, если не удалось распознать.</returns>
    private static string? DecodeBarcode(Stream imageStream)
    {
      using var original = SKBitmap.Decode(imageStream);
      if (original == null) return null;

      // 1. Попытка распознать на оригинальном изображении
      var text = _reader.Decode(original)?.Text;
      if (text != null) return text;

      // 2. Усиление контраста и резкости
      using var enhanced = Enhance(original);
      text = _reader.Decode(enhanced)?.Text;
      if (text != null) return text;

      // 3. Попытка на уменьшенном изображении
      var factor = 0.5f;
      var samplingOptions = new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.None);
      using var reduced = original.Resize(new SKImageInfo((int)(original.Width * factor), (int)(original.Height * factor)), samplingOptions);
      if (reduced != null)
      {
        text = _reader.Decode(reduced)?.Text;
        if (text != null) return text;
      }

      // 4. Попытка распознать на повернутых изображениях (90, 180, 270 градусов)
      foreach (var deg in new[] { 90, 180, 270 })
      {
        using var rotated = Rotate(original, deg);
        text = _reader.Decode(rotated)?.Text;
        if (text != null) return text;
      }

      return null;
    }

    /// <summary>
    /// Получает массив байтов RGB из Bitmap.
    /// </summary>
    /// <param name="bitmap">Изображение Bitmap.</param>
    /// <returns>Массив байтов RGB.</returns>
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

    /// <summary>
    /// Усиливает контраст и резкость изображения с помощью фильтра.
    /// </summary>
    /// <param name="source">Исходное изображение SKBitmap.</param>
    /// <returns>Обработанное изображение SKBitmap.</returns>
    private static SKBitmap Enhance(SKBitmap source)
    {
      var dest = new SKBitmap(source.Width, source.Height);
      using var canvas = new SKCanvas(dest);
      using var paint = new SKPaint
      {
        ImageFilter = SKImageFilter.CreateMatrixConvolution(
          new SKSizeI(3, 3),
          new float[]
          {
              0, -1,  0,
             -1,  5, -1,
              0, -1,  0
          },
          1,
          0,
          new SKPointI(1, 1),
          SKShaderTileMode.Clamp,
          true
        ),
        ColorFilter = SKColorFilter.CreateHighContrast(true, SKHighContrastConfigInvertStyle.NoInvert, 1)
      };
      canvas.DrawBitmap(source, 0, 0, paint);
      return dest;
    }

    /// <summary>
    /// Поворачивает изображение на заданное количество градусов.
    /// </summary>
    /// <param name="source">Исходное изображение SKBitmap.</param>
    /// <param name="degrees">Угол поворота в градусах.</param>
    /// <returns>Повернутое изображение SKBitmap.</returns>
    private static SKBitmap Rotate(SKBitmap source, int degrees)
    {
      double radians = Math.PI * degrees / 180.0;
      float sine = (float)Math.Abs(Math.Sin(radians));
      float cosine = (float)Math.Abs(Math.Cos(radians));

      int originalWidth = source.Width;
      int originalHeight = source.Height;
      int rotatedWidth = (int)(cosine * originalWidth + sine * originalHeight);
      int rotatedHeight = (int)(cosine * originalHeight + sine * originalWidth);

      var rotated = new SKBitmap(rotatedWidth, rotatedHeight);
      using var canvas = new SKCanvas(rotated);
      canvas.Clear();
      canvas.Translate(rotatedWidth / 2f, rotatedHeight / 2f);
      canvas.RotateDegrees(degrees);
      canvas.Translate(-originalWidth / 2f, -originalHeight / 2f);
      canvas.DrawBitmap(source, 0, 0);
      return rotated;
    }
  }
}
