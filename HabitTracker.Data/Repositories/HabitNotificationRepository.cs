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
    /// Добавить настройки уведомлений новой привычки в базу данных.
    /// </summary>
    /// <param name="habitNotification">Настройки уведомлений для привычки пользователя.</param>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    public async Task Add(HabitNotificationEntity habitNotification)
    {
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
          .SetProperty(n => n.TimeStart, habitNotification.TimeStart)
          .SetProperty(n => n.TimeEnd, habitNotification.TimeEnd)
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
