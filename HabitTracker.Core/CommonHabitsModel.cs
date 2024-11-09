using HabitTracker.Data;
using HabitTracker.Data.Models;
using HabitTracker.Data.Repositories;
using Microsoft.EntityFrameworkCore.Internal;

namespace HabitTracker.Core
{
  /// <summary>
  /// Общие методы работы с моделью привычки.
  /// </summary>
  public class CommonHabitsModel
  {
    #region Поля и свойства

    /// <summary>
    /// Контекст базы данных.
    /// </summary>
    private readonly DbContextFactory _dbContextFactory;
    private readonly string[] _args;


    #endregion

    #region Методы

    /// <summary>
    /// Получить список всех настроек уведомлений пользователей.
    /// </summary>
    /// <returns></returns>
    public async Task<List<HabitEntity>?> GetAll()
    {
      await using (var dbContext = _dbContextFactory.CreateDbContext(this._args))
      {
        var habitssRepository = new HabitsRepository(dbContext);
        return await habitssRepository.Get();
      }
    }

    /// <summary>
    /// Получить привычку по Id.
    /// </summary>
    /// <param name="id">уникальный идентификатор привычки.</param>
    /// <param name="chatId">Уникальный идентификатор чата пользователя.</param>
    /// <returns></returns>
    public async Task<HabitEntity?> GetById(long chatId, Guid id)
    {
      await using (var dbContext = _dbContextFactory.CreateDbContext(this._args))
      {
        var habitsRepository = new HabitsRepository(dbContext);
        var usersRepository = new UsersRepository(dbContext);
        UserEntity? foundUser = await usersRepository.GetByChatId(chatId);
        if (foundUser != null)
        {
          return await habitsRepository.GetById(id);
        }
        else
        {
          throw new Exception("Пользователь с таким chatId не найден в базе данных");
        }
      }
    }

    /// <summary>
    /// Получить список активных привычек с выбранным статусом.
    /// </summary>
    /// <param name="status">Статус привычки.</param>
    /// <param name="chatId">Уникальный идентификатор чата пользователя.</param>
    /// <returns></returns>
    public async Task<List<HabitEntity>?> GetByStatus(long chatId,HabitStatus status)
    {
      await using (var dbContext = _dbContextFactory.CreateDbContext(this._args))
      {
        var habitsRepository = new HabitsRepository(dbContext);
        var usersRepository = new UsersRepository(dbContext);
        UserEntity? foundUser = await usersRepository.GetByChatId(chatId);
        if (foundUser != null)
        {
          return await habitsRepository.GetByFilter(foundUser.Id, status);
        }
        else
        {
          throw new Exception("Пользователь с таким chatId не найден в базе данных");
        }
      }
    }

    /// <summary>
    /// Получить список всех активных привычек пользователя.
    /// </summary>
    /// <param name="chatId">Уникальный идентификатор чата пользователя.</param>
    /// <returns></returns>
    public async Task<List<HabitEntity>?> GetAllActive(long chatId)
    {
      await using (var dbContext = _dbContextFactory.CreateDbContext(this._args))
      {
        var habitsRepository = new HabitsRepository(dbContext);
        var usersRepository = new UsersRepository(dbContext);
        UserEntity? foundUser = await usersRepository.GetByChatId(chatId);
        if (foundUser != null)
        {
          return await habitsRepository.GetByFilter(foundUser.Id, false);
        }
        else
        {
          throw new Exception("Пользователь с таким chatId не найден в базе данных");
        }
      }
    }

    /// <summary>
    /// Получить список всех приостановленных привычек пользователя.
    /// </summary>
    /// <param name="chatId">Уникальный идентификатор чата пользователя.</param>
    /// <param name="isNecessary">Обязательность привычки.</param>
    /// <returns></returns>
    public async Task<List<HabitEntity>?> GetSuspendedHabit(long chatId)
    {
      await using (var dbContext = _dbContextFactory.CreateDbContext(this._args))
      {
        var habitsRepository = new HabitsRepository(dbContext);
        var usersRepository = new UsersRepository(dbContext);
        UserEntity? foundUser = await usersRepository.GetByChatId(chatId);
        if (foundUser != null)
        {
          return await habitsRepository.GetByFilter(foundUser.Id, true);
        }
        else
        {
          throw new Exception("Пользователь с таким chatId не найден в базе данных");
        }
      }
    }

