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
  /// <summary>
  /// 
  /// </summary>
  public class User : UserEntity, IMessageHandler
  {
    #region Поля и свойства

    /// <summary>
    /// Контекст базы данных.
    /// </summary>
    private HabitTrackerContext _dbContext;

    #endregion

    #region Методы

    /// <summary>
    /// Обработка сообщений от пользователя.
    /// </summary>
    /// <param name="botClient">Клиент Telegram бота.</param>
    /// <param name="chatId">Уникальный идентификатор чата пользователя.</param>
    /// <param name="message">Сообщение пользователя.</param>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    public async Task ProcessMessageAsync(ITelegramBotClient botClient, long chatId, string message)
    {
      if (string.IsNullOrEmpty(message))
      {
        return;
      }

      if (message.ToLower().Contains("/start"))
      {
        if (!string.IsNullOrEmpty(UserStateTracker.GetUserState(chatId)))
        {
          UserStateTracker.ClearTemporaryData(chatId);
          UserStateTracker.SetUserState(chatId, null);
        }
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
      else if (!string.IsNullOrEmpty(UserStateTracker.GetUserState(chatId))
        &&(UserStateTracker.GetUserState(chatId) == "awaiting_habit_title"))
      {
        await ProcessCreatingHabitAsync(botClient, chatId, message);
      }
      else if (!string.IsNullOrEmpty(UserStateTracker.GetUserState(chatId))
        &&(UserStateTracker.GetUserState(chatId).Contains("awaiting_habit_rename")))
      {
        await ProcessCreatingHabitAsync(botClient, chatId, message);
      }
      else
      {
        var keyboard = new InlineKeyboardMarkup(new[]
         {
          new[]
          {
            InlineKeyboardButton.WithCallbackData("На главную", "/start")
          },

    });
        await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Введена неизвестная команда ", keyboard);
      }
    }

    /// <summary>
    /// Обработка callback-запросов от пользователя.
    /// </summary>
    /// <param name="botClient">Клиент Telegram бота.</param>
    /// <param name="chatId">Уникальный идентификатор чата пользователя.</param>
    /// <param name="callbackData">Текст callback-запроса.</param>
    /// <param name="messageId">Уникальный идентификатор сообщения пользователя.</param>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
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
          await GetHabits(botClient, chatId, messageId, callbackData);
        }
        else if (callbackData.Contains("/get_undoneHabits"))
        {
          await GetHabitsByStatus(botClient, chatId, messageId, HabitStatus.Undone, callbackData);
        }
        else if (callbackData.Contains("/get_doneHabits"))
        {
          await GetHabitsByStatus(botClient, chatId, messageId, HabitStatus.Done, callbackData);
        }
        else if (callbackData.Contains("/addNewHabit"))
        {
          await CreateHabitAsync(botClient, chatId);
        }
        else if(callbackData.Contains($"{HabitStatus.Undone}_habit_") || callbackData.Contains($"{HabitStatus.Done}_habit_")
          || callbackData.Contains($"{HabitStatus.InProgress}_habit_"))
        {
          var habitId = callbackData.Remove(0, callbackData.LastIndexOf('_')+1);
          var keyboard = new InlineKeyboardMarkup(new[]
          {
            new[]
            {
              InlineKeyboardButton.WithCallbackData("Переименовать привычку", $"/edit_habit_{habitId}")
            },
            new[]
            {
              InlineKeyboardButton.WithCallbackData("Отметить выполнение", $"/mark_as_done_{habitId}")
            },
            new[]
            {
              InlineKeyboardButton.WithCallbackData("Удалить привычку", $"/delete_habit_{habitId}")
            },
            new[]
            {
              InlineKeyboardButton.WithCallbackData("На главную", "/start")
            },
          });
          var habit = await GetHabitById(chatId, Guid.Parse(habitId));
          await TelegramBotHandler.SendMessageAsync(botClient, chatId, $"Выберите действие с привычкой {habit.Title}: ", keyboard, messageId);
        }
        else if (callbackData.Contains($"/edit_habit_"))
        {
          var habitId = callbackData.Remove(0, callbackData.LastIndexOf('_') + 1);
          await EditHabitAsync(botClient, chatId, habitId);
        }
        else if (callbackData.Contains($"/mark_as_done_"))
        {
          var habitId = callbackData.Remove(0, callbackData.LastIndexOf('_') + 1);
          await EditHabit(botClient, messageId, habitId, HabitStatus.Done, chatId);
        }
        else if (callbackData.Contains($"/delete_habit_"))
        {
          var habitId = callbackData.Remove(0, callbackData.LastIndexOf('_') + 1);
          await DeleteHabitAsync(botClient, messageId, habitId, chatId);
        }
        else if (callbackData.Contains($"/getStatistics"))
        {
          var habitId = callbackData.Remove(0, callbackData.LastIndexOf('_') + 1);
          await GetHabitStatisticsAsync(_dbContext, botClient, messageId, habitId, chatId);
        }
      }
    }

    private async Task GetHabitStatisticsAsync(HabitTrackerContext _dbContext, ITelegramBotClient botClient, int messageId, string habitId, long chatId)
    {
      var habitsModel = new CommonHabitsModel(_dbContext);
      var habits = await habitsModel.GetAll(chatId);
      var statistics = new UserHabitsStatistics(_dbContext);
      var message = string.Empty;
      if (habits.Count > 0)
      {
        await statistics.GetStatistics(habits);
        message = "Файл со статистикой создан";
      }
      else
      {
        message = "У вас еще нет привычек";
      }
      var keyboard = new InlineKeyboardMarkup(new[]
       {
          new[]
          {
            InlineKeyboardButton.WithCallbackData("На главную", "/start")
          }
        });
      await HabitTracker.TelegramBotHandler.SendMessageAsync(botClient, chatId, message, keyboard, messageId);
    }

    /// <summary>
    /// Асинхронно удаляет привычку из базы данных.
    /// </summary>
    /// <param name="botClient">Клиент Telegram бота.</param>
    /// <param name="messageId">Уникальный идентификатор сообщения.</param>
    /// <param name="habitId">Уникальный идентификатор привычки.</param>
    /// <param name="chatId">Уникальный идентификатор чата.</param>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    private async Task DeleteHabitAsync(ITelegramBotClient botClient, int messageId, string habitId, long chatId)
    {
      var habitsModel = new CommonHabitsModel(_dbContext);
      var habit = await GetHabitById(chatId, Guid.Parse(habitId));
      var title = habit.Title;
      habitsModel.Delete(Guid.Parse(habitId));

      var keyboard = new InlineKeyboardMarkup(new[]
       {
          new[]
          {
            InlineKeyboardButton.WithCallbackData("На главную", "/start")
          }
        });
      await HabitTracker.TelegramBotHandler.SendMessageAsync(botClient, chatId, $"Данные о привычке {title} обновлены", keyboard, messageId);
    }

    /// <summary>
    /// Отредактировать данные о привычке.
    /// </summary>
    /// <param name="botClient">Клииент Telegram бота.</param>
    /// <param name="messageId">Уникальный идетификатор сообщения.</param>
    /// <param name="habitId">Уникальный идентификатор привычки.</param>
    /// <param name="status">Статус привычкии.</param>
    /// <param name="chatId">Уникальный идентификатор чата.</param>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    private async Task EditHabit(ITelegramBotClient botClient, int messageId, string habitId, HabitStatus status, long chatId)
    {
      var habitsModel = new CommonHabitsModel(_dbContext);
      var habit = await GetHabitById(chatId, Guid.Parse(habitId));
      var lastDay = DateOnly.FromDateTime(DateTime.UtcNow);
      var progressDays = habit.ProgressDays + 1;
      habitsModel.Update(habit.Id, habit.Title,  lastDay, status, progressDays);

      var keyboard = new InlineKeyboardMarkup(new[]
       {
          new[]
          {
            InlineKeyboardButton.WithCallbackData("На главную", "/start")
          }
        });
      await HabitTracker.TelegramBotHandler.SendMessageAsync(botClient, chatId, $"Данные о привычке {habit.Title} обновлены", keyboard, messageId);
    }
    /// <summary>
    /// Отредактировать данные о привычке.
    /// </summary>
    /// <param name="botClient">Клиент Telegram бота.</param>
    /// <param name="habitId">Уникальный идентификатор привычки.</param>
    /// <param name="chatId">Уникальный идентификатор чата.</param>
    /// <param name="newTitle">Новое название привычки.</param>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    private async Task EditHabit(ITelegramBotClient botClient, string habitId, long chatId, string newTitle)
    {
      var habitsModel = new CommonHabitsModel(_dbContext);
      var habit = await GetHabitById(chatId, Guid.Parse(habitId));
      habitsModel.Update(habit.Id, newTitle, habit.LastExecutionDate, habit.Status, habit.ProgressDays);
    }

    /// <summary>
    /// Получить привычку по ее идентификатору.
    /// </summary>
    /// <param name="chatId">Уникальный идентификатор чата пользователя.</param>
    /// <param name="habitId">Уникальный идентификатор привычки.</param>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    private async Task<HabitEntity?> GetHabitById(long chatId, Guid habitId)
    {
      var habitModel = new CommonHabitsModel(_dbContext);
      return await habitModel.GetById(chatId, habitId);
    }

    /// <summary>
    /// Получить список всех привычек пользователя.
    /// </summary>
    /// <param name="botClient">Клиент Telegram бота.</param>
    /// <param name="chatId">Уникальный идентификатор чата пользователя.</param>
    /// <param name="messageId">Уникальный идентификатор сообщения пользователя.</param>
    /// <param name="callbackData">Текст callback-запроса.</param>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    private async Task GetHabits(ITelegramBotClient botClient, long chatId, int messageId, string callbackData)
    {
      var habitModel = new CommonHabitsModel(_dbContext);
      var habits = await habitModel.GetAll(chatId);

      var messageBuilder = new StringBuilder();
      messageBuilder.AppendLine($"Список всех привычек");
      foreach(var habit in habits)
      {
        messageBuilder.AppendLine($"Название привычки: {habit.Title}." +
          $"\nСтатус на {DateOnly.FromDateTime(DateTime.Now)}: {habit.Status}" +
          $"\nКоличество дней прогресса: {habit.ProgressDays}");
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
    /// Получить список привычек по статусу.
    /// </summary>
    /// <param name="botClient">Клиент Telegram бота.</param>
    /// <param name="chatId">Уникальный идентификатор чата пользователя.</param>
    /// <param name="messageId">Уникальный идентификатор сообщения пользователя.</param>
    /// <param name="status">Статус привычки.</param>
    /// <param name="callbackData">Текст callback-запроса.</param>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    private async Task GetHabitsByStatus(ITelegramBotClient botClient, long chatId, int messageId, HabitStatus status, string callbackData)
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
      await DisplayHabitsButtons(botClient, chatId, callbackData, messageId, habits);
      return;
    }

    /// <summary>
    /// Представить данные о привычках в виде кнопок.
    /// </summary>
    /// <param name="botClient">Клиент Telegram бота.</param>
    /// <param name="chatId">Уникальный идентификатор чата пользователя.</param>
    /// <param name="callbackData">Текст callback-запроса.</param>
    /// <param name="messageId">Уникальный идентификатор сообщения пользователя.</param>
    /// <param name="habits">Коллекция привычек пользователя.</param>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    public async Task DisplayHabitsButtons(ITelegramBotClient botClient, long chatId, string callbackData, int messageId, List<HabitEntity> habits)
    {
      var buttons = new List<InlineKeyboardButton[]>();

      foreach (var habit in habits)
      {
        var button = InlineKeyboardButton.WithCallbackData(
            text: habit.Title,
            callbackData: $"{habit.Status}_habit_{habit.Id}"
        );

        buttons.Add(new[] { button });
      }

      buttons.Add(new[]
      {
        InlineKeyboardButton.WithCallbackData("На главную", "/start")
      });

      var inlineKeyboard = new InlineKeyboardMarkup(buttons);

      await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Выберите привычку", inlineKeyboard, messageId);
    }

    /// <summary>
    /// Создает новую привычку.
    /// </summary>
    /// <param name="botClient">Клиент Telegram бота.</param>
    /// <param name="chatId">Уникальный идентификатор чата пользователя.</param>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    public async Task CreateHabitAsync(ITelegramBotClient botClient, long chatId)
    {
      await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Введите название привычки:");
      UserStateTracker.SetUserState(chatId, "awaiting_habit_title");
    }

    /// <summary>
    /// Создает новую привычку.
    /// </summary>
    /// <param name="botClient">Клиент Telegram бота.</param>
    /// <param name="chatId">Уникальный идентификатор чата пользователя.</param>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    public async Task EditHabitAsync(ITelegramBotClient botClient, long chatId, string habitId)
    {
      await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Введите новое название привычки:");
      UserStateTracker.SetUserState(chatId, $"awaiting_habit_rename_{habitId}");
    }

    /// <summary>
    /// Обрабатывает создание привычки и редактирование ее данных.
    /// </summary>
    /// <param name="botClient">Клиент Telegram бота.</param>
    /// <param name="chatId">Уникальный идентификатор чата пользователя.</param>
    /// <param name="message">Текст сообщения от пользователя.</param>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    public async Task ProcessCreatingHabitAsync(ITelegramBotClient botClient, long chatId, string message)
    {
      var state = UserStateTracker.GetUserState(chatId);

      if (state == "awaiting_habit_title")
      {
        var habitsModel = new CommonHabitsModel(_dbContext);
        habitsModel.Add(_dbContext, chatId, message);
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
      else if(state.Contains("awaiting_habit_rename"))
      {
        var habitId = state.Remove(0, state.LastIndexOf('_') + 1);
        await EditHabit(botClient, habitId, chatId, message);
        UserStateTracker.ClearTemporaryData(chatId);
        UserStateTracker.SetUserState(chatId, null);
        var keyboard = new InlineKeyboardMarkup(new[]
       {
          new[]
          {
            InlineKeyboardButton.WithCallbackData("На главную", "/start")
          }
        });
        await HabitTracker.TelegramBotHandler.SendMessageAsync(botClient, chatId, $"Название привычки успешно изменено на {message}!", keyboard);

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
