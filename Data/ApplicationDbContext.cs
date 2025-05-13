using automation.mbtdistr.ru.Models;
using automation.mbtdistr.ru.Services.YandexMarket;
using automation.mbtdistr.ru.Services.YandexMarket.Models;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

using System;
using System.Reflection.Emit;
using System.Text.Json;

using Telegram.Bot.Types;

using ConnectionParameter = automation.mbtdistr.ru.Models.ConnectionParameter;

namespace automation.mbtdistr.ru.Data
{
  public class ApplicationDbContext : IdentityDbContext<IdentityUser>
  {
    public DbSet<Worker> Workers { get; set; }
    public DbSet<NotificationOptions> NotificationOptions { get; set; }
    public DbSet<Return> Returns { get; set; }
    public DbSet<Cabinet> Cabinets { get; set; }
    public DbSet<CabinetSettings> CabinetSettings { get; set; }
    public DbSet<ConnectionParameter> ConnectionParameters { get; set; }
    public DbSet<Compensation> Compensations { get; set; }

    public DbSet<ReturnMainInfo> ReturnMainInfo { get; set; }

    public DbSet<YMSupplyRequest> YMSupplyRequests { get; set; }

    public DbSet<YMSupplyRequestLocation> YMLocations { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder mb)
    {
      base.OnModelCreating(mb);

      //mb.Entity<NotificationOptions>()
      //    .Property(o => o.NotificationLevels)
      //    .HasConversion(
      //v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
      //v => JsonSerializer.Deserialize<List<NotificationLevel>>(v, (JsonSerializerOptions?)null))
      //   .HasColumnType("longtext"); // или longtext, если JSON не поддерживается
      // Находим все enum-типы в сборке моделей, начинающиеся на "YM"
      var ymEnumTypes = typeof(ApplicationDbContext).Assembly
          .GetTypes()
          .Where(t => t.IsEnum && t.Name.StartsWith("YM", StringComparison.Ordinal))
          .ToList();

      foreach (var enumType in ymEnumTypes)
      {
        // Создаём ValueConverter для данного enum-типа
        var converterType = typeof(EnumToStringConverter<>).MakeGenericType(enumType);
        var converter = (ValueConverter)Activator.CreateInstance(converterType);

        // Для каждой сущности ищем свойства этого enum-типа
        foreach (var entityType in mb.Model.GetEntityTypes())
        {
          var clrType = entityType.ClrType;
          var enumProperties = clrType
              .GetProperties()
              .Where(p => p.PropertyType == enumType);

          foreach (var prop in enumProperties)
          {
            mb
                .Entity(clrType)
                .Property(prop.Name)
                .HasConversion(converter);
          }
        }
      }

      //mb.Entity<YMSupplyRequestLocationAddress>(b =>
      //{
      //  // Расплющиваем owned type YMGps в колонки
      //  b.OwnsOne(e => e.Gps, gps =>
      //  {
      //    gps.Property(p => p.Latitude).HasColumnName("Latitude");
      //    gps.Property(p => p.Longitude).HasColumnName("Longitude");
      //  });

      //  // Составной ключ по двум колонкам
      //  b.HasKey(
      //      nameof(YMSupplyRequestLocationAddress.Gps) + "." + nameof(YMGps.Latitude),
      //      nameof(YMSupplyRequestLocationAddress.Gps) + "." + nameof(YMGps.Longitude)
      //  );
      //});
      mb.Entity<YMSupplyRequestLocationAddress>(b =>
      {
        b.HasKey(e => e.Id);
        b.HasIndex(e => new { e.Latitude, e.Longitude })
         .IsUnique();
      });

      mb.Entity<ReturnMainInfo>()
        .Property(r => r.ReturnStatus)
        .HasConversion<string>();

      // 1:1 Cabinet ↔ CabinetSettings
      mb.Entity<Cabinet>()
        .HasOne(c => c.Settings)
        .WithOne(s => s.Cabinet)
        .HasForeignKey<CabinetSettings>(s => s.CabinetId);

      // 1:* CabinetSettings ↔ ConnectionParameter
      mb.Entity<ConnectionParameter>()
        .HasOne(p => p.CabinetSettings)
        .WithMany(s => s.ConnectionParameters)
        .HasForeignKey(p => p.CabinetSettingsId);

      // Many-to-Many Worker ↔ Cabinet через таблицу WorkerCabinets
      mb.Entity<Worker>()
        .HasMany(w => w.AssignedCabinets)
        .WithMany(c => c.AssignedWorkers)
        .UsingEntity<Dictionary<string, object>>(
          "WorkerCabinets",
          wc => wc
            .HasOne<Cabinet>()
            .WithMany()
            .HasForeignKey("CabinetId")
            .OnDelete(DeleteBehavior.Cascade),
          wc => wc
            .HasOne<Worker>()
            .WithMany()
            .HasForeignKey("WorkerId")
            .OnDelete(DeleteBehavior.Cascade),
          wc =>
          {
            wc.HasKey("WorkerId", "CabinetId");
            wc.ToTable("WorkerCabinets");
          }
        );
    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
      if (!optionsBuilder.IsConfigured)
      {



        optionsBuilder.UseMySql(Program.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 21)));
      }
    }

  }
}