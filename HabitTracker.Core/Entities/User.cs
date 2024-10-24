namespace HabitTracker.Core
{
  /// <summary>
  /// Сущность пользователя системы.
  /// </summary>
  public class User
  {
    /// <summary>
    /// Уникальный идентификатор пользователя.
    /// </summary>
    public int Id { get; set; }
    /// <summary>
    /// Имя пользователя.
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// Уникальный идентификатор чата пользователя.
    /// </summary>
    public long ChatId { get; set; }
  }
}
