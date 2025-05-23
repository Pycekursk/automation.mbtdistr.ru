using automation.mbtdistr.ru.Data;
using automation.mbtdistr.ru.Services;
using automation.mbtdistr.ru.Services.BarcodeService;
using automation.mbtdistr.ru.Services.Google.Drive;
using automation.mbtdistr.ru.Services.Google.Sheets;
using automation.mbtdistr.ru.Services.LLM;
using automation.mbtdistr.ru.Services.Ozon;
using automation.mbtdistr.ru.Services.Wildberries;
using automation.mbtdistr.ru.Services.YandexMarket;
using Microsoft.EntityFrameworkCore.InMemory;
using Google.Apis.Sheets.v4;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.SwaggerUI;

using System;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Unicode;

using Telegram.Bot;

using static automation.mbtdistr.ru.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Microsoft.Extensions.Hosting;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace automation.mbtdistr.ru
{
  public class Program
  {
    public static IWebHostEnvironment Environment { get; set; } = null!;

    public static IServiceCollection Services { get; set; } = null!;
    public static IConfiguration Configuration { get; set; } = null!;

    public static void Main(string[] args)
    {
      var builder = WebApplication.CreateBuilder(args);
      Configuration = builder.Configuration;
      Services = builder.Services;
      Environment = builder.Environment;

      // Add services to the container

      builder.Services.AddDbContext<ApplicationDbContext>(options =>
      {
        options.EnableDetailedErrors();
        options.EnableSensitiveDataLogging();

        var connectionString = Environment.IsProduction()
          ? Configuration.GetConnectionString("DefaultConnection")
          : Configuration.GetConnectionString("DebugConnection");

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
      });

      builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseInMemoryDatabase("TestDb"));

      builder.Services.AddControllersWithViews().AddNewtonsoftJson(options =>
      {
        options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
        options.SerializerSettings.Formatting = Formatting.Indented;
        options.SerializerSettings.Converters.Add(new StringEnumConverter());


      });




      builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));

      // 2. Генерация XML-комментариев
      var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
      var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

      // 3. SwaggerGen
      builder.Services.AddSwaggerGen(c =>
      {
        // Основная документация
        c.SwaggerDoc("v1", new OpenApiInfo
        {
          Title = "MBT API",
          Version = "v1",
          Description = "Максимальная функциональность Swagger UI",
          Contact = new OpenApiContact { Name = "Руслан", Email = "boss@it-wiki.site" },
          License = new OpenApiLicense { Name = "MIT" }
        });

        // Подключаем XML-комментарии
        c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);

        // Фильтры и примеры запросов/ответов
        c.ExampleFilters();                                      // из Swashbuckle.AspNetCore.Filters

        // Настройка нейминга схем (избежать коллизий)
        c.CustomSchemaIds(type => type.FullName);
      });

      // Регистрация Swagger примеров (если используете Swashbuckle.AspNetCore.Filters)
      builder.Services.AddSwaggerExamplesFromAssemblyOf<Program>();


      builder.Services.AddHostedService<automation.mbtdistr.ru.Services.MarketSyncService>();


      builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
      {

        options.User.AllowedUserNameCharacters = "абвгдеёжзийклмнопрстуфхцчшщьыъэюяАБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЬЫЪЭЮЯabcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
        options.User.RequireUniqueEmail = false;
        options.SignIn.RequireConfirmedPhoneNumber = false;
        options.SignIn.RequireConfirmedEmail = false;
        options.Password.RequireDigit = false;
        options.Password.RequiredLength = 3;
        options.Password.RequireLowercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
      })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

      // Telegram Bot (регистрируем через IConfiguration)
      builder.Services.AddSingleton<ITelegramBotClient>(sp =>
      {
        string? token = builder.Configuration["TelegramBot:Token"];
        if (string.IsNullOrEmpty(token))
          throw new ArgumentNullException("TelegramBot:Token", "Токен бота не задан в конфигурации");
        return new TelegramBotClient(token);
      });

      // 2) Регистрация handler’ов
      // builder.Services.AddTransient<WildberriesAuthHandler>();
      builder.Services.AddTransient<OzonAuthHandler>();

      string? openAiApiKey = builder.Configuration.GetSection("OpenAI:ApiKey").Value;
      string? openAiProxy = builder.Configuration.GetSection("Proxy:String").Value;
      if (!string.IsNullOrEmpty(openAiApiKey) && !string.IsNullOrEmpty(openAiProxy))
        builder.Services.AddTransient<OpenAiApiService>(s => new OpenAiApiService(openAiApiKey, openAiProxy));

      // 3) HTTP‑клиенты
      builder.Services.AddHttpClient<WildberriesApiService>()
          .AddHttpMessageHandler<WildberriesAuthHandler>();
      builder.Services.AddHttpClient<OzonApiService>()
          .AddHttpMessageHandler<OzonAuthHandler>();


      builder.Services.AddScoped<WBApiHttpClient>();
      builder.Services.AddScoped<WildberriesApiService>();

      builder.Services.AddScoped<YMApiService>();
      builder.Services.AddScoped<YMApiHttpClient>();



      builder.Services.AddScoped<OzonSellerApiHttpClient>();
      builder.Services.AddScoped<OzonApiService>();
      builder.Services.AddSingleton<UserInputWaitingService>();

      builder.Services.AddScoped<BarcodeService>();

      builder.Services.AddScoped<DriveApiService>();
      builder.Services.AddSingleton<SheetsApiService>();

      var app = builder.Build();
      app.UseStaticFiles();

      // После сборки app
      var scopeFactory = app.Services.GetRequiredService<IServiceScopeFactory>();
      using (var scope = scopeFactory.CreateScope())
      {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Проверяем, есть ли Worker с ролью Admin
        var adminWorker = dbContext.Workers.FirstOrDefault(w => w.Role == Models.RoleType.Admin);
        if (adminWorker == null)
        {
          // Если нет, создаем
          adminWorker = new automation.mbtdistr.ru.Models.Worker
          {
            TelegramId = "1406950293", // Можете сюда поставить фиксированное значение или оставить пустым
            Name = "Руслан",
            Role = Models.RoleType.Admin,
            CreatedAt = DateTime.UtcNow,
            Id = 3
          };
          dbContext.Workers.Add(adminWorker);
          dbContext.SaveChanges();
        }

      }



      if (app.Environment.IsDevelopment())
      {
        //// 5. Swagger middleware
        //app.UseSwagger(c =>
        //{
        //  c.RouteTemplate = "docs/{documentName}/swagger.json";
        //  // при необходимости пишем PreSerializeFilters, чтобы динамически менять host, schemes и т.д.
        //});
        app.UseDeveloperExceptionPage();
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
          c.SwaggerEndpoint("/swagger/v1/swagger.json", "API V1");
          c.InjectStylesheet("/swagger-ui/styles.css"); // свой CSS
          c.InjectJavascript("https://code.jquery.com/jquery-3.7.1.js");
          c.InjectJavascript("/swagger-ui/scripts.js"); // свой JS
          c.RoutePrefix = string.Empty; // Открывать Swagger по корню сайта
                                        // c.IndexStream = () => File.OpenRead("wwwroot/swagger-ui/index.html");
        });
      }
      else
      {
        //app.UseExceptionHandler("/Home/Error");

        app.UseDeveloperExceptionPage();

        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();

      }
      app.UseHttpsRedirection();
      app.UseRouting();

      app.UseAuthorization();

      app.MapControllerRoute(
          name: "default",
          pattern: "{controller=Home}/{action=Index}/{id?}");

      app.Run();
    }
  }
}
