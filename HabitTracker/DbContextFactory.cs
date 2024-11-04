using HabitTracker.Data;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
using HabitTracker.Data.Data;
using System.Diagnostics;

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
     // Debugger.Launch();
      DbContextOptionsBuilder<HabitTrackerContext> optionsBuilder = new DbContextOptionsBuilder<HabitTrackerContext>()
                .UseNpgsql(BotConfigManager.ConfigApp.DatabaseConnectionString);

      return new HabitTrackerContext(optionsBuilder.Options);
    }
  }
}
