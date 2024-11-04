namespace HabitTracker.Data
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
    public string Title { get; set; }
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
    public DateOnly? LastExecutionDate { get; set; }
    /// <summary>
    /// Обзязательность ежедневного выполнения.
    /// </summary>
    public bool IsNecessary { get; set; }
    /// <summary>
    /// Срок выполнения привычки.
    /// </summary>
    public DateTime? ExpirationDate {  get; set; }
    /// <summary>
    /// Количество раз выполнения привычки за день.
    /// </summary>
    public int NumberOfExecutions { get; set; }
    /// <summary>
    /// Приостановлена привычка или нет.
    /// </summary>
    public bool IsSuspended { get; set; } 

    #region Конструкторы
    public HabitEntity() { }

    public HabitEntity(Guid id, string name, DateOnly? lastDay, HabitStatus status, long progressDays, DateTime? expirationDate,
      int numberOfExecutions, bool isSuspeded)
    {
      this.Id = id;
      this.Title = name;
      this.LastExecutionDate = lastDay;
      this.Status = status;
      this.ProgressDays = progressDays;
      this.ExpirationDate = expirationDate;
      this.NumberOfExecutions = numberOfExecutions;
      this.IsSuspended = isSuspeded;
    }

    public HabitEntity(Guid id, Guid userId, string name, int numberOfExecutions, DateTime? expirationDate, bool isNecessary)
    {
      this.Id = id;
      this.UserId = userId;
      this.Title = name;
      this.CreationDate = DateOnly.FromDateTime(DateTime.UtcNow);
      this.LastExecutionDate = null;
      this.Status = HabitStatus.Undone;
      this.ProgressDays = 0;
      this.IsSuspended = false;
      this.NumberOfExecutions = numberOfExecutions;
      this.ExpirationDate = expirationDate;
      this.IsNecessary = isNecessary;
    }
    #endregion
  }
  /// <summary>
  /// Статусы привычки.
  /// </summary>
  public enum HabitStatus
  {
    /// <summary>
    /// Привычка выполнена.
    /// </summary>
    Done,
    /// <summary>
    /// Привычка выполняется.
    /// </summary>
    InProgress,
    /// <summary>
    /// Привычка не выполнена
    /// </summary>
    Undone
  } 
}
