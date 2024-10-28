namespace HabitTracker.Core.Entities
{
  /// <summary>
  /// Сущность привычки.
  /// </summary>
  public class HabitEntity
  {
    /// <summary>
    /// Уникальный идентификатор привычки.
    /// </summary>
    public Guid Id { get; set; } 
    /// <summary>
    /// Уникальный идентификатор владельца привычки.
    /// </summary>
    public Guid UserId { get; set; }
    /// <summary>
    /// Ссылка на объект класса UserEntity - владельца привычки.
    /// </summary>
    public UserEntity? User { get; set; }
    /// <summary>
    /// Название привычки
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// Статус привычки.
    /// </summary>
    public HabitStatus Status { get; set; }
    /// <summary>
    /// Продолжительность выполнения привычки в днях.
    /// </summary>
    public long ProgressDays {  get; set; }
    /// <summary>
    /// Дата создания привычки.
    /// </summary>
    public DateOnly CreationDate { get; set; }
    /// <summary>
    /// Дата последнего выполнения привычки.
    /// </summary>
    public DateOnly LastExecutionDate { get; set; }
  }
  /// <summary>
  /// Статусы привычки.
  /// </summary>
  public enum HabitStatus
  {
    /// <summary>
    /// Привычка выполняется.
    /// </summary>
    Active,
    /// <summary>
    /// Привычка удалена.
    /// </summary>
    Deleted
  } 
}
