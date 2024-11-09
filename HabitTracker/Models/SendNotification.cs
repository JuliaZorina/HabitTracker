using HabitTracker.Data;
using Microsoft.EntityFrameworkCore.Internal;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace HabitTracker.Core
{
  /// <summary>
  /// Обработка методов для отправки уведомлений пользователю.
  /// </summary>
  public static class SendNotification
  {
    /// <summary>
    /// Словарь для хранения уникального идентификатора настроек уведомлений и соответствующего ему уникального идентификатора пользователя.
    /// </summary>
    private static Dictionary<Guid, Guid> userNotificationsSettings = new Dictionary<Guid, Guid>();
    /// <summary>
    /// Словарь для хранения уникального идентификатора настроек уведомлений и соответсвующего ему словаря из пары значений 
    /// уникального идентификатора привычки и времени, в которое должно прийти уведомление об этой привычке.
    /// </summary>
    private static Dictionary<Guid, Dictionary<Guid, TimeOnly>> usersHabitsNotifications = new Dictionary<Guid, Dictionary<Guid, TimeOnly>>();

    /// <summary>
    /// Отправить пользователю уведомление с напоминанием о выполнении привычки.
    /// </summary>
    /// <param name="dbContextFactory">Фабрика создания контекста базы данных</param>
    /// <param name="args">Аргументы командной строки.</param>
    /// <param name="botClient">Клиент Telegram бота.</param>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    public static async Task SendNotificationToUser(DbContextFactory dbContextFactory, string[] args, ITelegramBotClient botClient)
    {
      await using (var dbContext = dbContextFactory.CreateDbContext(args))
      {
        GetData(dbContext, dbContextFactory, args);
        DateTime ntpTime = GetTime.GetNetworkTime("time.google.com");
        foreach (var userHabitNotification in usersHabitsNotifications)
        {
          var notificationsSettingsId = userHabitNotification.Key;
          Dictionary<Guid, TimeOnly> habitAndTime = userHabitNotification.Value;
          foreach (var habitData in habitAndTime)
          {
            var habitId = habitData.Key;
            var habitTime = habitData.Value;
            var currentTime = TimeOnly.FromDateTime(ntpTime);

            if (currentTime == habitTime || currentTime.IsBetween(habitTime, habitTime.AddMinutes(1)))
            {
              var habitsModel = new CommonHabitsModel(dbContextFactory, args);
              var userModel = new CommonUserModel(dbContextFactory, args);
              var notificationsModel = new CommonNotificationModel(dbContextFactory, args);
              var foundNotification = await notificationsModel.GetById(notificationsSettingsId);
              if (foundNotification != null)
              {
                var foundUser = await userModel.GetById(foundNotification.UserId);
                if (foundUser != null)
                {
                  var foundHabit = await habitsModel.GetById(foundUser.ChatId, habitId);
                  if (foundHabit != null && foundHabit.Status != HabitStatus.Done)
                  {
                    var keyboard = new InlineKeyboardMarkup(new[]
                  {
                    new[]
                    {
                      InlineKeyboardButton.WithCallbackData("Отметить выполнение", $"/mark_as_done_{habitId}")
                    },
                    new[]
                    {
                      InlineKeyboardButton.WithCallbackData("Скрыть уведомление", "/delete_notification")
                    }
                });
                    await HabitTracker.TelegramBotHandler.SendMessageAsync(botClient, foundUser.ChatId,
                      $"Не забудьте выполнить привычку \"{foundHabit.Title}\"", keyboard);
                    Console.WriteLine($"Notification send to {foundUser.Name} sucesessfully. Habit: {foundHabit.Title}. Time: {habitTime}." +
                      $"\nSent time: {ntpTime}");
                  }                  
                }
              }
            }
          }
        }            
      }
    }

    /// <summary>
    /// Получить данные о времени отправки уведомлений пользователям.
    /// </summary>
    /// <param name="dbContextFactory">Фабрика создания контекста базы данных</param>
    /// <param name="args">Аргументы командной строки.</param>
    private static async void GetData(HabitTrackerContext dbContext, DbContextFactory dbContextFactory, string[] args)
    {
      var notificationsModel = new CommonNotificationModel(dbContextFactory, args);
      var habitNotificationsModel = new CommonHabitNotificationModel(dbContextFactory, args);
      var allNotifications = await notificationsModel.GetAll();
      if (allNotifications != null)
      {
        foreach (var notification in allNotifications)
        {
          userNotificationsSettings[notification.Id] = notification.UserId;
        }
      }
      var allHabitsNotifications = await habitNotificationsModel.GetAll();
      if (allHabitsNotifications != null)
      {
        foreach (var habitNotification in allHabitsNotifications)
        {
          if (!usersHabitsNotifications.ContainsKey(habitNotification.UserNotificationsId))
          {
            usersHabitsNotifications[habitNotification.UserNotificationsId] = new Dictionary<Guid, TimeOnly>();
          }
          if (habitNotification.NotificationTime.Count > 0)
          {
            foreach (var time in habitNotification.NotificationTime)
            {
              usersHabitsNotifications[habitNotification.UserNotificationsId][habitNotification.HabitId] = time;
            }
          }
        }
      }
    }    
  }
}
