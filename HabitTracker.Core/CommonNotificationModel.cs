using HabitTracker.Data.Models;
using HabitTracker.Data.Repositories;
using HabitTracker.Data;
using Microsoft.EntityFrameworkCore.Internal;

namespace HabitTracker.Core
{
  public class CommonNotificationModel
  {
    #region Поля и свойства

    /// <summary>
    /// Фабрика создания контекста базы данных.
    /// </summary>
    private readonly DbContextFactory _dbContextFactory;
    /// <summary>
    /// Аргументы командной строки.
    /// </summary>
    private readonly string[] _args;


    #endregion

    #region Методы

    /// <summary>
    /// Получить список всех настроек уведомлений пользователей.
    /// </summary>
    /// <returns></returns>
    public async Task<List<NotificationEntity>?> GetAll()
    {
      await using (var dbContext = _dbContextFactory.CreateDbContext(this._args))
      {
        var notificationsRepository = new NotificationRepository(dbContext);
        return await notificationsRepository.Get();
      }
    }

    /// <summary>
    /// Получить настройки уведомлений по id.
    /// </summary>
    /// <param name="id">Уникальный идентификатор уведомлений.</param>
    /// <returns></returns>
    public async Task<NotificationEntity?> GetById(Guid id)
    {
      await using (var dbContext = _dbContextFactory.CreateDbContext(this._args))
      {
        var notificationRepository = new NotificationRepository(dbContext);
        return await notificationRepository.GetById(id);
      }
    }

    /// <summary>
    /// Получить общие настройки уведомлений пользователя по Id пользователя.
    /// </summary>
    /// <param name="chatId">Уникальный идентификатор чата пользователя.</param>
    /// <returns></returns>
    public async Task<NotificationEntity?> GetByUserChatId(long chatId)
    {
      await using (var dbContext = _dbContextFactory.CreateDbContext(this._args))
      {
        var notificationRepository = new NotificationRepository(dbContext);
        var usersRepository = new UsersRepository(dbContext);
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
      await using (var dbContext = _dbContextFactory.CreateDbContext(this._args))
      {
        var notificationRepository = new NotificationRepository(dbContext);
        var usersRepository = new UsersRepository(dbContext);
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
    }

    /// <summary>
    /// Обновить настройки уведомлений пользователя.
    /// </summary>
    /// <param name="chatId">Уникальный идентификатор чата.</param>
    /// <param name="timeStart">Время начала отправки уведомлений.</param>
    /// <param name="timeEnd">Время конца отправки уведомлений.</param>
    public async void Update(long chatId, TimeOnly timeStart, TimeOnly timeEnd)
    {
      await using (var dbContext = _dbContextFactory.CreateDbContext(this._args))
      {
        var notificationRepository = new NotificationRepository(dbContext); 
      var habitNotificationRepository = new HabitNotificationRepository(dbContext); 
      var usersRepository = new UsersRepository(dbContext);
      UserEntity? foundUser = await usersRepository.GetByChatId(chatId);
        if (foundUser != null)
        {
          var foundNotification = await notificationRepository.GetByUserId(foundUser.Id);
          if (foundNotification != null)
          {
            foundNotification.TimeStart = timeStart;
            foundNotification.TimeEnd = timeEnd;
            await notificationRepository.Update(foundNotification);
            Thread.Sleep(1000);
            var userHabitsSettings = await habitNotificationRepository.GetByNotificationSettingsId(foundNotification.Id);
            if (userHabitsSettings != null)
            {
              foreach (var habitSettings in userHabitsSettings)
              {
                habitSettings.NotificationTime.Clear();
                List<TimeOnly> notificationTime = new List<TimeOnly>();

                var period = (timeEnd - timeStart) / habitSettings.CountOfNotifications;
                for (int i = 0; i < habitSettings.CountOfNotifications; i++)
                {
                  if (i == 0)
                  {
                    notificationTime.Add(timeStart);
                  }
                  else
                  {
                    notificationTime.Add(notificationTime.LastOrDefault().AddMinutes(period.TotalMinutes));
                  }
                  habitSettings.NotificationTime = notificationTime;
                }
                await habitNotificationRepository.Update(habitSettings);
              }
            }
          }
        }
      }
    }

    #endregion

    #region Конструкторы
    public CommonNotificationModel(DbContextFactory dbContextFactory, string[] args)
    {
      this._dbContextFactory = dbContextFactory;
      this._args = args;
    }
    #endregion
  }
}
