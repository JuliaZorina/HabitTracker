using HabitTracker.Data.Models;
using HabitTracker.Data.Repositories;
using HabitTracker.Data;

namespace HabitTracker.Core
{
  public class CommonNotificationModel
  {
    #region Поля и свойства

    /// <summary>
    /// Контекст базы данных
    /// </summary>
    private readonly HabitTrackerContext _dbContext;

    #endregion

    #region Методы

    /// <summary>
    /// Получить список всех настроек уведомлений пользователей.
    /// </summary>
    /// <returns></returns>
    public async Task<List<NotificationEntity>?> GetAll()
    {
      var notificationsRepository = new NotificationRepository(_dbContext);
      return await notificationsRepository.Get();
    }

    /// <summary>
    /// Получить настройки уведомлений по id.
    /// </summary>
    /// <param name="id">Уникальный идентификатор уведомлений.</param>
    /// <returns></returns>
    public async Task<NotificationEntity?> GetById(Guid id)
    {
      var notificationRepository = new NotificationRepository(_dbContext);
      return await notificationRepository.GetById(id);
    }

    /// <summary>
    /// Получить общие настройки уведомлений пользователя по Id пользователя.
    /// </summary>
    /// <param name="chatId">Уникальный идентификатор чата пользователя.</param>
    /// <returns></returns>
    public async Task<NotificationEntity?> GetByUserChatId(long chatId)
    {
      var notificationRepository = new NotificationRepository(_dbContext);
      var usersRepository = new UsersRepository(_dbContext);
      UserEntity? foundUser = await usersRepository.GetByChatId(chatId);
      if (foundUser != null)
      {
        return await notificationRepository.GetByUserId(foundUser.Id);
      }
      else
      {
        throw new Exception("Пользователь с таким chatId не найден в базе данных");
      }
    }

    /// <summary>
    /// Добавить настройки уведомлений о привычках для нового пользователя в базу данных.
    /// </summary>
    /// <param name="chatId">Уникальный идентификатор чата.</param>
    /// <param name="timeStart">Время начала отправки уведомлений.</param>
    /// <param name="timeEnd">Время конца отправки уведомлений.</param>
    /// <exception cref="Exception"></exception>
    public async void Add(long chatId, TimeOnly timeStart, TimeOnly timeEnd)
    {
      var notificationRepository = new NotificationRepository(_dbContext);
      var usersRepository = new UsersRepository(_dbContext);
      UserEntity? foundUser = await usersRepository.GetByChatId(chatId);
      if (foundUser != null)
      {
        var newNotification = new NotificationEntity(Guid.NewGuid(), foundUser.Id, timeStart, timeEnd);
        await notificationRepository.Add(newNotification);
      }
      else
      {
        throw new Exception("Пользователь с таким chatId не существует в базе данных");
      }
    }

    /// <summary>
    /// Обновить настройки уведомлений пользователя.
    /// </summary>
    /// <param name="chatId">Уникальный идентификатор чата.</param>
    /// <param name="timeStart">Время начала отправки уведомлений.</param>
    /// <param name="timeEnd">Время конца отправки уведомлений.</param>
    public async void Update(long chatId, TimeOnly timeStart, TimeOnly timeEnd)
    {
      var notificationRepository = new NotificationRepository(_dbContext); var usersRepository = new UsersRepository(_dbContext);
      UserEntity? foundUser = await usersRepository.GetByChatId(chatId);
      if (foundUser != null)
      {
        var foundNotification = await notificationRepository.GetByUserId(foundUser.Id);
        if (foundNotification != null)
        {
          foundNotification.TimeStart = timeStart;
          foundNotification.TimeEnd = timeEnd;
          await notificationRepository.Update(foundNotification);
        }
      }
    }

    #endregion

    #region Конструкторы
    public CommonNotificationModel(HabitTrackerContext dbContext)
    {
      _dbContext = dbContext;
    }
    #endregion
  }
}
