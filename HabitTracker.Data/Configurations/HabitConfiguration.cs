using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HabitTracker.Data.Configurations
{
  /// <summary>
  /// Конфигурация таблицы привычек в базе данных.
  /// </summary>
  public class HabitConfiguration : IEntityTypeConfiguration<HabitEntity>
  {
    /// <summary>
    /// Настройки таблицы привычек в базе данных.
    /// </summary>
    /// <param name="builder">Объект для настройки сущности привычки в базе данных.</param>
    public void Configure(EntityTypeBuilder<HabitEntity> builder)
    {
      builder.HasKey(x => x.Id);

      builder.
        HasOne(h => h.User);
    }
  }
}
