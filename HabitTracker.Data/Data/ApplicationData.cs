namespace HabitTracker.Data.Data
{
  static public class ApplicationData
  {
    public static Config ConfigApp { get; set; }

    static ApplicationData()
    {
      ConfigApp = Config.LoadFromFile("config.yaml");
    }
  }
}
