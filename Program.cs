using automation.mbtdistr.ru.Data;
using automation.mbtdistr.ru.Models;
using automation.mbtdistr.ru.Services.BarcodeService;
using automation.mbtdistr.ru.Services.LLM;
using automation.mbtdistr.ru.Services.Ozon;
using automation.mbtdistr.ru.Services.Wildberries;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.SwaggerUI;

using System.Reflection;

using Telegram.Bot;

namespace automation.mbtdistr.ru
{
  public class Program
  {
    public static IServiceCollection Services { get; set; } = null!;
    public static IConfiguration Configuration { get; set; } = null!;

    public static void Main(string[] args)
    {
      var builder = WebApplication.CreateBuilder(args);
      Configuration = builder.Configuration;
      Services = builder.Services;

      // Add services to the container.

      builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
    new MySqlServerVersion(new Version(8, 0, 21))));


      builder.Services.AddControllersWithViews();

      // 2. Генерация XML-комментариев
      var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
      var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

      // 3. SwaggerGen
      builder.Services.AddSwaggerGen(c =>
      {
        // Основная документация
        c.SwaggerDoc("v1", new OpenApiInfo
        {
          Title = "My API",
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



      builder.Services.AddScoped<OzonSellerApiHttpClient>();
      builder.Services.AddScoped<OzonApiService>();
      builder.Services.AddSingleton<UserInputWaitingService>();

      builder.Services.AddScoped<BarcodeService>();


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
            CreatedAt = DateTime.UtcNow
          };
          dbContext.Workers.Add(adminWorker);
          dbContext.SaveChanges();
        }

      }

      // Configure the HTTP request pipeline.
      if (!app.Environment.IsDevelopment())
      {
        app.UseExceptionHandler("/Home/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
      }
      else
      {
        app.UseDeveloperExceptionPage();
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
          c.SwaggerEndpoint("/swagger/v1/swagger.json", "API V1");
          c.RoutePrefix = string.Empty; // Открывать Swagger по корню сайта
        });
      }

      if (app.Environment.IsDevelopment())
      {
        // 5. Swagger middleware
        app.UseSwagger(c =>
        {
          c.RouteTemplate = "docs/{documentName}/swagger.json";
          // при необходимости пишем PreSerializeFilters, чтобы динамически менять host, schemes и т.д.
        });

        // 6. Swagger UI
        app.UseSwaggerUI(c =>
        {
          // несколько версий
          c.SwaggerEndpoint("/docs/v1/swagger.json", "My API V1");
          // c.SwaggerEndpoint("/docs/v2/swagger.json", "My API V2");

          // UI-параметры
          c.RoutePrefix = "docs";                   // UI будет доступен по /docs
          c.DocumentTitle = "Документация My API";    // <title> страницы
          c.DocExpansion(DocExpansion.None);        // минимальное раскрытие разделов
          c.DefaultModelsExpandDepth(-1);                    // скрыть модели по умолчанию
          c.DisplayOperationId();                            // показывать operationId
          c.DisplayRequestDuration();                        // время выполнения запроса
          c.EnableDeepLinking();                             // прямые ссылки
          c.ShowExtensions();                                // показывать custom-расширения
          c.EnableFilter();                                  // фильтр по тегам и путям
          c.MaxDisplayedTags(10);                            // максимум тегов в фильтре
          c.SupportedSubmitMethods(new[] { SubmitMethod.Get, SubmitMethod.Post, SubmitMethod.Put, SubmitMethod.Delete, SubmitMethod.Patch });
          c.OAuthClientId("swagger-ui-client");
          c.OAuthClientSecret("secret-if-needed");
          c.OAuthAppName("Swagger UI for My API");
          c.OAuthUsePkce();                                  // для Authorization Code Flow
          c.InjectStylesheet("/swagger-ui/styles.css");      // свой CSS
          c.InjectJavascript("/swagger-ui/scripts.js");       // свой JS
          c.InjectJavascript("console.log('Swagger UI Loaded');");
          c.HeadContent = "<link rel=\"icon\" href=\"/swagger-ui/favicon.ico\" />";
          c.IndexStream = () => File.OpenRead("wwwroot/swagger-ui/index.html");
          // чтобы полностью заменить index.html
        });
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
