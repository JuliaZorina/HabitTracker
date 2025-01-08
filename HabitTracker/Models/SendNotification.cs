using HabitTracker.Data;
using Microsoft.EntityFrameworkCore.Internal;
using System.Text;
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
    private static Dictionary<Guid, Dictionary<Guid, List<TimeOnly>>> usersHabitsNotifications = new Dictionary<Guid, Dictionary<Guid, List<TimeOnly>>>();

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
        userNotificationsSettings.Clear();
        usersHabitsNotifications.Clear();
        await GetData(dbContext, dbContextFactory, args);
        DateTime ntpTime = GetTime.GetNetworkTime("time.google.com");
        Console.WriteLine($"Start cheking for notifications. Sent time: {ntpTime}");
        foreach (var userHabitNotification in usersHabitsNotifications)
        {
          var notificationsSettingsId = userHabitNotification.Key;
          Dictionary<Guid, List<TimeOnly>> habitAndTime = userHabitNotification.Value;
          int i = 0;
          foreach (var habitData in habitAndTime)
          {
            var habitId = habitData.Key;
            var habitTimes = habitData.Value;
            var currentTime = TimeOnly.FromDateTime(ntpTime);

            foreach (var habitTime in habitTimes)
            {
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
                    var messageBuilder = new StringBuilder();
                    var keyboard = new InlineKeyboardMarkup(new[]
                        {
                         new[]
                         {
                           InlineKeyboardButton.WithCallbackData("Отметить выполнение", $"/mark_as_done_{habitId}")
                         },
                         new[]
                         {
                           InlineKeyboardButton.WithCallbackData("Изменить срок выполнения привычки", $"/edit_ex_date_{habitId}")
                         },
                         new[]
                         {
                           InlineKeyboardButton.WithCallbackData("Скрыть уведомление", "/delete_notification")
                         }
                       });
                    var foundHabit = await habitsModel.GetById(foundUser.ChatId, habitId);
                    if (foundHabit != null && foundHabit.Status != HabitStatus.Done)
                    {
                      
                      messageBuilder.AppendLine($"Не забудьте выполнить привычку \"{foundHabit.Title}\"");
                      if (foundHabit.ExpirationDate != null)
                      {
                        if (DateOnly.FromDateTime((DateTime)foundHabit.ExpirationDate) == DateOnly.FromDateTime(ntpTime).AddDays(1)
                          || foundHabit.ExpirationDate < ntpTime)
                        {
                          messageBuilder.AppendLine("Сегодня последний день выполнения привычки. Завтра она будет удалена");
                        }
                      }                      
                    }
                    if (messageBuilder.Length > 0)
                    {
                      await HabitTracker.TelegramBotHandler.SendMessageAsync(botClient, foundUser.ChatId, messageBuilder.ToString(), keyboard);
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
    }


    /// <summary>
    /// Получить данные о времени отправки уведомлений пользователям.
    /// </summary>
    /// <param name="dbContextFactory">Фабрика создания контекста базы данных</param>
    /// <param name="args">Аргументы командной строки.</param>
    private static async Task GetData(HabitTrackerContext dbContext, DbContextFactory dbContextFactory, string[] args)
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
        for (int i = 0; i < allHabitsNotifications.Count; i++)
        {
          if (!usersHabitsNotifications.ContainsKey(allHabitsNotifications[i].UserNotificationsId))
          {
            usersHabitsNotifications[allHabitsNotifications[i].UserNotificationsId] = new Dictionary<Guid, List<TimeOnly>>();
          }

          if (!usersHabitsNotifications[allHabitsNotifications[i].UserNotificationsId].ContainsKey(allHabitsNotifications[i].HabitId))
          {
            usersHabitsNotifications[allHabitsNotifications[i].UserNotificationsId][allHabitsNotifications[i].HabitId] = new List<TimeOnly>();
          }
          if (allHabitsNotifications[i].NotificationTime.Count > 0)
          {
            for (int j=0; j<allHabitsNotifications[i].NotificationTime.Count; j++)
            {
              var time = allHabitsNotifications[i].NotificationTime[j];
              
              if (!usersHabitsNotifications[allHabitsNotifications[i].UserNotificationsId][allHabitsNotifications[i].HabitId].Contains(time))
              {
                usersHabitsNotifications[allHabitsNotifications[i].UserNotificationsId][allHabitsNotifications[i].HabitId].Add(time);
              }
            }
          }
        }
      }
    }
  }
}
