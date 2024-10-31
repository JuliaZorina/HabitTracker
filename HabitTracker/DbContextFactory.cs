using HabitTracker.Data;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace HabitTracker
{
  /// <summary>
  /// Фабрика для создания экземпляров HabitTrackerContext.
  /// </summary>
  public class DbContextFactory : IDesignTimeDbContextFactory<HabitTrackerContext>
  {
    /// <summary>
    /// Метод для создания контекста базы данных.
    /// </summary>
    /// <param name="args">Аргументы командной строки.</param>
    /// <returns>Экземпляр контекста базы данных.</returns>
    public HabitTrackerContext CreateDbContext(string[] args)
    {
      var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("AppSettings.json", optional: true, reloadOnChange: true)
            .Build();

      DbContextOptionsBuilder<HabitTrackerContext> optionsBuilder = new DbContextOptionsBuilder<HabitTrackerContext>()
                .UseNpgsql(configuration.GetConnectionString("Postgre"));

      return new HabitTrackerContext(optionsBuilder.Options);
    }
  }
}
