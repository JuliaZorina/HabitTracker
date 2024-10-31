using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HabitTracker.Data.Configurations
{
  /// <summary>
  /// Конфигурация таблицы пользователей в базе данных.
  /// </summary>
  public class UserConfiguration : IEntityTypeConfiguration<UserEntity>
  {
    /// <summary>
    /// Настройки таблицы пользователей в базе данных.
    /// </summary>
    /// <param name="builder">Объект для настройки сущности пользователя в базе данных.</param>
    public void Configure(EntityTypeBuilder<UserEntity> builder)
    {
      builder.HasKey(u => u.Id);

      builder
        .HasMany(u => u.Habbits)
        .WithOne(h => h.User)
        .HasForeignKey(h => h.UserId);
    }
  }
}
