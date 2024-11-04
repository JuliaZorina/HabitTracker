namespace HabitTracker.Data.Models
{
  /// <summary>
  /// Сущность общих настроек уведомлений пользователя
  /// </summary>
  public class NotificationEntity
  {
    #region Поля и свойства

    /// <summary>
    /// Уникальный идентификатор настройки уведомлений пользователя.
    /// </summary>
    public Guid Id { get; set; }
    /// <summary>
    /// Уникальный идентификатор пользователя-владельца.
    /// </summary>
    public Guid UserId { get; set; }
    /// <summary>
    /// Ссылка на сущность UserEntity - владельца настроек привычки.
    /// </summary>
    public UserEntity User { get; set; }
    /// <summary>
    /// Время начала рассылки уведомлений.
    /// </summary>
    public TimeOnly TimeStart { get; set; }
    /// <summary>
    /// Время конца рассылки уведомлений.
    /// </summary>
    public TimeOnly TimeEnd { get; set; }
    #endregion

    #region Конструкторы
    public NotificationEntity(Guid id, Guid userId, TimeOnly timeStart, TimeOnly timeEnd)
    {
      this.Id = id;
      this.UserId = userId; 
      this.TimeStart = timeStart; 
      this.TimeEnd = timeEnd;
    }
    #endregion
  }
}
