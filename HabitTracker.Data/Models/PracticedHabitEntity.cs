using System.ComponentModel.DataAnnotations.Schema;

namespace HabitTracker.Data.Models
{
  /// <summary>
  /// Сущность привычки, которая выполняется.
  /// </summary>
  public class PracticedHabitEntity
  {
    /// <summary>
    /// Уникальный идентификатор записи о выполнении привычки.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Уникальный идентификатор выполненной  привычки.
    /// </summary>
    public Guid HabitId { get; set; }
    /// <summary>
    /// Ссылка на объект класса HabitEntity - выполняемой привычки.
    /// </summary>
    public HabitEntity Habit { get; set; }
    /// <summary>
    /// Дата и время выполнения привычки.
    /// </summary>
    [Column(TypeName = "timestamp")]
    public DateTime LastExecutionDate { get; set; }

    #region Конструкторы
    public PracticedHabitEntity(Guid id, Guid habitId, DateTime lastExecutionDate)
    {
      this.Id = id;
      this.HabitId = habitId;
      this.LastExecutionDate = lastExecutionDate;
    }
    #endregion
  }
}