    /// <summary>
    /// Получить список всех активных привычек пользователя в зависимости от обязательности привычки.
    /// </summary>
    /// <param name="chatId">Уникальный идентификатор чата пользователя.</param>
    /// <returns></returns>
    public async Task<List<HabitEntity>?> GetNecessaryHabit(long chatId, bool isNecessary)
    {
      await using (var dbContext = _dbContextFactory.CreateDbContext(this._args))
      {
        var habitsRepository = new HabitsRepository(dbContext);
        var usersRepository = new UsersRepository(dbContext);
        UserEntity? foundUser = await usersRepository.GetByChatId(chatId);
        if (foundUser != null)
        {
          return await habitsRepository.GetByFilter(foundUser.Id, false, isNecessary);
        }
        else
        {
          throw new Exception("Пользователь с таким chatId не найден в базе данных");
        }
      }
    }
    /// <summary>
    /// Добавить новую привычку в базу данных.
    /// </summary>
    /// <param name="chatId">Уникальный идентификатор чата пользователя.</param>
    /// <param name="title">Название привычки.</param>
    public async void Add(long chatId, string title, int numberOfExecutions, DateTime? days, bool isNecessary)
    {
      await using (var dbContext = _dbContextFactory.CreateDbContext(this._args))
      {
        var habitsRepository = new HabitsRepository(dbContext);
        var usersRepository = new UsersRepository(dbContext);
        var notificationRepository = new NotificationRepository(dbContext);
        var habitNotificationRepository = new HabitNotificationRepository(dbContext);
        UserEntity? foundUser = await usersRepository.GetByChatId(chatId);
        if (foundUser != null)
        {
          var expirationDate = days;
          var habit = new HabitEntity(Guid.NewGuid(), foundUser.Id, title, numberOfExecutions, expirationDate, isNecessary);

          var userNotifications = await notificationRepository.GetByUserId(foundUser.Id);
          if (userNotifications != null)
          {
            List<TimeOnly> notificationTime = new List<TimeOnly>();

            var period = (userNotifications.TimeEnd - userNotifications.TimeStart) / numberOfExecutions;
            for (int i = 0; i < numberOfExecutions; i++)
            {
              if (i == 0)
              {
                notificationTime.Add(userNotifications.TimeStart);
              }
              else
              {
                notificationTime.Add(notificationTime.LastOrDefault().AddMinutes(period.TotalMinutes));
              }
            }
            var habitNotification = new HabitNotificationEntity(Guid.NewGuid(), habit.Id, userNotifications.Id, true,
              habit.NumberOfExecutions, notificationTime);
            await habitsRepository.Add(habit);
            Thread.Sleep(1000);
            await habitNotificationRepository.Add(habitNotification);
          }
        }
        else
        {
          throw new Exception("Пользователь с таким chatId не существует в базе данных");
        }
      }
    }

    /// <summary>
    /// Обновить данные о привычке.
    /// </summary>
    /// <param name="id">Уникальный идентификатор привычки.</param>
    /// <param name="name">Название привычки.</param>
    /// <param name="lastDay">Дата последнего выполнения привычки.</param>
    /// <param name="status">Статус привычки.</param>
    /// <param name="progressDays">Количество дней прогресса привычки.</param>
    public async void Update(Guid id, string name, DateOnly? lastDay, HabitStatus status, long progressDays, 
      DateTime? expirationDate, int numberOfExecutions, bool isSuspended)
    {
      await using (var dbContext = _dbContextFactory.CreateDbContext(this._args))
      {
        var habitsRepository = new HabitsRepository(dbContext);
        var result = await habitsRepository.GetById(id);
        if (result != null)
        {
          var habit = new HabitEntity(id, name, lastDay, status, progressDays, expirationDate, numberOfExecutions, isSuspended);
          await habitsRepository.Update(habit);
        }
      } 
    }

    /// <summary>
    /// Удалить привычку из базы данных.
    /// </summary>
    /// <param name="id">Уникальный идентификатор привычки.</param>
    public async void Delete(Guid id)
    {
      await using (var dbContext = _dbContextFactory.CreateDbContext(this._args))
      {
        var habitsRepository = new HabitsRepository(dbContext);
        var result = await habitsRepository.GetById(id);
        if (result != null)
        {
          await habitsRepository.Delete(id);
        }
      }
    }

    #endregion

    #region Конструкторы
    public CommonHabitsModel(DbContextFactory dbContextFactory, string[] args)
    {
      this._dbContextFactory = dbContextFactory;
      this._args = args;
    }
    #endregion
  }
}
