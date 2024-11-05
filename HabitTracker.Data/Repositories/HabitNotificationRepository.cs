using HabitTracker.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace HabitTracker.Data.Repositories
{
  public class HabitNotificationRepository
  {
    #region Поля и свойства

    /// <summary>
    /// Контекст базы данных.
    /// </summary>
    private readonly HabitTrackerContext _dbContext;

    #endregion

    /// <summary>
    /// Получить список всех настроек уведомлений.
    /// </summary>
    /// <returns>Асинхронно возвращает коллекцию всех настроек уведомлений для привычек в базе данных.</returns>
    public async Task<List<HabitNotificationEntity>> Get()
    {
      return await _dbContext.HabitsNotificationSettings
        .AsNoTracking()
        .ToListAsync();
    }

    /// <summary>
    /// Получить настройки уведомлений для привычки пользователя по уникальному идентификатору настройки.
    /// </summary>
    /// <returns>Асинхронно настройки уведомлений пользователя для привычки, найденные по ее уникальному идентификатору.</returns>
    public async Task<HabitNotificationEntity?> GetById(Guid id)
    {
      return await _dbContext.HabitsNotificationSettings
        .AsNoTracking()
        .FirstOrDefaultAsync(h => id == h.Id);
    }

    /// <summary>
    /// Получить настройки уведомлений для привычки пользователя по уникальному идентификатору привычки.
    /// </summary>
    /// <returns>Асинхронно настройки уведомлений пользователя для привычки, найденные по уникальному идентификатору привычки.</returns>
    public async Task<HabitNotificationEntity?> GetByHabitId(Guid habitId)
    {
      return await _dbContext.HabitsNotificationSettings
        .AsNoTracking()
        .FirstOrDefaultAsync(n => habitId == n.HabitId);
    }
    /// <summary>
    /// Получить настройки уведомлений для привычек пользователя по уникальному идентификатору общих настроек.
    /// </summary>
    /// <returns>Асинхронно получает коллекцию настроек уведомлений пользователя для привычек, 
    /// найденных по уникальному идентификатору общих настроек уведомлений.</returns>
    public async Task<List<HabitNotificationEntity>?> GetByNotificationSettingsId(Guid notificationSettingsId)
    {
      return await _dbContext.HabitsNotificationSettings
        .AsNoTracking()
        .Where(n => notificationSettingsId == n.UserNotificationsId)
        .ToListAsync();
    }

    /// <summary>
    /// Добавить настройки уведомлений новой привычки в базу данных.
    /// </summary>
    /// <param name="habitNotification">Настройки уведомлений для привычки пользователя.</param>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    public async Task Add(HabitNotificationEntity habitNotification)
    {
      var notification = await _dbContext.NotificationSettings.FirstOrDefaultAsync(u => u.Id == habitNotification.UserNotificationsId)
        ?? throw new ArgumentNullException("Не найдены настройки уведомлений с указанным Id.");
      var newHabitNotification = new HabitNotificationEntity(habitNotification.Id, habitNotification.HabitId, habitNotification.UserNotificationsId,
        habitNotification.IsSending, habitNotification.CountOfNotifications);

      notification.HabitNotifications.Add(newHabitNotification);

      await _dbContext.AddAsync(habitNotification);
      await _dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Обновить настроки уведомлений для привычки.
    /// </summary>
    /// <param name="habitNotification">Экземпляр класса настроек уведомлений привычки.</param>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    public async Task Update(HabitNotificationEntity habitNotification)
    {
      await _dbContext.HabitsNotificationSettings
        .Where(h => h.Id == habitNotification.Id)
        .ExecuteUpdateAsync(s => s
          .SetProperty(n => n.CountOfNotifications, habitNotification.CountOfNotifications)
          .SetProperty(n => n.IsSending, habitNotification.IsSending)
          .SetProperty(n => n.NotificationTime, habitNotification.NotificationTime));
    }
    /// <summary>
    /// Удалить настройки уведомлений привычки из базы данных.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task Delete(Guid id)
    {
      await _dbContext.HabitsNotificationSettings
        .Where(h => h.Id == id)
        .ExecuteDeleteAsync();
    }

    #region Конструкторы

    public HabitNotificationRepository(HabitTrackerContext dbContext)
    {
      _dbContext = dbContext;
    }

    #endregion
  }
}
