﻿using HabitTracker.Data;
using HabitTracker.Data.Models;
using HabitTracker.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HabitTracker.Core
{
  public class CommonHabitNotificationModel
  {

    #region Поля и свойства

    /// <summary>
    /// Контекст базы данных
    /// </summary>
    private readonly HabitTrackerContext _dbContext;

    #endregion

    #region Методы

    /// <summary>
    /// Получить настройки привычки по Id.
    /// </summary>
    /// <param name="habitId">уникальный идентификатор привычки.</param>
    /// <param name="chatId">Уникальный идентификатор чата пользователя.</param>
    /// <returns></returns>
    public async Task<HabitNotificationEntity?> GetById(long chatId, Guid habitId)
    {
      var habitsNotificationRepository = new HabitNotificationRepository(_dbContext);
      var usersRepository = new UsersRepository(_dbContext);
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

    /// <summary>
    /// Добавить настройки новой привычки в базу данных.
    /// </summary>
    /// <param name="chatId">Уникальный идентификатор чата.</param>
    /// <param name="timeStart">Время начала отправки уведомлений.</param>
    /// <param name="timeEnd">Время конца отправки уведомлений.</param>
    /// <param name="habitId">Уникальный идентификатор привычки.</param>
    /// <param name="isSending">Статус активности данной привычки.</param>
    /// <param name="countOfNotifications">Количество уведомлений для привычки.</param>
    /// <exception cref="Exception"></exception>
    public async void Add(long chatId, Guid habitId, bool isSending, int countOfNotifications)
    {
      var habitsNotificationRepository = new HabitNotificationRepository(_dbContext);
      var notificationRepository = new NotificationRepository(_dbContext);
      var usersRepository = new UsersRepository(_dbContext);
      UserEntity? foundUser = await usersRepository.GetByChatId(chatId);
      if (foundUser != null)
      {
        NotificationEntity? foundNotification = await notificationRepository.GetByUserId(foundUser.Id);
        if(foundNotification != null)
        {
          var habitNotification = new HabitNotificationEntity(Guid.NewGuid(), habitId, foundNotification.Id, isSending, countOfNotifications);
          await habitsNotificationRepository.Add(habitNotification);
        }        
      }
      else
      {
        throw new Exception("Пользователь с таким chatId не существует в базе данных");
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
      var habitsNotificationRepository = new HabitNotificationRepository(_dbContext);
      var foundNotification = await habitsNotificationRepository.GetByHabitId(habitId);
      if (foundNotification != null)
      {
        foundNotification.IsSending = isSending;
        foundNotification.CountOfNotifications = countOfNotifications;
        await habitsNotificationRepository.Update(foundNotification);
      }
    }

    /// <summary>
    /// Удалить настройки уведомлений привычки из базы данных.
    /// </summary>
    /// <param name="id">Уникальный идентификатор привычки.</param>
    public async void Delete(Guid id)
    {
      var habitsNotificationRepository = new HabitNotificationRepository(_dbContext);
      var result = await habitsNotificationRepository.GetById(id);
      if (result != null)
      {
        await habitsNotificationRepository.Delete(id);
      }
    }


    #endregion

    #region Конструкторы
    public CommonHabitNotificationModel(HabitTrackerContext dbContext)
    {
      _dbContext = dbContext;
    }
    #endregion
  }
}
