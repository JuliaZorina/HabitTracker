using System.Diagnostics;

namespace HabitTracker.Data.Models
{
  public class HabitNotificationEntity : NotificationEntity
  {   
    /// <summary>
    /// Уникальный идентификатор привычки.
    /// </summary>
    public Guid HabitId { get; set; }
    /// <summary>
    /// Экземпляр класса HabitEntity - владелец настроек уведомлений конкретной привычки.
    /// </summary>
    public HabitEntity Habit { get; set; }
    /// <summary>
    /// Отправлять уведомления или нет.
    /// </summary>
    public bool IsSending {  get; set; }
    /// <summary>
    /// Количество уведомлений в день.
    /// </summary>
    public int CountOfNotifications {  get; set; }
    /// <summary>
    /// Коллекция со временем отправки уведомлений.
    /// </summary>
    public List<TimeOnly> NotificationTime
    {
      get { return this.NotificationTime; }
      set
      {
        var period = (this.TimeEnd - this.TimeStart) / this.CountOfNotifications;
        for (int i = 0; i < this.CountOfNotifications; i++)
        {
          if (i == 0)
          {
            this.NotificationTime.Add(TimeStart);
          }
          else
          {
            NotificationTime.Add(TimeStart.AddHours(period.TotalHours));
          }
        }
      }
    }
    
    public HabitNotificationEntity(Guid id, Guid userId, TimeOnly timeStart, TimeOnly timeEnd, Guid habitId, bool isSending, int countOfNotifications) 
      : base(id, userId, timeStart, timeEnd)
    {
      this.HabitId = habitId; 
      this.IsSending = isSending;
      this.CountOfNotifications = countOfNotifications;      
    }
  }
}
