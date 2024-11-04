using HabitTracker.Data.Configurations;
using HabitTracker.Data.Data;
using HabitTracker.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace HabitTracker.Data
{
  /// <summary>
  /// Контекст базы данных для управления сущностями приложения HabitTracker.
  /// </summary>
  public class HabitTrackerContext : DbContext
  {
    /// <summary>
    /// Поток для записи логов в файл.
    /// </summary>
    private StreamWriter logStream = new StreamWriter("mylog.txt", true);
    /// <summary>
    /// Коллекция пользователей в базе данных.
    /// </summary>
    public DbSet<UserEntity> Users { get; set; }
    /// <summary>
    /// Коллекция привычек в базе данных.
    /// </summary>
    public DbSet<HabitEntity> Habits { get; set; }
    /// <summary>
    /// Коллекция записей о выполняемых привычках в базе данных.
    /// </summary>
    public DbSet<PracticedHabitEntity> PracticedHabits { get; set; }
    /// <summary>
    /// Коллекция настроек уведомлений для конкретной привычки пользователя.
    /// </summary>
    public DbSet<HabitNotificationEntity> HabitsNotificationSettings { get; set; }

    /// <summary>
    /// Настройки модели базы данных.
    /// </summary>
    /// <param name="modelBuilder">Объект для настройки модели.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      modelBuilder.ApplyConfiguration(new UserConfiguration());
      modelBuilder.ApplyConfiguration(new HabitConfiguration());
      modelBuilder.ApplyConfiguration(new PracticedHabitConfiguration());
      modelBuilder.ApplyConfiguration(new HabitNotificationEntityConfiguration());

      base.OnModelCreating(modelBuilder);
    }

    /// <summary>
    /// Конфигурация параметров контекста базы данных.
    /// </summary>
    /// <param name="optionsBuilder">Объект для настройки параметров.</param>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
      optionsBuilder.UseNpgsql(BotConfigManager.ConfigApp.DatabaseConnectionString);
      optionsBuilder.LogTo(logStream.WriteLine, new[] { DbLoggerCategory.Database.Command.Name });
    }

    /// <summary>
    /// Освобождение ресурсов, связанных с логированием.
    /// </summary>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    public override async ValueTask DisposeAsync()
    {
      await base.DisposeAsync();
      await logStream.DisposeAsync();
    }

    #region Конструторы

    public HabitTrackerContext(DbContextOptions<HabitTrackerContext> options) 
      :base(options)
    {
      //Database.EnsureDeleted();
      //Database.EnsureCreated();
    }

    #endregion
  }
}
