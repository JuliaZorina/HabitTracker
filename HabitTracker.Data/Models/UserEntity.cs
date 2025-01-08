namespace HabitTracker.Data
{
  /// <summary>
  /// Сущность пользователя системы.
  /// </summary>
  public class UserEntity 
  {
    /// <summary>
    /// Уникальный идентификатор пользователя.
    /// </summary>
    public Guid Id { get; set; }
    /// <summary>
    /// Имя пользователя.
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// Уникальный идентификатор чата пользователя.
    /// </summary>
    public long ChatId { get; set; }
    /// <summary>
    /// Список привычек пользователя.
    /// </summary>
    public List<HabitEntity> Habbits { get; set; } = [];

    #region Конструкторы

    public UserEntity(string name, long chatId)
    {
      this.Name = name; 
      this.ChatId = chatId;
    }

    public UserEntity(Guid id, string name, long chatId)
    {
      this.Id = id;
      this.Name = name;
      this.ChatId = chatId;
    }

    #endregion
  }
}
