﻿namespace HabitTracker.Data.Data
{
  /// <summary>
  /// Данные для работы приложения.
  /// </summary>
  static public class BotConfigManager
  {
    /// <summary>
    /// Конфигурация приложения.
    /// </summary>
    public static Config ConfigApp { get; set; }

    /// <summary>
    /// Данные, загруженные из файла конфигурации приложения.
    /// </summary>
    static BotConfigManager()
    {
      ConfigApp = Config.LoadFromFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.yaml"));
    }
  }
}
