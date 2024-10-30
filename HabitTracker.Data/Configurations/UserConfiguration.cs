using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HabitTracker.Data.Configurations
{
  public class UserConfiguration : IEntityTypeConfiguration<UserEntity>
  {
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
