﻿namespace HabitTracker.Data
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

    #region Конструкторы
    public HabitEntity() { }

    public HabitEntity(Guid id, string name, DateOnly lastDay, HabitStatus status, long progressDays)
    {
      this.Id = id;
      this.Title = name;
      this.LastExecutionDate = lastDay;
      this.Status = status;
      this.ProgressDays = progressDays;
    }

    public HabitEntity(Guid id, Guid userId, string name)
    {
      this.Id = id;
      this.UserId = userId;
      this.Title = name;
      this.CreationDate = DateOnly.FromDateTime(DateTime.UtcNow);
      this.LastExecutionDate = null;
      this.Status = HabitStatus.Undone;
      this.ProgressDays = 0;
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
