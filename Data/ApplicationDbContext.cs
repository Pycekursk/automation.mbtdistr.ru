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

    public DbSet<ReturnImage> ReturnImages { get; set; }

    public DbSet<ReturnProduct> ReturnProducts { get; set; }

    public DbSet<Warehouse> Warehouses { get; set; }

    public DbSet<automation.mbtdistr.ru.Models.Address> Addresses { get; set; }

    public DbSet<Compensation> Compensations { get; set; }

    //public DbSet<ReturnMainInfo> ReturnMainInfo { get; set; }

    public DbSet<YMSupplyRequest> YMSupplyRequests { get; set; }

    public DbSet<YMSupplyRequestLocation> YMSupplyRequestLocations { get; set; }

    public DbSet<YMSupplyRequestLocation> YMLocations { get; set; }

    public DbSet<YMSupplyRequestLocationAddress> YMLocationAddresses { get; set; }

    public DbSet<YMSupplyRequestItem> YMSupplyRequestItems { get; set; }

    public DbSet<YMSupplyRequestReference> YMSupplyRequestReferences { get; set; }

    public DbSet<YMOrder> YMOrders { get; set; }

    public DbSet<YMOrderBuyer> YMOrderBuyers { get; set; }

    public DbSet<YMOrderDelivery> YMOrderDeliveries { get; set; }

    public DbSet<YMOrderItem> YMOrderItems { get; set; }

    public DbSet<YMCurrencyValue> YMCurrencyValues { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      base.OnModelCreating(modelBuilder);
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
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
          var clrType = entityType.ClrType;
          var enumProperties = clrType
              .GetProperties()
              .Where(p => p.PropertyType == enumType);

          foreach (var prop in enumProperties)
          {
            modelBuilder
                .Entity(clrType)
                .Property(prop.Name)
                .HasConversion(converter);
          }
        }
      }

      modelBuilder.ApplyConfiguration(new YMOrderConfiguration());
      modelBuilder.ApplyConfiguration(new YMOrderBuyerConfiguration());
      modelBuilder.ApplyConfiguration(new YMOrderDeliveryConfiguration());
      modelBuilder.ApplyConfiguration(new YMOrderItemConfiguration());

      // 1. Один-ко-многим: заявка → дочерние ссылки
      modelBuilder.Entity<YMSupplyRequestReference>()
          .HasOne(rf => rf.Request)
          .WithMany(r => r.ChildrenLinks)
          .HasForeignKey(rf => rf.RequestId)
          .OnDelete(DeleteBehavior.Cascade);

      modelBuilder.Entity<YMSupplyRequestReference>()
      .HasOne(rf => rf.RelatedRequest)
      .WithOne(r => r.ParentLink)
      .HasForeignKey<YMSupplyRequestReference>(rf => rf.RelatedRequestId)
      .IsRequired(false)                            // optional
      .OnDelete(DeleteBehavior.Cascade);

      modelBuilder.Entity<YMSupplyRequestItem>()
          .HasOne(i => i.Price)
          .WithOne(p => p.SupplyRequestItem)
          .HasForeignKey<YMCurrencyValue>(p => p.YMSupplyRequestItemId)
          .OnDelete(DeleteBehavior.Cascade);

      modelBuilder.Entity<YMSupplyRequestItem>()
          .HasOne(i => i.Counters)
          .WithOne(c => c.Item)
          .HasForeignKey<YMSupplyRequestItemCounters>(c => c.Id) // при совпадении ID с item.Id
          .OnDelete(DeleteBehavior.Cascade);

      modelBuilder.Entity<YMSupplyRequest>()
 .HasMany(r => r.Items)
 .WithOne(i => i.SupplyRequest)
 .HasForeignKey(i => i.SupplyRequestId)
 .OnDelete(DeleteBehavior.Cascade);

      // 1) Один-ко-многим: YMSupplyRequest → ChildrenLinks
      modelBuilder.Entity<YMSupplyRequest>()
          .HasMany(r => r.ChildrenLinks)
          .WithOne(rf => rf.Request)
          .HasForeignKey(rf => rf.RequestId)
          .OnDelete(DeleteBehavior.Cascade);

      // 2) Один-к-одному (опционально): YMSupplyRequest → ParentLink
      modelBuilder.Entity<YMSupplyRequest>()
          .HasOne(r => r.ParentLink)
          .WithOne(rf => rf.RelatedRequest)
          .HasForeignKey<YMSupplyRequestReference>(rf => rf.RelatedRequestId)
          .OnDelete(DeleteBehavior.Restrict);

      // ─── SupplyRequest → TargetLocation ────────
      modelBuilder.Entity<YMSupplyRequest>()
          .HasOne(r => r.TargetLocation)
          .WithMany(l => l.AsTargetInRequests)
          .HasForeignKey(r => r.TargetLocationServiceId)
          .OnDelete(DeleteBehavior.Restrict);

      // ─── SupplyRequest → TransitLocation ───────
      modelBuilder.Entity<YMSupplyRequest>()
          .HasOne(r => r.TransitLocation)
          .WithMany(l => l.AsTransitInRequests)
          .HasForeignKey(r => r.TransitLocationServiceId)
          .OnDelete(DeleteBehavior.Restrict);

      // ─── Location → Address ────────────────────
      modelBuilder.Entity<YMSupplyRequestLocation>()
          .HasOne(l => l.Address)
          .WithMany(a => a.LocationAddresses)
          .HasForeignKey(l => l.AddressId)
          .OnDelete(DeleteBehavior.Restrict);

      modelBuilder.Entity<ReturnMainInfo>()
        .Property(r => r.ReturnStatus)
        .HasConversion<string>();

      // 1:1 Cabinet ↔ CabinetSettings
      modelBuilder.Entity<Cabinet>()
        .HasOne(c => c.Settings)
        .WithOne(s => s.Cabinet)
        .HasForeignKey<CabinetSettings>(s => s.CabinetId);

      // 1:* CabinetSettings ↔ ConnectionParameter
      modelBuilder.Entity<ConnectionParameter>()
        .HasOne(p => p.CabinetSettings)
        .WithMany(s => s.ConnectionParameters)
        .HasForeignKey(p => p.CabinetSettingsId);

      // Many-to-Many Worker ↔ Cabinet через таблицу WorkerCabinets
      modelBuilder.Entity<Worker>()
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
        optionsBuilder.EnableDetailedErrors();
        optionsBuilder.EnableSensitiveDataLogging();

        if (Program.Environment.IsDevelopment())
        {
          optionsBuilder.UseMySql(Program.Configuration.GetConnectionString("DebugConnection"),
            new MySqlServerVersion(new Version(8, 0, 21)));
        }
        else
        // получаем строку подключения DefaultConnection
        {
          optionsBuilder.UseMySql(Program.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 21)));
        }
      }

    }
  }
}