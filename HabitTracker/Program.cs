using HabitTracker.Data.Data;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace HabitTracker
{
  /// <summary>
  /// Главный класс программы, содержащий точку входа.
  /// </summary>
  internal class Program
  {
    /// <summary>
    /// Точка входа в приложение.
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    static async Task Main(string[] args)
    {
      Console.WriteLine("Отладка: Программа запущена");

      try
      {
        var config = ApplicationData.ConfigApp;

        Console.WriteLine("Конфигурация успешно загружена");
        Console.WriteLine($"Токен бота: {config.BotToken}");
        var dbContextFactory = new DbContextFactory();
        var dbContext = dbContextFactory.CreateDbContext(args);

        await dbContext.Database.MigrateAsync();

        var botHandler = new TelegramBotHandler(config.BotToken, dbContext);
        await botHandler.StartBotAsync();

        await Task.Delay(-1);
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Отладка: Произошла ошибка - {ex.Message}");
        Debug.WriteLine($"Отладка: Подробности исключения - {ex}");
      }
      finally
      {
        var config = ApplicationData.ConfigApp;

        Console.WriteLine("Конфигурация успешно загружена");
        Console.WriteLine($"Токен бота: {config.BotToken}");
        var dbContextFactory = new DbContextFactory();
        var dbContext = dbContextFactory.CreateDbContext(args);

        await dbContext.Database.MigrateAsync();

        var botHandler = new TelegramBotHandler(config.BotToken, dbContext);
        await botHandler.StartBotAsync();

        await Task.Delay(-1);
      }
      Console.WriteLine("Отладка: Программа завершена");

      Console.WriteLine("Нажмите любую клавишу для выхода...");
      Console.ReadKey();
    }
  }
}
