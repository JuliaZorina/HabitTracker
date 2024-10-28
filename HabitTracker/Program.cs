using HabitTracker.Core;
using HabitTracker.Data;
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
      
    }
  }
}
