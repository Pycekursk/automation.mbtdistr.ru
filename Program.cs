using automation.mbtdistr.ru.Data;
using automation.mbtdistr.ru.Models;
using automation.mbtdistr.ru.Services.Ozon;
using automation.mbtdistr.ru.Services.Wildberries;
using automation.mbtdistr.ru.temp;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using Telegram.Bot;

namespace automation.mbtdistr.ru
{
  public class Program
  {
    public static void Main(string[] args)
    {
      var builder = WebApplication.CreateBuilder(args);

      // Add services to the container.

      builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
    new MySqlServerVersion(new Version(8, 0, 21))));

     
      builder.Services.AddControllersWithViews();

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
      builder.Services.AddTransient<WildberriesAuthHandler>();
      builder.Services.AddTransient<OzonAuthHandler>();

      // 3) HTTP‑клиенты
      builder.Services.AddHttpClient<WildberriesApiService>()
          .AddHttpMessageHandler<WildberriesAuthHandler>();
      builder.Services.AddHttpClient<OzonApiService>()
          .AddHttpMessageHandler<OzonAuthHandler>();


      builder.Services.AddScoped<OzonSellerApiHttpClient>();
      builder.Services.AddScoped<OzonApiService>();
      builder.Services.AddSingleton<UserInputWaitingService>();

      var app = builder.Build();

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
            Name = "Administrator",
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

      app.UseHttpsRedirection();
      app.UseStaticFiles();

      app.UseRouting();

      app.UseAuthorization();

      app.MapControllerRoute(
          name: "default",
          pattern: "{controller=Home}/{action=Index}/{id?}");

      app.Run();
    }
  }
}
