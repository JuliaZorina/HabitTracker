using HabitTracker.Data.Configurations;
using HabitTracker.Data.Data;
using Microsoft.EntityFrameworkCore;

namespace HabitTracker.Data
{
  public class HabitTrackerContext : DbContext
  {
    private StreamWriter logStream = new StreamWriter("mylog.txt", true);

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
      //TODO : заменить ApplicationData.ConfigApp.DatabaseConnectionString на считывание данных из AppSettings.json
      optionsBuilder.UseNpgsql(ApplicationData.ConfigApp.DatabaseConnectionString);
      optionsBuilder.LogTo(logStream.WriteLine, new[] { DbLoggerCategory.Database.Command.Name });
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
      //Database.EnsureDeleted();
      //Database.EnsureCreated();
    }

    #endregion
  }
}
