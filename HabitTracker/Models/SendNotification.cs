using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using HabitTracker.Data;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace HabitTracker.Core
{
  public static class SendNotification
  {
    private static Dictionary<Guid, Guid> userNotificationsSettings = new Dictionary<Guid, Guid>();
    private static Dictionary<Guid, Dictionary<Guid, TimeOnly>> usersHabitsNotifications = new Dictionary<Guid, Dictionary<Guid, TimeOnly>>();

    public static async Task SendNotificationToUser(HabitTrackerContext dbContext, ITelegramBotClient botClient)
    {
      GetData(dbContext);
      DateTime ntpTime = GetNetworkTime("time.google.com");
      foreach (var userHabitNotification in usersHabitsNotifications)
      {
        var notificationsSettingsId = userHabitNotification.Key;
        Dictionary<Guid, TimeOnly> habitAndTime = userHabitNotification.Value;
        foreach(var habitData in habitAndTime)
        {
          var habitId = habitData.Key;
          var habitTime = habitData.Value;

          if (TimeOnly.FromDateTime(ntpTime) == habitTime|| TimeOnly.FromDateTime(ntpTime).IsBetween(habitTime, habitTime.AddMinutes(1)))
          {
            var habitsModel = new CommonHabitsModel(dbContext);
            var userModel = new CommonUserModel(dbContext);
            var notificationsModel = new CommonNotificationModel(dbContext);
            var foundNotification = await notificationsModel.GetById(notificationsSettingsId);
            if (foundNotification != null) 
            {
              var foundUser = await userModel.GetById(foundNotification.UserId);
              if (foundUser != null)
              {
                var foundHabit = await habitsModel.GetById(foundUser.ChatId, habitId);
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

    private static async void GetData(HabitTrackerContext dbContext)
    {
      var notificationsModel = new CommonNotificationModel(dbContext);
      var habitNotificationsModel = new CommonHabitNotificationModel(dbContext);
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

    private static DateTime GetNetworkTime(string ntpServer)
    {
      const int ntpDataLength = 48;
      byte[] ntpData = new byte[ntpDataLength];
      ntpData[0] = 0x1B;

      IPEndPoint ipEndPoint = new IPEndPoint(Dns.GetHostAddresses(ntpServer)[0], 123);
      using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
      {
        socket.Connect(ipEndPoint);
        socket.Send(ntpData);
        socket.Receive(ntpData);
        socket.Close();
      }

      ulong intPart = BitConverter.ToUInt32(ntpData, 40);
      ulong fractPart = BitConverter.ToUInt32(ntpData, 44);

      intPart = SwapEndianness(intPart);
      fractPart = SwapEndianness(fractPart);

      ulong milliseconds = (intPart * 1000) + ((fractPart * 1000) / 0x100000000L);
      DateTime networkDateTime = new DateTime(1900, 1, 1).AddMilliseconds((long)milliseconds);

      return networkDateTime.ToLocalTime();
    }

    private static uint SwapEndianness(ulong x)
    {
      return (uint)(((x & 0x000000FF) << 24) +
                    ((x & 0x0000FF00) << 8) +
                    ((x & 0x00FF0000) >> 8) +
                    ((x & 0xFF000000) >> 24));
    }
  }

}
