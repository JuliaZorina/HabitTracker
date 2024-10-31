namespace HabitTracker.Data.Data
{
  /// <summary>
  /// Данные для работы приложения.
  /// </summary>
  static public class ApplicationData
  {
    /// <summary>
    /// Конфигурация приложения.
    /// </summary>
    public static Config ConfigApp { get; set; }

    /// <summary>
    /// Данные, загруженные из файла конфигурации приложения.
    /// </summary>
    static ApplicationData()
    {
      ConfigApp = Config.LoadFromFile("config.yaml");
    }
  }
}
