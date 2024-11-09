using HabitTracker.Core;
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
    /// <param name="args">Аргументы командной строки.</param>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    static async Task Main(string[] args)
    {
      Console.WriteLine("Отладка: Программа запущена");
      try
      {
        var config = BotConfigManager.ConfigApp;

        Console.WriteLine("Конфигурация успешно загружена");
        Console.WriteLine($"Токен бота: {config.BotToken}");

        var dbContextFactory = new DbContextFactory();
        var dbContext = dbContextFactory.CreateDbContext(args);
        await dbContext.Database.MigrateAsync();

        var botHandler = new TelegramBotHandler(config.BotToken, dbContextFactory, args);
        await botHandler.StartBotAsync();

        await Task.Delay(-1);
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Отладка: Произошла ошибка - {ex.Message}");
        Debug.WriteLine($"Отладка: Подробности исключения - {ex}");
      }

      Console.WriteLine("Отладка: Программа завершена");
      Console.WriteLine("Нажмите любую клавишу...");
      Console.ReadKey();
    }
  }
}
