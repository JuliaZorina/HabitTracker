using HabitTracker.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace HabitTracker.Data.Repositories
{
  public class NotificationRepository
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
    public async Task<List<NotificationEntity>> Get()
    {
      return await _dbContext.NotificationSettings
        .AsNoTracking()
        .ToListAsync();
    }

    /// <summary>
    /// Получить настройки уведомлений для привычек пользователя по уникальному идентификатору настройки.
    /// </summary>
    /// <returns>Асинхронно настройки уведомлений пользователя для привычки, найденные по ее уникальному идентификатору.</returns>
    public async Task<NotificationEntity?> GetById(Guid id)
    {
      return await _dbContext.NotificationSettings
        .AsNoTracking()
        .FirstOrDefaultAsync(h => id == h.Id);
    }

    /// <summary>
    /// Получить настройки уведомлений для привычки пользователя по уникальному идентификатору привычки.
    /// </summary>
    /// <returns>Асинхронно настройки уведомлений пользователя для привычки, найденные по уникальному идентификатору привычки.</returns>
    public async Task<NotificationEntity?> GetByUserId(Guid userId)
    {
      return await _dbContext.NotificationSettings
        .AsNoTracking()
        .FirstOrDefaultAsync(n => userId == n.UserId);
    }

    /// <summary>
    /// Добавить настройки уведомлений для привычек нового пользователя в базу данных.
    /// </summary>
    /// <param name="notification">Настройки уведомлений для привычек пользователя.</param>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    public async Task Add(NotificationEntity notification)
    {
      await _dbContext.AddAsync(notification);
      await _dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Обновить настроки уведомлений для привычки.
    /// </summary>
    /// <param name="notification">Экземпляр класса настроек уведомлений привычки.</param>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    public async Task Update(NotificationEntity notification)
    {
      await _dbContext.NotificationSettings
        .Where(h => h.Id == notification.Id)
        .ExecuteUpdateAsync(s => s
          .SetProperty(n => n.TimeStart, notification.TimeStart)
          .SetProperty(n => n.TimeEnd, notification.TimeEnd));
    }

    #region Конструкторы

    public NotificationRepository(HabitTrackerContext dbContext)
    {
      _dbContext = dbContext;
    }

    #endregion
  }
}
