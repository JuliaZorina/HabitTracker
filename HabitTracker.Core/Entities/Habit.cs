using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HabitTracker.Core.Entities
{
  /// <summary>
  /// Сущность привычки.
  /// </summary>
  public class Habit
  {
    /// <summary>
    /// Уникальный идентификатор привычки.
    /// </summary>
    public int Id { get; set; } 
    /// <summary>
    /// Уникальный идентификатор владельца привычки.
    /// </summary>
    public int UserId { get; set; }
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
