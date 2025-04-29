using automation.mbtdistr.ru.Models;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

using System;
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
    public DbSet<AuditLog> AuditLogs { get; set; }

    public DbSet<ReturnMainInfo> ReturnMainInfo { get; set; }

    public DbSet<CallbackAction> CallbackActions { get; set; }

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