using Aspose.Cells.Charts;
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
        && (UserStateTracker.GetUserState(chatId) == "awaiting_habit_title"))
      {
        await ProcessCreatingHabitAsync(botClient, chatId, message);
      }
      else if (!string.IsNullOrEmpty(UserStateTracker.GetUserState(chatId))
        && UserStateTracker.GetUserState(chatId) == "awaiting_habit_execution_day")
      {
        await ProcessCreatingHabitAsync(botClient, chatId, message);
      }
      else if (!string.IsNullOrEmpty(UserStateTracker.GetUserState(chatId))
        && UserStateTracker.GetUserState(chatId) == "awaiting_habit_frequency")
      {
        await ProcessCreatingHabitAsync(botClient, chatId, message);
      }
      else if (!string.IsNullOrEmpty(UserStateTracker.GetUserState(chatId))
        && UserStateTracker.GetUserState(chatId) == "awaiting_habit_notify_start")
      {
        await ProcessCreatingHabitAsync(botClient, chatId, message);
      }
      else if (!string.IsNullOrEmpty(UserStateTracker.GetUserState(chatId))
        && UserStateTracker.GetUserState(chatId) == "awaiting_habit_notify_end")
      {
        await ProcessCreatingHabitAsync(botClient, chatId, message);
      }
      else if (!string.IsNullOrEmpty(UserStateTracker.GetUserState(chatId))
        && (UserStateTracker.GetUserState(chatId).Contains("awaiting_habit_rename")))
      {
        await ProcessEditingHabitAsync(botClient, chatId, message);
      }
      else if (!string.IsNullOrEmpty(UserStateTracker.GetUserState(chatId))
        && (UserStateTracker.GetUserState(chatId).Contains("awaiting_newex_date_")))
      {
        await ProcessEditingHabitAsync(botClient, chatId, message);
      }
      else if (!string.IsNullOrEmpty(UserStateTracker.GetUserState(chatId))
        && (UserStateTracker.GetUserState(chatId).Contains("awaiting_habit_newfreq_")))
      {
        await ProcessEditingHabitAsync(botClient, chatId, message);
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
          await GetHabbitsButtons(botClient, chatId, messageId);
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
        else if (callbackData.Contains("/get_suspendedHabits"))
        {
          await GetSuspendedHabits(botClient, chatId, messageId, true, callbackData);
        }
        else if (callbackData.Contains("/addNewHabit"))
        {
          await CreateHabitAsync(botClient, chatId);
        }
        else if (!string.IsNullOrEmpty(UserStateTracker.GetUserState(chatId))
          && UserStateTracker.GetUserState(chatId) == "awaiting_habit_necessary")
        {
          await ProcessCreatingHabitAsync(botClient, chatId, callbackData);
        }
        else if (!string.IsNullOrEmpty(UserStateTracker.GetUserState(chatId))
          && UserStateTracker.GetUserState(chatId) == "awaiting_habit_execution_day")
        {
          await ProcessCreatingHabitAsync(botClient, chatId, callbackData);
        }
        else if (callbackData.Contains($"{HabitStatus.Undone}_habit_") || callbackData.Contains($"{HabitStatus.Done}_habit_")
          || callbackData.Contains($"{HabitStatus.InProgress}_habit_"))
        {
          await HabitActionsButtons(botClient, chatId, callbackData, messageId);
        }
        else if (callbackData.Contains("/edit_habit_"))
        {
          await EditHabitButtons(botClient, chatId, callbackData, messageId);
        }
        else if (callbackData.Contains($"/rename_"))
        {
          var habitId = callbackData.Remove(0, callbackData.LastIndexOf('_') + 1);
          var state = $"awaiting_habit_rename_{habitId}";
          var message = "Введите новое название привычки:";
          await EditHabitAsync(botClient, chatId, message, state);
        }
        else if (callbackData.Contains("/edit_freq_"))
        {
          var habitId = callbackData.Remove(0, callbackData.LastIndexOf('_') + 1);
          var state = $"awaiting_habit_newfreq_{habitId}";
          var message = "Введите частоту выполнения привычкии за день:";
          await EditHabitAsync(botClient, chatId, message, state);
        }
        else if (callbackData.Contains("/edit_ex_date_"))
        {
          var habitId = callbackData.Remove(0, callbackData.LastIndexOf('_') + 1);
          var habitModel = new CommonHabitsModel(_dbContext);
          var foundHabit = await habitModel.GetById(chatId, Guid.Parse(habitId));
          var creationDate = foundHabit.CreationDate;
          var state = $"awaiting_newex_date_{habitId}";
          var message = $"Введите новое количество дней выполнения привычки отностельно даты ее создания ({creationDate}):";
          await EditHabitAsync(botClient, chatId, message, state);
        }
        else if (callbackData.Contains("/suspended"))
        {
          var habitId = callbackData.Remove(0, callbackData.LastIndexOf('_') + 1);
          await SuspendHabit(botClient, messageId, habitId, true, chatId);          
        }
        else if (callbackData.Contains("/unsuspended"))
        {
          var habitId = callbackData.Remove(0, callbackData.LastIndexOf('_') + 1);
          await SuspendHabit(botClient, messageId, habitId, false, chatId);          
        }
        else if (callbackData.Contains($"/mark_as_done_"))
        {
          var habitId = callbackData.Remove(0, callbackData.LastIndexOf('_') + 1);
          await MarkHabitComplete(botClient, messageId, habitId, HabitStatus.Done, chatId);
        }
        else if (callbackData.Contains($"/delete_habit_"))
        {
          var habitId = callbackData.Remove(0, callbackData.LastIndexOf('_') + 1);
          await AssertHabitDelete(botClient, messageId, habitId, chatId);
        }
        else if (callbackData.Contains("/assert_delete_"))
        {
          var habitId = callbackData.Remove(0, callbackData.LastIndexOf('_') + 1);
          await DeleteHabitAsync(botClient, messageId, habitId, chatId);
        }
        else if (callbackData.Contains($"/getStatistics"))
        {
          var habitId = callbackData.Remove(0, callbackData.LastIndexOf('_') + 1);
          await GetHabitStatisticsAsync(_dbContext, botClient, messageId, habitId, chatId);
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
    }

    private async Task HabitActionsButtons(ITelegramBotClient botClient, long chatId, string callbackData, int messageId)
    {
      var habitId = callbackData.Remove(0, callbackData.LastIndexOf('_') + 1);
      var habit = await GetHabitById(chatId, Guid.Parse(habitId));
      if(habit.IsSuspended == false)
      {
        var keyboard = new InlineKeyboardMarkup(new[]
      {
        new[]
        {
          InlineKeyboardButton.WithCallbackData("Изменить настройки привычки", $"/edit_habit_{habitId}")
        },
        new[]
        {
          InlineKeyboardButton.WithCallbackData("Отметить выполнение", $"/mark_as_done_{habitId}")
        },
        new[]
        {
          InlineKeyboardButton.WithCallbackData("Приостановить привычку", $"/suspended_{habitId}")
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
        await TelegramBotHandler.SendMessageAsync(botClient, chatId, $"Выберите действие с привычкой {habit.Title}: ", keyboard, messageId);
      }
      else
      {
        var keyboard = new InlineKeyboardMarkup(new[]
      {
        new[]
        {
          InlineKeyboardButton.WithCallbackData("Возобновить привычку", $"/unsuspended_{habitId}")
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
        await TelegramBotHandler.SendMessageAsync(botClient, chatId, $"Выберите действие с привычкой {habit.Title}: ", keyboard, messageId);
      }
      
      
    }

    private static async Task GetHabbitsButtons(ITelegramBotClient botClient, long chatId, int messageId)
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
          InlineKeyboardButton.WithCallbackData("Получить список приостановленных привычек", "/get_suspendedHabits")
        },
        new[]
        {
          InlineKeyboardButton.WithCallbackData("На главную", "/start")
        },
      });
      await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Выберите действие: ", keyboard, messageId);
    }

    private async Task EditHabitButtons(ITelegramBotClient botClient, long chatId, string callbackData, int messageId)
    {
      var habitId = callbackData.Remove(0, callbackData.LastIndexOf('_') + 1);
      var keyboard = new InlineKeyboardMarkup(new[]
      {
            new[]
            {
              InlineKeyboardButton.WithCallbackData("Переименовать привычку", $"/rename_{habitId}")
            },
            new[]
            {
              InlineKeyboardButton.WithCallbackData("Изменить срок выполнения привычки", $"/edit_ex_date_{habitId}")
            },
            new[]
            {
              InlineKeyboardButton.WithCallbackData("Изменить частоту выполнения привычки за день", $"/edit_freq_{habitId}")
            },
            new[]
            {
               InlineKeyboardButton.WithCallbackData("Изменить период получения уведомлений", $"/edit_notify_interval_{habitId}")
            },
            new[]
            {
              InlineKeyboardButton.WithCallbackData("На главную", "/start")
            },
          });
      var habit = await GetHabitById(chatId, Guid.Parse(habitId));
      await TelegramBotHandler.SendMessageAsync(botClient, chatId, $"Выберите действие с привычкой {habit.Title}: ", keyboard, messageId);
    }

    /// <summary>
    /// Получить статистику пользователя по привычкам и отправить ее пользователю в формате .jpg.
    /// </summary>
    /// <param name="_dbContext">Контекст базы данных.</param>
    /// <param name="botClient">Клиент Telegram бота.</param>
    /// <param name="messageId">Уникальный идентификатор сообщения.</param>
    /// <param name="habitId">Уникальный идентификатор привычки.</param>
    /// <param name="chatId">Уникальный идентификатор чата.</param>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    private async Task GetHabitStatisticsAsync(HabitTrackerContext _dbContext, ITelegramBotClient botClient, int messageId, string habitId, long chatId)
    {
      var habitsModel = new CommonHabitsModel(_dbContext);
      var habits = await habitsModel.GetAllActive(chatId);
      var message = string.Empty;
      if (habits.Count > 0)
      {
        await UserHabitsStatistics.SendStatistics(botClient, chatId, habits);
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
      await HabitTracker.TelegramBotHandler.SendMessageAsync(botClient, chatId, message, keyboard);
    }

    /// <summary>
    /// Асинхронно удаляет привычку из базы данных.
    /// </summary>
    /// <param name="botClient">Клиент Telegram бота.</param>
    /// <param name="messageId">Уникальный идентификатор сообщения.</param>
    /// <param name="habitId">Уникальный идентификатор привычки.</param>
    /// <param name="chatId">Уникальный идентификатор чата.</param>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    private async Task AssertHabitDelete(ITelegramBotClient botClient, int messageId, string habitId, long chatId)
    {
      var habitsModel = new CommonHabitsModel(_dbContext);
      var habit = await GetHabitById(chatId, Guid.Parse(habitId));
      var title = habit.Title;

      var keyboard = new InlineKeyboardMarkup(new[]
        {
          new[]
          {
            InlineKeyboardButton.WithCallbackData("Подтвердить удаление", $"/assert_delete_{habitId}")
          },
          new[]
          {
            InlineKeyboardButton.WithCallbackData("Отмена", $"/cancel_{habitId}")
          },
          new[]
          {
            InlineKeyboardButton.WithCallbackData("На главную", "/start")
          }
        });
      await HabitTracker.TelegramBotHandler.SendMessageAsync(botClient, chatId, $"Вы уверены, что хотите удалить привычку '{title}'?",
        keyboard, messageId);
    }
    private async Task DeleteHabitAsync(ITelegramBotClient botClient, int messageId, string habitId, long chatId)
    {
      var habitsModel = new CommonHabitsModel(_dbContext);
      var habit = await GetHabitById(chatId, Guid.Parse(habitId));
      var title = habit.Title;

      var practicedHabitsModel = new CommonPracticedHabitModel(_dbContext);
      practicedHabitsModel.Delete(Guid.Parse(habitId));
      Thread.Sleep(1000);
      habitsModel.Delete(Guid.Parse(habitId));

      var keyboard = new InlineKeyboardMarkup(new[]
        {
          new[]
          {
            InlineKeyboardButton.WithCallbackData("На главную", "/start")
          }
        });
      await HabitTracker.TelegramBotHandler.SendMessageAsync(botClient, chatId, $"Привычка '{title}' успешно удалена",
        keyboard, messageId);
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
    private async Task MarkHabitComplete(ITelegramBotClient botClient, int messageId, string habitId, HabitStatus status, long chatId)
    {
      var habitsModel = new CommonHabitsModel(_dbContext);
      var practicedHabitsModel = new CommonPracticedHabitModel(_dbContext);
      var habit = await GetHabitById(chatId, Guid.Parse(habitId));
      var lastDay = DateOnly.FromDateTime(DateTime.UtcNow);
      var progressDays = habit.ProgressDays + 1;
      habitsModel.Update(habit.Id, habit.Title, lastDay, status, progressDays, habit.ExpirationDate, 
        habit.NumberOfExecutions, habit.IsSuspended);
      Thread.Sleep(1000);
      practicedHabitsModel.Add(habit.Id, DateTime.UtcNow);

      var keyboard = new InlineKeyboardMarkup(new[]
       {
          new[]
          {
            InlineKeyboardButton.WithCallbackData("На главную", "/start")
          }
        });
      await HabitTracker.TelegramBotHandler.SendMessageAsync(botClient, chatId, $"Привычка {habit.Title} отмечена как выполненная",
        keyboard, messageId);
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
    private async Task SuspendHabit(ITelegramBotClient botClient, int messageId, string habitId, bool isSuspended, long chatId)
    {
      var habitsModel = new CommonHabitsModel(_dbContext);
      var habit = await GetHabitById(chatId, Guid.Parse(habitId));

      habitsModel.Update(habit.Id, habit.Title, habit.LastExecutionDate, habit.Status, habit.ProgressDays,
        habit.ExpirationDate, habit.NumberOfExecutions, isSuspended);

      var message = $"Привычка {habit.Title} приостановлена";
      if (!isSuspended)
      {
        message = $"Привычка {habit.Title} возобновлена";
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
    /// Отредактировать данные о привычке.
    /// </summary>
    /// <param name="botClient">Клиент Telegram бота.</param>
    /// <param name="habitId">Уникальный идентификатор привычки.</param>
    /// <param name="chatId">Уникальный идентификатор чата.</param>
    /// <param name="newTitle">Новое название привычки.</param>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    private async Task EditHabit(ITelegramBotClient botClient, string habitId, long chatId, string message, string state)
    {
      var habitsModel = new CommonHabitsModel(_dbContext);
      var habit = await GetHabitById(chatId, Guid.Parse(habitId));
      if (state.Contains("awaiting_habit_rename_"))
      {
        habitsModel.Update(habit.Id, message, habit.LastExecutionDate, habit.Status, habit.ProgressDays, 
          habit.ExpirationDate, habit.NumberOfExecutions, habit.IsSuspended);
      }
      else if (state.Contains("awaiting_newex_date_"))
      {
        if (int.TryParse(message, out int value))
        {
          var creationDate = habit.CreationDate;
          var newExpirationDate = creationDate.AddDays(value);
          habit.ExpirationDate = newExpirationDate.ToDateTime(TimeOnly.MinValue).ToUniversalTime();
        }
        else if (message.Contains("/endless"))
        {
          habit.ExpirationDate = null;
        }
        habitsModel.Update(habit.Id, habit.Title, habit.LastExecutionDate, habit.Status, habit.ProgressDays,
            habit.ExpirationDate, habit.NumberOfExecutions, habit.IsSuspended);
      }
      else if (state.Contains("awaiting_habit_newfreq_"))
      {
        if (int.TryParse(message, out int value))
        {
          habit.NumberOfExecutions = value;
          habitsModel.Update(habit.Id, habit.Title, habit.LastExecutionDate, habit.Status, habit.ProgressDays,
            habit.ExpirationDate, habit.NumberOfExecutions, habit.IsSuspended);
        }
      }
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
      var habits = await habitModel.GetAllActive(chatId);

      var messageBuilder = new StringBuilder();
      messageBuilder.AppendLine($"Список всех привычек");
      foreach (var habit in habits)
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
      if (status == HabitStatus.Done)
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
    /// Получить список привычек по статусу.
    /// </summary>
    /// <param name="botClient">Клиент Telegram бота.</param>
    /// <param name="chatId">Уникальный идентификатор чата пользователя.</param>
    /// <param name="messageId">Уникальный идентификатор сообщения пользователя.</param>
    /// <param name="status">Статус привычки.</param>
    /// <param name="callbackData">Текст callback-запроса.</param>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    private async Task GetSuspendedHabits(ITelegramBotClient botClient, long chatId, int messageId, bool isSuspended, string callbackData)
    {
      var habitModel = new CommonHabitsModel(_dbContext);
      List<HabitEntity>? habits = new List<HabitEntity>();
      if (isSuspended == true)
      {
        habits = await habitModel.GetSuspendedHabit(chatId);
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
    /// Отредактировать привычку.
    /// </summary>
    /// <param name="botClient">Клиент Telegram бота.</param>
    /// <param name="chatId">Уникальный идентификатор чата пользователя.</param>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    public async Task EditHabitAsync(ITelegramBotClient botClient, long chatId, string message, string state)
    {
      await TelegramBotHandler.SendMessageAsync(botClient, chatId, message);
      UserStateTracker.SetUserState(chatId, state);
    }

    /// <summary>
    /// Обрабатывает создание привычки.
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
        UserStateTracker.SetTemporaryData(chatId, "habit_title", message);
        var keyboard = new InlineKeyboardMarkup(new[]
        {
          new[]
          {
            InlineKeyboardButton.WithCallbackData("Обязательно", "/necessary")
          },
          new[]
          {
            InlineKeyboardButton.WithCallbackData("Не обязательно", "/unnecessary")
          }
        });
        await HabitTracker.TelegramBotHandler.SendMessageAsync(botClient, chatId, $"Выберите необходимость выполнения привычки ежедневно привычки",
          keyboard);

        UserStateTracker.SetUserState(chatId, "awaiting_habit_necessary");
      }
      else if (state == "awaiting_habit_necessary")
      {
        if (message.Contains("necessary"))
        {
          UserStateTracker.SetTemporaryData(chatId, "habit_necessary", "true");
        }
        else if (message.Contains("unnecessary"))
        {
          UserStateTracker.SetTemporaryData(chatId, "habit_necessary", "false");
        }
        var keyboard = new InlineKeyboardMarkup(new[]
      {
          new[]
          {
            InlineKeyboardButton.WithCallbackData("Не устанавливать срок", "/endless")
          }
        });
        await HabitTracker.TelegramBotHandler.SendMessageAsync(botClient, chatId, $"Введите число дней выполнения привычки." +
          $"\nЕсли хотите сделать привычку бессрочной, нажмите кнопку 'Не устанавливать срок'",
          keyboard);
        UserStateTracker.SetUserState(chatId, "awaiting_habit_execution_day");

      }
      else if (state == "awaiting_habit_execution_day")
      {
        if (int.TryParse(message, out int value))
        {
          UserStateTracker.SetTemporaryData(chatId, "habit_execution_day", message);
          await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Введите число того,  " +
            "день вы хотите выполнять привычку:");
          UserStateTracker.SetUserState(chatId, "awaiting_habit_frequency");
        }
        else if (message.Contains("/endless"))
        {
          UserStateTracker.SetTemporaryData(chatId, "habit_frequency", "null");
          await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Введите число того, сколько раз за " +
            "день вы хотите выполнять привычку:");
          UserStateTracker.SetUserState(chatId, "awaiting_habit_frequency");
        }
      }
      else if (state == "awaiting_habit_frequency")
      {
        if (int.TryParse(message, out int value))
        {
          UserStateTracker.SetTemporaryData(chatId, "habit_frequency", message);
          await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Введите время начала отправки уведомлений:");
          UserStateTracker.SetUserState(chatId, "awaiting_habit_notify_start");
        }
      }
      else if (state == "awaiting_habit_notify_start")
      {
        if (TimeOnly.TryParse(message, out TimeOnly result))
        {
          UserStateTracker.SetTemporaryData(chatId, "habit_notify_start", message);
          await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Введите время конца отправки уведомлений:");
          UserStateTracker.SetUserState(chatId, "awaiting_habit_notify_end");
        }
      }
      else if (state == "awaiting_habit_notify_end")
      {
        var habitsModel = new CommonHabitsModel(_dbContext);
        var title = UserStateTracker.GetTemporaryData(chatId, "habit_title");
        var numberOfExecutions = int.Parse(UserStateTracker.GetTemporaryData(chatId, "habit_frequency"));
        DateTime? days = null;
        if (int.TryParse(UserStateTracker.GetTemporaryData(chatId, "habit_execution_day"), out int daysNumber))
        {
          days = DateTime.Now.AddDays(daysNumber);
        }
        var isNecessary = bool.Parse(UserStateTracker.GetTemporaryData(chatId, "habit_necessary"));
        habitsModel.Add(chatId, title, numberOfExecutions, days, isNecessary);
        UserStateTracker.ClearTemporaryData(chatId);
        UserStateTracker.SetUserState(chatId, null);
        var keyboard = new InlineKeyboardMarkup(new[]
       {
          new[]
          {
            InlineKeyboardButton.WithCallbackData("На главную", "/start")
          }
        });
        await HabitTracker.TelegramBotHandler.SendMessageAsync(botClient, chatId, $"Привычка {title} успешно добавлена!", keyboard);

        return;
      }

    }

    /// <summary>
    /// Обрабатывает редактирование данных привычки.
    /// </summary>
    /// <param name="botClient">Клиент Telegram бота.</param>
    /// <param name="chatId">Уникальный идентификатор чата пользователя.</param>
    /// <param name="message">Текст сообщения от пользователя.</param>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    public async Task ProcessEditingHabitAsync(ITelegramBotClient botClient, long chatId, string message)
    {
      var state = UserStateTracker.GetUserState(chatId);
      if (!string.IsNullOrEmpty(state))
      {
        var habitId = state.Remove(0, state.LastIndexOf('_') + 1);
        await EditHabit(botClient, habitId, chatId, message, state);
        UserStateTracker.ClearTemporaryData(chatId);
        UserStateTracker.SetUserState(chatId, null);
        var keyboard = new InlineKeyboardMarkup(new[]
       {
          new[]
          {
            InlineKeyboardButton.WithCallbackData("На главную", "/start")
          }
        });
        await HabitTracker.TelegramBotHandler.SendMessageAsync(botClient, chatId, $"Данные о привычке {message} успешно обновлены!", keyboard);

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
