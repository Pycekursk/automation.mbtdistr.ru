using automation.mbtdistr.ru.Models;
using automation.mbtdistr.ru.Services.Wildberries.Models;
using automation.mbtdistr.ru.Services.YandexMarket;
using automation.mbtdistr.ru.Services.YandexMarket.Models;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
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

    public DbSet<Product> Products { get; set; }

    public DbSet<ProductBarcode> ProductBarcodes { get; set; }

    public DbSet<Cabinet> Cabinets { get; set; }
    public DbSet<CabinetSettings> CabinetSettings { get; set; }
    public DbSet<ConnectionParameter> ConnectionParameters { get; set; }

    public DbSet<ReturnImage> ReturnImages { get; set; }

    public DbSet<ReturnProduct> ReturnProducts { get; set; }

    public DbSet<Warehouse> Warehouses { get; set; }

    public DbSet<Compensation> Compensations { get; set; }

    //public DbSet<ReturnMainInfo> ReturnMainInfo { get; set; }

    public DbSet<WBOrder> WBOrders { get; set; }

    public DbSet<Order> Orders { get; set; }

    public DbSet<YMSupplyRequest> YMSupplyRequests { get; set; }

    public DbSet<YMSupplyRequestLocation> YMSupplyRequestLocations { get; set; }

    public DbSet<YMSupplyRequestLocation> YMLocations { get; set; }

    public DbSet<YMSupplyRequestLocationAddress> YMLocationAddresses { get; set; }

    public DbSet<YMSupplyRequestItem> YMSupplyRequestItems { get; set; }

    public DbSet<YMSupplyRequestReference> YMSupplyRequestReferences { get; set; }

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

      ymEnumTypes.Add(typeof(SellScheme));
      ymEnumTypes.Add(typeof(ReturnType));

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


      modelBuilder.ApplyConfiguration(new ReturnConfiguration());
      modelBuilder.ApplyConfiguration(new YMOrderConfiguration());
      modelBuilder.ApplyConfiguration(new YMOrderBuyerConfiguration());
      modelBuilder.ApplyConfiguration(new YMOrderDeliveryConfiguration());
      modelBuilder.ApplyConfiguration(new YMOrderItemConfiguration());

      // ─── Конфигурация элементов заявки ──────────────────────────

      modelBuilder.Entity<YMSupplyRequestItem>(entity =>
      {
        // Item → Request (Cascade при удалении заявки)
        entity.HasOne(i => i.SupplyRequest)
            .WithMany(r => r.Items)
            .HasForeignKey(i => i.SupplyRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        // Item → Price (Cascade при удалении Item)
        entity.HasOne(i => i.Price)
            .WithOne(p => p.SupplyRequestItem)
            .HasForeignKey<YMCurrencyValue>(p => p.YMSupplyRequestItemId)
            .OnDelete(DeleteBehavior.Cascade);

        // Item → Counters (Cascade при удалении Item)
        entity.HasOne(i => i.Counters)
            .WithOne(c => c.Item)
            .HasForeignKey<YMSupplyRequestItemCounters>(c => c.Id)
            .OnDelete(DeleteBehavior.Cascade);
      });

      // ─── Конфигурация локаций заявки ───────────────────────────

      // Location → Address (Restrict)
      modelBuilder.Entity<YMSupplyRequestLocation>()
          .HasOne(l => l.Address)
          .WithMany(a => a.LocationAddresses)
          .HasForeignKey(l => l.AddressId)
          .OnDelete(DeleteBehavior.Restrict);

      // Request → TargetLocation (Restrict)
      modelBuilder.Entity<YMSupplyRequest>()
          .HasOne(r => r.TargetLocation)
          .WithMany(l => l.AsTargetInRequests)
          .HasForeignKey(r => r.TargetLocationServiceId)
          .OnDelete(DeleteBehavior.Restrict);

      // Request → TransitLocation (Restrict)
      modelBuilder.Entity<YMSupplyRequest>()
          .HasOne(r => r.TransitLocation)
          .WithMany(l => l.AsTransitInRequests)
          .HasForeignKey(r => r.TransitLocationServiceId)
          .OnDelete(DeleteBehavior.Restrict);

      // 1:1 Cabinet ↔ CabinetSettings
      modelBuilder.Entity<Cabinet>()
        .HasOne(c => c.Settings)
        .WithOne(s => s.Cabinet)
        .HasForeignKey<CabinetSettings>(s => s.CabinetId);

      // 1:* Cabinet ↔ Order
      modelBuilder.Entity<Order>()
        .HasOne(o => o.Cabinet)
        .WithMany(c => c.Orders)
        .HasForeignKey(o => o.CabinetId)
        .OnDelete(DeleteBehavior.Cascade);

      // 1:* Cabinet ↔ Return
      modelBuilder.Entity<Return>()
        .HasOne(r => r.Cabinet)
        .WithMany(c => c.Returns)
        .HasForeignKey(r => r.CabinetId)
        .OnDelete(DeleteBehavior.Cascade);

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

      modelBuilder.Entity<Product>()
      .ToTable("Products");

      modelBuilder.Entity<ProductBarcode>()
          .ToTable("ProductBarcodes")
          .HasIndex(x => new { x.ProductId, x.Barcode })
          .IsUnique();

      // Опционально: настроить каскадное удаление
      modelBuilder.Entity<Product>()
          .HasMany(p => p.Barcodes)
          .WithOne(b => b.Product)
          .HasForeignKey(b => b.ProductId)
          .OnDelete(DeleteBehavior.Cascade);
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

  public class ReturnConfiguration : IEntityTypeConfiguration<Return>
  {
    public void Configure(EntityTypeBuilder<Return> builder)
    {
      builder
      .HasOne(r => r.CurrentWarehouse)
      .WithMany(w => w.CurrentReturns)
      .HasForeignKey(r => r.CurrentWarehouseId)
      .HasConstraintName("FK_Returns_CurrentWarehouse")
      .OnDelete(DeleteBehavior.Restrict);

      builder
          .HasOne(r => r.TargetWarehouse)
          .WithMany(w => w.DestinationReturns)
          .HasForeignKey(r => r.TargetWarehouseId)
          .HasConstraintName("FK_Returns_DestinationWarehouse")
          .OnDelete(DeleteBehavior.Restrict);

      builder.HasIndex(r => r.ReturnId).HasDatabaseName("IX_Returns_ReturnId");
    }
  }
}