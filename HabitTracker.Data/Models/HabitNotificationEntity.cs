using System.Diagnostics;

namespace HabitTracker.Data.Models
{
  public class HabitNotificationEntity 
  {   
    /// <summary>
    /// Уникальный идентификатор настройки привычки.
    /// </summary>
    public Guid Id { get; set; }
    /// <summary>
    /// Уникальный идентификатор привычки.
    /// </summary>
    public Guid HabitId { get; set; }
    /// <summary>
    /// Экземпляр класса HabitEntity - владелец настроек уведомлений конкретной привычки.
    /// </summary>
    public HabitEntity Habit { get; set; }
    /// <summary>
    /// Уникальный идентификатор настроек уведомлений пользователя.
    /// </summary>
    public Guid UserNotificationsId { get; set; }
    /// <summary>
    /// Экземпляр класса Notification  - настроек уведомлений пользователя.
    /// </summary>
    public NotificationEntity Notification { get; set; }

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
    public List<TimeOnly> NotificationTime { get; set; } = [];
    public HabitNotificationEntity(Guid id, Guid habitId, Guid userNotificationsId, bool isSending, int countOfNotifications) 
    {
      this.Id = id;
      this.HabitId = habitId; 
      this.UserNotificationsId = userNotificationsId;  
      this.IsSending = isSending;
      this.CountOfNotifications = countOfNotifications;      
    }
  }
}
