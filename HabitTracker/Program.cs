using HabitTracker.Core;
using HabitTracker.Core.Entities;
using HabitTracker.Data;
using HabitTracker.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HabitTracker
{
  internal class Program : IDesignTimeDbContextFactory<HabitTrackerContext>
  {
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
    static void Main(string[] args)
    {
      /*  Тестовые данные.
      var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("AppSettings.json", optional: true, reloadOnChange: true)
            .Build();

      DbContextOptions<HabitTrackerContext> optionsBuilder = new DbContextOptionsBuilder<HabitTrackerContext>()
                .UseNpgsql(configuration.GetConnectionString("Postgre")).Options;
      using (HabitTrackerContext dbContext = new HabitTrackerContext(optionsBuilder)) {
        var habitsRepo = new HabitsRepository(dbContext);
        List<HabitEntity> habits = habitsRepo.Get().GetAwaiter().GetResult();

        // Используйте список habits
        foreach (var habit in habits)
        {
          Console.WriteLine($"{habit.Id} {habit.Name}");
        }

        var userssRepo = new UsersRepository(dbContext);
        UserEntity user = userssRepo.GetById(Guid.Parse("9b08a27f-92eb-4ae8-9893-e21f41e3c277")).GetAwaiter().GetResult();

        Console.WriteLine($"{user.Id} {user.Name}");
        
      }*/
    }
  }
}
