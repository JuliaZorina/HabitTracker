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
    private StreamWriter? _logStream;
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
    public DbSet<NotificationEntity> NotificationSettings { get; set; }

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
      modelBuilder.ApplyConfiguration(new NotificationConfiguration());

      base.OnModelCreating(modelBuilder);
    }

    /// <summary>
    /// Конфигурация параметров контекста базы данных.
    /// </summary>
    /// <param name="optionsBuilder">Объект для настройки параметров.</param>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
      optionsBuilder.UseNpgsql(BotConfigManager.ConfigApp.DatabaseConnectionString);
      optionsBuilder.LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Warning);
      optionsBuilder.LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Error);
      optionsBuilder.LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Critical);
      try
      {
        if (_logStream == null)
        {
          var fileStream = new FileStream($"logs\\database_log.txt", FileMode.Append, FileAccess.Write, FileShare.Write);
          _logStream = new StreamWriter(fileStream) { AutoFlush = true };
        }
        // Логируем команды базы данных
        optionsBuilder.LogTo(Console.WriteLine, new[] { DbLoggerCategory.Database.Connection.Name });
        optionsBuilder.LogTo(_logStream.WriteLine, new[] { DbLoggerCategory.Database.Command.Name });
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Ошибка записи в лог: {ex.Message}");
      }

    }

    /// <summary>
    /// Освобождение ресурсов, связанных с логированием.
    /// </summary>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    public override async ValueTask DisposeAsync()
    {
      if (_logStream != null)
      {
        await _logStream.DisposeAsync(); 
        _logStream = null; 
      }
      await base.DisposeAsync();
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
