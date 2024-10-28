using HabitTracker.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HabitTracker.Data.Configurations
{
  public class HabitConfiguration : IEntityTypeConfiguration<HabitEntity>
  {
    public void Configure(EntityTypeBuilder<HabitEntity> builder)
    {
      builder.HasKey(x => x.Id);

      builder.
        HasOne(h => h.User);
    }
  }
}
