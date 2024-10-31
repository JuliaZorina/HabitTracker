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
