using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using HabitTracker.Data.Models;

namespace HabitTracker.Data.Configurations
{
  /// <summary>
  /// Конфигурация таблицы выполняемых привычек в базе данных.
  /// </summary>
  public class PracticedHabitConfiguration : IEntityTypeConfiguration<PracticedHabitEntity>
  {
    
    /// <summary>
    /// Настройки таблицы выполняемых привычек в базе данных.
    /// </summary>
    /// <param name="builder">Объект для настройки сущности привычки в базе данных.</param>
    public void Configure(EntityTypeBuilder<PracticedHabitEntity> builder)
    {
      builder.HasKey(x => x.Id);
      builder.
        HasOne(h => h.Habit);
    }
    
  }
}
