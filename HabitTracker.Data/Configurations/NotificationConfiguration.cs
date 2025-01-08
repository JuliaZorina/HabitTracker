using HabitTracker.Data.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace HabitTracker.Data.Configurations
{
  public  class NotificationConfiguration : IEntityTypeConfiguration<NotificationEntity>
  {
    public void Configure(EntityTypeBuilder<NotificationEntity> builder)
    {
      builder.HasKey(x => x.Id);

      builder.HasOne(x => x.User);
      builder
        .HasMany(x => x.HabitNotifications)
        .WithOne(n => n.Notification)
        .HasForeignKey(n => n.UserNotificationsId);

    }
  }
}
