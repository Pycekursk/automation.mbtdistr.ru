using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace automation.mbtdistr.ru.Data
{
  public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
  {
    public ApplicationDbContext CreateDbContext(string[] args)
    {
      // Пример: передаём "Production" или "Development"
      string environment = args.FirstOrDefault() ?? "Development";

      var config = new ConfigurationBuilder()
          .AddJsonFile("appsettings.json")
          .Build();

      var connectionString = environment == "Production"
          ? config.GetConnectionString("DefaultConnection")
          : config.GetConnectionString("DebugConnection");

      var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
      optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

      return new ApplicationDbContext(optionsBuilder.Options);
    }
  }
}