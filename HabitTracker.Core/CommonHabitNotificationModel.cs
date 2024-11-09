using HabitTracker.Data;
using HabitTracker.Data.Models;
using HabitTracker.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

namespace HabitTracker.Core
{
  public class CommonHabitNotificationModel
  {
    #region Поля и свойства

    /// <summary>
    /// Контекст базы данных
    /// </summary>
    private readonly DbContextFactory _dbContextFactory;
    private readonly string[] _args;


    #endregion

    #region Методы

    /// <summary>
    /// Получить список всех настроек уведомлений к привычкам пользователей.
    /// </summary>
    /// <returns></returns>
    public async Task<List<HabitNotificationEntity>?> GetAll()
    {
      await using (var dbContext = _dbContextFactory.CreateDbContext(this._args))
      {
        var habitNotificationsRepository = new HabitNotificationRepository(dbContext);
        return await habitNotificationsRepository.Get();
      }
    }

    /// <summary>
    /// Получить настройки привычки по Id.
    /// </summary>
    /// <param name="habitId">уникальный идентификатор привычки.</param>
    /// <param name="chatId">Уникальный идентификатор чата пользователя.</param>
    /// <returns></returns>
    public async Task<HabitNotificationEntity?> GetById(long chatId, Guid habitId)
    {
      await using (var dbContext = _dbContextFactory.CreateDbContext(this._args))
      {
        var habitsNotificationRepository = new HabitNotificationRepository(dbContext);
        var usersRepository = new UsersRepository(dbContext);
        UserEntity? foundUser = await usersRepository.GetByChatId(chatId);
        if (foundUser != null)
        {
          return await habitsNotificationRepository.GetByHabitId(habitId);
        }
        else
        {
          throw new Exception("Пользователь с таким chatId не найден в базе данных");
        }
      }
    }

    /// <summary>
    /// Добавить настройки новой привычки в базу данных.
    /// </summary>
    /// <param name="chatId">Уникальный идентификатор чата.</param>
    /// <param name="habitId">Уникальный идентификатор привычки.</param>
    /// <param name="isSending">Статус активности данной привычки.</param>
    /// <param name="countOfNotifications">Количество уведомлений для привычки.</param>
    /// <exception cref="Exception"></exception>
    public async void Add(long chatId, Guid habitId, bool isSending, int countOfNotifications, List<TimeOnly> notificationTime)
    {
      await using (var dbContext = _dbContextFactory.CreateDbContext(this._args))
      {
        var habitsNotificationRepository = new HabitNotificationRepository(dbContext);
        var notificationRepository = new NotificationRepository(dbContext);
        var usersRepository = new UsersRepository(dbContext);
        UserEntity? foundUser = await usersRepository.GetByChatId(chatId);
        if (foundUser != null)
        {
          NotificationEntity? foundNotification = await notificationRepository.GetByUserId(foundUser.Id);
          if (foundNotification != null)
          {
            var habitNotification = new HabitNotificationEntity(Guid.NewGuid(), habitId, foundNotification.Id, isSending,
              countOfNotifications, notificationTime);
            await habitsNotificationRepository.Add(habitNotification);
          }
        }
        else
        {
          throw new Exception("Пользователь с таким chatId не существует в базе данных");
        }
      }
    }

    /// <summary>
    /// Обновить настройки уведомлений привычки.
    /// </summary>
    /// <param name="habitId">Уникальный идентификатор привычки.</param>
    /// <param name="isSending">Активна привычка или нет.</param>
    /// <param name="countOfNotifications">Количество уведомлений о привычке.</param>
    public async void Update(Guid habitId, bool isSending, int countOfNotifications)
    {
      await using (var dbContext = _dbContextFactory.CreateDbContext(this._args))
      {
        var habitsNotificationRepository = new HabitNotificationRepository(dbContext);
        var foundNotification = await habitsNotificationRepository.GetByHabitId(habitId);
        if (foundNotification != null)
        {
          foundNotification.IsSending = isSending;
          foundNotification.CountOfNotifications = countOfNotifications;
          await habitsNotificationRepository.Update(foundNotification);
        }
      }
    }

    /// <summary>
    /// Удалить настройки уведомлений привычки из базы данных.
    /// </summary>
    /// <param name="id">Уникальный идентификатор привычки.</param>
    public async void Delete(Guid id)
    {
      await using (var dbContext = _dbContextFactory.CreateDbContext(this._args))
      {
        var habitsNotificationRepository = new HabitNotificationRepository(dbContext);
        var result = await habitsNotificationRepository.GetById(id);
        if (result != null)
        {
          await habitsNotificationRepository.Delete(id);
        }
      }
    }

    #endregion

    #region Конструкторы

    public CommonHabitNotificationModel(DbContextFactory dbContextFactory, string[] args)
    {
      this._dbContextFactory = dbContextFactory;
      this._args = args;
    }

    #endregion
  }
}
