using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    public Guid HabiitId { get; set; }
    /// <summary>
    /// Ссылка на объект класса HabitEntity - выполняемой привычки.
    /// </summary>
    public HabitEntity Habit { get; set; }
    /// <summary>
    /// Дата и время выполнения привычки.
    /// </summary>
    public DateTime LastExecutionDate { get; set; }
  }
}
