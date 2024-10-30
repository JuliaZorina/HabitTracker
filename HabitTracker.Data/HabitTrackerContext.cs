using HabitTracker.Data.Configurations;
using HabitTracker.Data.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Npgsql.EntityFrameworkCore.PostgreSQL;

namespace HabitTracker.Data
{
  public class HabitTrackerContext : DbContext
  {
    private readonly StreamWriter logStream = new StreamWriter("mylog.txt", true);

    public DbSet<UserEntity> Users { get; set; }
    public DbSet<HabitEntity> Habits { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      modelBuilder.ApplyConfiguration(new UserConfiguration());
      modelBuilder.ApplyConfiguration(new HabitConfiguration());

      base.OnModelCreating(modelBuilder);
    }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
      optionsBuilder.UseNpgsql(ApplicationData.ConfigApp.DatabaseConnectionString);
      //optionsBuilder.LogTo(Console.WriteLine, new[] { RelationalEventId.CommandExecuted });
      optionsBuilder.LogTo(Console.WriteLine, new[] { DbLoggerCategory.Database.Command.Name });
      optionsBuilder.LogTo(logStream.WriteLine, Microsoft.Extensions.Logging.LogLevel.Error);
    }

    public override async ValueTask DisposeAsync()
    {
      await base.DisposeAsync();
      await logStream.DisposeAsync();
    }
    
    #region Конструторы

    public HabitTrackerContext(DbContextOptions<HabitTrackerContext> options) 
      :base(options)
    {
      //Database.EnsureDeleted(); // гарантируем, что бд удалена
      //Database.EnsureCreated();   // гарантируем, что БД создана
    }

    #endregion
  }
}
