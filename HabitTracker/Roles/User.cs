using HabitTracker.Core;
using HabitTracker.Data;
using HabitTracker.Interfaces;
using HabitTracker.Models;
using System.Globalization;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace HabitTracker
{
  public class User : UserEntity, IMessageHandler
  {
    #region Поля и свойства

    private HabitTrackerContext _dbContext;

    #endregion

    #region Методы

    public async Task ProcessMessageAsync(ITelegramBotClient botClient, long chatId, string message)
    {
      if (string.IsNullOrEmpty(message))
      {
        return;
      }

      if (message.ToLower().Contains("/start"))
      {
        var keyboard = new InlineKeyboardMarkup(new[]
         {
          new[]
          {
            InlineKeyboardButton.WithCallbackData("Получить список привычек", "/getHabits")
          },
          new[]
          {
            InlineKeyboardButton.WithCallbackData("Создать новую привычку", "/addNewHabit")
          },
          new[]
          {
            InlineKeyboardButton.WithCallbackData("Получить статистику по привычкам", "/getStatistics")
          },

    });
        await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Выберите действие: ", keyboard);
      }
      else if (!string.IsNullOrEmpty(UserStateTracker.GetUserState(chatId)))
      {
        await ProcessCreatingHabitAsync(botClient, chatId, message);
      }
    }

    public async Task ProcessCallbackAsync(ITelegramBotClient botClient, long chatId, string callbackData, int messageId)
    {
      if (string.IsNullOrEmpty(callbackData)) return;
      else
      {
        if (callbackData.ToLower(CultureInfo.CurrentCulture).Contains("/gethabits"))
        {
          var keyboard = new InlineKeyboardMarkup(new[]
          {
            new[]
            {
              InlineKeyboardButton.WithCallbackData("Получить список всех привычек", "/get_allHabits")
            },
            new[]
            {
              InlineKeyboardButton.WithCallbackData("Получить список невыполненных сегодня привычек", "/get_undoneHabits")
            },
            new[]
            {
              InlineKeyboardButton.WithCallbackData("Получить список выполненных сегодня привычек", "/get_doneHabits")
            },
            new[]
            {
              InlineKeyboardButton.WithCallbackData("На главную", "/start")
            },
          });
          await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Выберите действие: ", keyboard, messageId);
        }
        else if (callbackData.Contains("/start"))
        {
          await botClient.DeleteMessageAsync(chatId, messageId);
          await Task.Delay(10);
          await ProcessMessageAsync(botClient, chatId, callbackData);
        }
        else if (callbackData.Contains("/get_allHabits"))
        {
          await GetHabits(botClient, chatId, messageId);
        }
        else if (callbackData.Contains("/get_undoneHabits"))
        {
          await GetHabitsByStatus(botClient, chatId, messageId, HabitStatus.Undone);
        }
        else if (callbackData.Contains("/get_doneHabits"))
        {
          await GetHabitsByStatus(botClient, chatId, messageId, HabitStatus.Done);
        }
        else if (callbackData.Contains("/addNewHabit"))
        {
          await CreateHabitAsync(botClient, chatId);
        }

      }
    }

    private async Task GetHabits(ITelegramBotClient botClient, long chatId, int messageId)
    {
      var habitModel = new CommonHabitsModel(_dbContext);
      var habits = await habitModel.GetAll(chatId);

      var messageBuilder = new StringBuilder();
      messageBuilder.AppendLine($"Список всех привычек");
      foreach(var habit in habits)
      {
        messageBuilder.AppendLine($"Название привычки: {habit.Title}." +
          $"\nСтатус на {DateOnly.FromDateTime(DateTime.Now)}: {habit.Status}");
      }
      var keyboard = new InlineKeyboardMarkup(new[]
       {
          new[]
          {
            InlineKeyboardButton.WithCallbackData("На главную", "/start")
          }
        });
      await HabitTracker.TelegramBotHandler.SendMessageAsync(botClient, chatId, messageBuilder.ToString(), keyboard, messageId);

      return;
    }

    private async Task GetHabitsByStatus(ITelegramBotClient botClient, long chatId, int messageId, HabitStatus status)
    {
      var habitModel = new CommonHabitsModel(_dbContext);
      List<HabitEntity>? habits = new List<HabitEntity>();
      if(status == HabitStatus.Done)
      {
        habits = await habitModel.GetByStatus(chatId, status);
      }
      else
      {
        habits = await habitModel.GetByStatus(chatId, HabitStatus.InProgress);
        habits.AddRange(await habitModel.GetByStatus(chatId, HabitStatus.Undone));
      }
      var messageBuilder = new StringBuilder();
      messageBuilder.AppendLine($"Список всех привычек со статусом: {status}");
      foreach (var habit in habits)
      {
        messageBuilder.AppendLine($"Название привычки: {habit.Title}." +
          $"\nСтатус на {DateOnly.FromDateTime(DateTime.Now)}: {habit.Status}");
      }
      var keyboard = new InlineKeyboardMarkup(new[]
       {
          new[]
          {
            InlineKeyboardButton.WithCallbackData("На главную", "/start")
          }
        });
      await HabitTracker.TelegramBotHandler.SendMessageAsync(botClient, chatId, messageBuilder.ToString(), keyboard, messageId);

      return;
    }


    /// <summary>
    /// Создает новое домашнее задание.
    /// </summary>
    /// <param name="botClient">Клиент Telegram бота.</param>
    /// <param name="chatId">Идентификатор чата.</param>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    public async Task CreateHabitAsync(ITelegramBotClient botClient, long chatId)
    {
      await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Введите название привычки:");
      UserStateTracker.SetUserState(chatId, "awaiting_habit_title");
    }

    /// <summary>
    /// Обрабатывает создание домашнего задания.
    /// </summary>
    /// <param name="botClient">Клиент Telegram бота.</param>
    /// <param name="chatId">Идентификатор чата.</param>
    /// <param name="message">Текст сообщения от учителя.</param>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    public async Task ProcessCreatingHabitAsync(ITelegramBotClient botClient, long chatId, string message)
    {
      var state = UserStateTracker.GetUserState(chatId);

      if (state == "awaiting_habit_title")
      {
        var habitsModel = new CommonHabitsModel(_dbContext);
        habitsModel.Add(_dbContext, chatId, message);
        //await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Привычка успешно добавлена!");
        UserStateTracker.ClearTemporaryData(chatId);
        UserStateTracker.SetUserState(chatId, null);
        var keyboard = new InlineKeyboardMarkup(new[]
       {
          new[]
          {
            InlineKeyboardButton.WithCallbackData("На главную", "/start")
          }
        });
        await HabitTracker.TelegramBotHandler.SendMessageAsync(botClient, chatId, "Привычка успешно добавлена!", keyboard);

        return;
      }
      
    }

    #endregion

    #region Конструкторы

    public User(string name, long chatId, HabitTrackerContext dbContext)
      : base(name, chatId) 
    {
      _dbContext = dbContext;
    }

    #endregion
  }
}
