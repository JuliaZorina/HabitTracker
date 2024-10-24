using HabitTracker.Core;
using HabitTracker.Core.Entities;
using HabitTracker.Data.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace HabitTracker.Data.Context
{
  public class HabitTrackerContext : DbContext
  {
    public DbSet<User> Users => Set<User>();
    public DbSet<Habit> Habits => Set<Habit>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
      optionsBuilder.UseNpgsql(ApplicationData.ConfigApp.DatabaseConnectionString);
    }

    #region Конструторы

    public HabitTrackerContext()
    {
      Database.EnsureDeleted(); // гарантируем, что бд удалена
      Database.EnsureCreated();   // гарантируем, что БД создана
    }

    #endregion
  }
}
