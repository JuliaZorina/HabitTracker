using HabitTracker.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HabitTracker.Data.Configurations
{
  public class HabitNotificationEntityConfiguration : IEntityTypeConfiguration<HabitNotificationEntity>

  {
    public void Configure(EntityTypeBuilder<HabitNotificationEntity> builder)
    {
      builder.HasKey(x => x.Id);

      builder.HasOne(x => x.Habit);
      builder
        .HasOne(x =>x.Notification)
        .WithMany(n => n.HabitNotifications)
        .HasForeignKey(n => n.UserNotificationsId);
      
    }
  }
}
