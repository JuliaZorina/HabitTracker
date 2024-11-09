using HabitTracker.Core;
using HabitTracker.Data;
using HabitTracker.Data.Models;
using HabitTracker.Interfaces;
using HabitTracker.Models;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace HabitTracker
{
  /// <summary>
  /// Взаимодействие пользователя с клиентом Telegram-бота.
  /// </summary>
  public class User : UserEntity, IMessageHandler
  {
    #region Поля и свойства

    /// <summary>
    /// Фабрика создания контекста базы данных
    /// </summary>
    private readonly DbContextFactory _dbContextFactory;
    /// <summary>
    /// Аргументы.
    /// </summary>
    private readonly string[] _args;

    #endregion

    #region Методы

    #region Обработка сообщений

    /// <summary>
    /// Обработка сообщений от пользователя.
    /// </summary>
    /// <param name="botClient">Клиент Telegram бота.</param>
    /// <param name="chatId">Уникальный идентификатор чата пользователя.</param>
    /// <param name="message">Сообщение пользователя.</param>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    public async Task ProcessMessageAsync(ITelegramBotClient botClient, long chatId, string message)
    {
      try
      {
        if (string.IsNullOrEmpty(message))
        {
          return;
        }

        if (message.ToLower().Contains("/start"))
        {
          if (!string.IsNullOrEmpty(UserStateTracker.GetUserState(chatId)) &&
            UserStateTracker.GetUserState(chatId).Contains("awaiting_notification_settings"))
          {
            await SetNotificationSettings(botClient, chatId);
          }
          else if (!string.IsNullOrEmpty(UserStateTracker.GetUserState(chatId)))
          {
            UserStateTracker.ClearTemporaryData(chatId);
            UserStateTracker.SetUserState(chatId, null);
          }
          if (string.IsNullOrEmpty(UserStateTracker.GetUserState(chatId)))
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
            new[]
            {
              InlineKeyboardButton.WithCallbackData("Настроить период отправки уведомлений", "/notificationsSettings")
            },

          });
            await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Выберите действие: ", keyboard);
          }
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
        else if (!string.IsNullOrEmpty(UserStateTracker.GetUserState(chatId))
          && UserStateTracker.GetUserState(chatId).Contains("awaiting_new_start_time"))
        {
          await ProcessSetNotificationsTimeAsync(botClient, chatId, message);
        }
        else if (!string.IsNullOrEmpty(UserStateTracker.GetUserState(chatId))
          && (UserStateTracker.GetUserState(chatId).Contains("awaiting_notification_end_time")))
        {
          await ProcessSetNotificationsTimeAsync(botClient, chatId, message);
        }
        else if (!string.IsNullOrEmpty(UserStateTracker.GetUserState(chatId))
          && (UserStateTracker.GetUserState(chatId).Contains("awaiting_notification_start_time")))
        {
          await ProcessSetNotificationsTimeAsync(botClient, chatId, message);
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
      catch (Exception ex)
      {
        Console.WriteLine($"Отладка: Произошла ошибка - {ex.Message}");
        Debug.WriteLine($"Отладка: Подробности исключения - {ex}");
        var keyboard = new InlineKeyboardMarkup(new[]
            {
              new[]
              {
                InlineKeyboardButton.WithCallbackData("На главную", "/start")
              },

            });
        await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Произошла ошибка. Повторите попытку.", keyboard);
      }
    }

    #endregion

    #region Обработка Callback-запросов

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
      try
      {
        if (string.IsNullOrEmpty(callbackData)) return;
        else
        {
          if (callbackData.ToLower(CultureInfo.CurrentCulture).Contains("/gethabits"))
          {
            await GetHabbitsButtons(botClient, chatId, messageId);
          }
          else if (callbackData.Contains("/start") || callbackData.Contains("/cancel_"))
          {
            try
            {
              await botClient.DeleteMessageAsync(chatId, messageId);
              await Task.Delay(10);
            }
            catch (Telegram.Bot.Exceptions.ApiRequestException)
            {
              Console.WriteLine($"Сообщение с ID {messageId} не найдено.");
            }
            await ProcessMessageAsync(botClient, chatId, "/start");
          }
          else if (callbackData.Contains("/delete_notification"))
          {
            await botClient.DeleteMessageAsync(chatId, messageId);
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

            var habitModel = new CommonHabitsModel(_dbContextFactory, _args);
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
            await MarkHabitComplete(botClient, messageId, habitId, chatId);
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
            await GetHabitsStatisticsAsync(botClient, messageId, chatId);
          }
          else if (callbackData.Contains($"/week_stat_"))
          {
            var habitId = callbackData.Remove(0, callbackData.LastIndexOf('_') + 1);
            await GetHabitWeekStatisticsAsync(botClient, messageId, chatId, habitId);
          }
          else if (callbackData.Contains("/notificationsSettings"))
          {
            await EditNotificationSettings(botClient, messageId, chatId);
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
      catch (Exception ex)
      {
        Console.WriteLine($"Отладка: Произошла ошибка - {ex.Message}");
        Debug.WriteLine($"Отладка: Подробности исключения - {ex}");
        var keyboard = new InlineKeyboardMarkup(new[]
            {
              new[]
              {
                InlineKeyboardButton.WithCallbackData("На главную", "/start")
              },

            });
        await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Произошла ошибка. Повторите попытку.", keyboard);
      }
    }


    #endregion

    /// <summary>
    /// Отредактировать настройки уведомлений.
    /// </summary>
    /// <param name="botClient">Клиент Telegram-бота.</param>
    /// <param name="messageId">Уникальный идентификатор сообщения.</param>
    /// <param name="chatId">Уникальный идентификатора чата пользователя.</param>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    private async Task EditNotificationSettings(ITelegramBotClient botClient, int messageId, long chatId)
    {
      await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Введите новое время начала отправки уведомлений в формате hh:mm");
      UserStateTracker.SetUserState(chatId, "awaiting_new_start_time");
    }

    /// <summary>
    /// Установить настройки уведомлений пользователя.
    /// </summary>
    /// <param name="botClient">Клиент Telegram-бота.</param>
    /// <param name="chatId">Уникальный идентификатор чата пользователя.</param>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    private async Task SetNotificationSettings(ITelegramBotClient botClient, long chatId)
    {
      await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Введите время начала отправки уведомлений в формате hh:mm");
      UserStateTracker.SetUserState(chatId, "awaiting_notification_start_time");
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

    #region Генерация кнопок

    /// <summary>
    /// Генерирует кнопки действий с привычкой.
    /// </summary>
    /// <param name="botClient">Клиент Telegram-бота.</param>
    /// <param name="chatId">Уникальный идентификатор чата пользователя.</param>
    /// <param name="callbackData">Данные Callback-запроса.</param>
    /// <param name="messageId">Уникальный идентификатор сообщения.</param>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    private async Task HabitActionsButtons(ITelegramBotClient botClient, long chatId, string callbackData, int messageId)
    {
      var habitId = callbackData.Remove(0, callbackData.LastIndexOf('_') + 1);
      var habit = await GetHabitById(chatId, Guid.Parse(habitId));
      if (habit.IsPaused == false)
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
          InlineKeyboardButton.WithCallbackData("Получить статистику за последнюю неделю", $"/week_stat_{habitId}")
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
        await TelegramBotHandler.SendMessageAsync(botClient, chatId, $"Выберите действие с привычкой \"{habit.Title}\": ", keyboard, messageId);
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

    /// <summary>
    /// Генерирует кнопки получения данных о привычках по предоставленным фильтрам.
    /// </summary>
    /// <param name="botClient">Клиент Telegram-бота.</param>
    /// <param name="chatId">Уникальный идентификатор чата.</param>
    /// <param name="messageId">Уникальный идентификатор сообщения.</param>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
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

    /// <summary>
    /// Генерирует кнопки редактирования данных привычки.
    /// </summary>
    /// <param name="botClient"></param>
    /// <param name="chatId"></param>
    /// <param name="callbackData"></param>
    /// <param name="messageId"></param>
    /// <returns></returns>
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
          InlineKeyboardButton.WithCallbackData("На главную", "/start")
        },
      });
      var habit = await GetHabitById(chatId, Guid.Parse(habitId));
      await TelegramBotHandler.SendMessageAsync(botClient, chatId, $"Выберите действие с привычкой {habit.Title}: ", keyboard, messageId);
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

    #endregion

    #region Удаление привычки

    /// <summary>
    /// Запрашивает подтверждение от пользователя на удаление привычки.
    /// </summary>
    /// <param name="botClient">Клиент Telegram бота.</param>
    /// <param name="messageId">Уникальный идентификатор сообщения.</param>
    /// <param name="habitId">Уникальный идентификатор привычки.</param>
    /// <param name="chatId">Уникальный идентификатор чата.</param>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    private async Task AssertHabitDelete(ITelegramBotClient botClient, int messageId, string habitId, long chatId)
    {
      var habitsModel = new CommonHabitsModel(_dbContextFactory, _args);
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

    /// <summary>
    /// Асинхронно удаляет привычку и связанные с ней данные из базы данных.
    /// </summary>
    /// <param name="botClient">Клиент Telegram-бота.</param>
    /// <param name="messageId">Уникальный идентификатор сообщения.</param>
    /// <param name="habitId">Уникальный идентификатор привычки.</param>
    /// <param name="chatId">Уникальный идентификатор чата пользователя.</param>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    private async Task DeleteHabitAsync(ITelegramBotClient botClient, int messageId, string habitId, long chatId)
    {
      var habitsModel = new CommonHabitsModel(_dbContextFactory, _args);
      var habit = await GetHabitById(chatId, Guid.Parse(habitId));
      var title = habit.Title;
      var practicedHabitsModel = new CommonPracticedHabitModel(_dbContextFactory, _args);

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

    #endregion

    #region Получение статистики привычек

    /// <summary>
    /// Получить статистику пользователя по привычкам и отправить ее пользователю в формате .jpg.
    /// </summary>
    /// <param name="botClient">Клиент Telegram бота.</param>
    /// <param name="messageId">Уникальный идентификатор сообщения.</param>
    /// <param name="chatId">Уникальный идентификатор чата.</param>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    private async Task GetHabitsStatisticsAsync(ITelegramBotClient botClient, int messageId, long chatId)
    {
      var habitsModel = new CommonHabitsModel(_dbContextFactory, _args);
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
    /// Получить статистику по выбранной привычке за неделю.
    /// </summary>
    /// <param name="botClient"></param>
    /// <param name="messageId"></param>
    /// <param name="chatId"></param>
    /// <param name="habitId"></param>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    private async Task GetHabitWeekStatisticsAsync(ITelegramBotClient botClient, int messageId, long chatId, string habitId)
    {
      var habitsModel = new CommonHabitsModel(_dbContextFactory, _args);
      var habit = await habitsModel.GetById(chatId, Guid.Parse(habitId));
      var messageBuilder = new StringBuilder();
      var today = DateOnly.FromDateTime(GetTime.GetNetworkTime("time.google.com"));
      var weekStart = today.AddDays(-7);      
      if (habit != null)
      {
        messageBuilder.AppendLine($"Статистика выполнения привычки \"{habit.Title}\" за период {weekStart} - {today}");
        var foundPractice = new List<PracticedHabitEntity>();
        var practicedHabitsModel = new CommonPracticedHabitModel(_dbContextFactory, _args);
        for (var i = weekStart; i <= today; i = i.AddDays(1))
        {
          var practiceInDay = await practicedHabitsModel.GetByDateAndHabitId(Guid.Parse(habitId), i);
          var dayOfWeek = CultureInfo.CurrentCulture.DateTimeFormat.GetDayName(i.DayOfWeek);
          if (practiceInDay.Count >= habit.NumberOfExecutions)
          {
            messageBuilder.AppendLine($"{dayOfWeek}. Статус: выполнено{char.ConvertFromUtf32(0x2705)}");
          }
          else if (practiceInDay.Count == 0 && i >= habit.CreationDate)
          {
            messageBuilder.AppendLine($"{dayOfWeek}. Статус: не выполнено{char.ConvertFromUtf32(0x274C)}");
          }
          else if(i >= habit.CreationDate)
          {
            messageBuilder.AppendLine($"{dayOfWeek}. Статус: выполнено не полностью{char.ConvertFromUtf32(0x1F4DD)}");
          }
        }
      }
      else
      {
        messageBuilder.Append("Данные о привычке не найдены в базе данных.");
      }
      var keyboard = new InlineKeyboardMarkup(new[]
       {
          new[]
          {
            InlineKeyboardButton.WithCallbackData("На главную", "/start")
          }
        });
      await HabitTracker.TelegramBotHandler.SendMessageAsync(botClient, chatId, messageBuilder.ToString(), keyboard);
    }

    #endregion

    /// <summary>
    /// Отредактировать данные о привычке.
    /// </summary>
    /// <param name="botClient">Клииент Telegram бота.</param>
    /// <param name="messageId">Уникальный идетификатор сообщения.</param>
    /// <param name="habitId">Уникальный идентификатор привычки.</param>
    /// <param name="chatId">Уникальный идентификатор чата.</param>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    private async Task MarkHabitComplete(ITelegramBotClient botClient, int messageId, string habitId, long chatId)
    {
      var habitsModel = new CommonHabitsModel(_dbContextFactory, _args);
      var habit = await GetHabitById(chatId, Guid.Parse(habitId));
      var practicedHabitsModel = new CommonPracticedHabitModel(_dbContextFactory, _args);

      if (habit != null)
      {
        var ntwTime = GetTime.GetNetworkTime("time.google.com");
        //var ntwTime = DateTime.Now.AddDays(1);
        practicedHabitsModel.Add(habit.Id, ntwTime);
        Thread.Sleep(1000);
        HabitStatus status = habit.Status;
        long progressDays = 0;
        if (!habit.IsNecessary ||
          (habit.IsNecessary && habit.LastExecutionDate == DateOnly.FromDateTime(ntwTime).AddDays(-1))
          || habit.LastExecutionDate == DateOnly.FromDateTime(ntwTime))
        {
          progressDays = habit.ProgressDays;
        }
        var lastDay = DateOnly.FromDateTime(ntwTime);
        var countPractice = 0;
        var foundHabitPractice = await practicedHabitsModel.GetByDateAndHabitId(habit.Id, DateOnly.FromDateTime(ntwTime));
        if (foundHabitPractice != null)
        {
          countPractice = foundHabitPractice.Count;
          if (countPractice == habit.NumberOfExecutions)
          {
            progressDays++;
            status = HabitStatus.Done;
          }
          else if (countPractice < habit.NumberOfExecutions)
          {
            status = HabitStatus.InProgress;
          }
          else if (countPractice > habit.NumberOfExecutions)
          {
            status = HabitStatus.Done;
          }

          habitsModel.Update(habit.Id, habit.Title, lastDay, status, progressDays, habit.ExpirationDate,
                            habit.NumberOfExecutions, habit.IsPaused);

          var keyboard = new InlineKeyboardMarkup(new[]
          {
            new[]
            {
              InlineKeyboardButton.WithCallbackData("На главную", "/start")
            }
          });
          if (status == HabitStatus.Done)
          {
            await HabitTracker.TelegramBotHandler.SendMessageAsync(botClient, chatId, $"Привычка {habit.Title} отмечена как выполненная",
            keyboard, messageId);
          }
          else if (status == HabitStatus.InProgress)
          {
            await HabitTracker.TelegramBotHandler.SendMessageAsync(botClient, chatId, $"Выполнение привычки {habit.Title} отмечено." +
              $"\nДля того, чтобы привычка получила статус \"Выполнено\" необходимо выполнить ее еще {habit.NumberOfExecutions - countPractice} раз",
            keyboard, messageId);
          }
        }
      }
      else
      {
        var keyboard = new InlineKeyboardMarkup(new[]
           {
          new[]
          {
            InlineKeyboardButton.WithCallbackData("На главную", "/start")
          }
        });
        await HabitTracker.TelegramBotHandler.SendMessageAsync(botClient, chatId, $"Привычка не найдена в базе данных",
            keyboard, messageId);
      }

    }
    /// <summary>
    /// Изменить состояние активности привычки.
    /// </summary>
    /// <param name="botClient">Клииент Telegram бота.</param>
    /// <param name="messageId">Уникальный идетификатор сообщения.</param>
    /// <param name="habitId">Уникальный идентификатор привычки.</param>
    /// <param name="isSuspended">Статус активности привычкии.</param>
    /// <param name="chatId">Уникальный идентификатор чата.</param>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    private async Task SuspendHabit(ITelegramBotClient botClient, int messageId, string habitId, bool isSuspended, long chatId)
    {
      var habitsModel = new CommonHabitsModel(_dbContextFactory, _args);
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

    #region Получение списка привычек

    /// <summary>
    /// Получить привычку по ее идентификатору.
    /// </summary>
    /// <param name="chatId">Уникальный идентификатор чата пользователя.</param>
    /// <param name="habitId">Уникальный идентификатор привычки.</param>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    private async Task<HabitEntity?> GetHabitById(long chatId, Guid habitId)
    {
      var habitModel = new CommonHabitsModel(_dbContextFactory, _args);
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
      var habitsModel = new CommonHabitsModel(_dbContextFactory, _args);
      var practicedHabitsModel = new CommonPracticedHabitModel(_dbContextFactory, _args);
      var habits = await habitsModel.GetAllActive(chatId);
      var messageBuilder = new StringBuilder();
      var status = string.Empty;
      var countPractice = 0;
      messageBuilder.AppendLine($"Список всех привычек");
      foreach (var habit in habits)
      {
        if (habit.Status == HabitStatus.Done)
        {
          status = "Выполнено";
        }
        else if (habit.Status == HabitStatus.InProgress)
        {
          status = "В процессе";
        }
        else
        {
          status = "Не выполнено";
        }
        var foundHabitPractice = await practicedHabitsModel.GetByDateAndHabitId(habit.Id,
          DateOnly.FromDateTime(GetTime.GetNetworkTime("time.google.com")));
        if (foundHabitPractice != null)
        {
          countPractice = foundHabitPractice.Count;
        }
        var repeats = habit.NumberOfExecutions - countPractice;
        messageBuilder.Append($"\nНазвание привычки: {habit.Title}." +
          $"\nСтатус на {DateOnly.FromDateTime(DateTime.Now)}: {status}" +
          $"\nКоличество дней прогресса: {habit.ProgressDays}.\n");
        if (repeats > 0)
        {
          messageBuilder.Append($"До получения статуса \"Выполнено\" осталось {repeats} повторений.\n");
        }
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
      List<HabitEntity>? habits = new List<HabitEntity>();
      var habitModel = new CommonHabitsModel(_dbContextFactory, _args);
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
      List<HabitEntity>? habits = new List<HabitEntity>();
      var habitModel = new CommonHabitsModel(_dbContextFactory, _args);
      if (isSuspended == true)
      {
        habits = await habitModel.GetSuspendedHabit(chatId);
      }
      await DisplayHabitsButtons(botClient, chatId, callbackData, messageId, habits);
      return;
    }

    #endregion


    #region Процессы создания, редактирования данных привычек

    /// <summary>
    /// Обрабатывает процесс создания привычки.
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
          if (value <= 0)
          {
            await HabitTracker.TelegramBotHandler.SendMessageAsync(botClient, chatId, $"Количество дней не может быть отрицательным или равным 0." +
              $"\nВведите корректное значение.");
          }
          else
          {
            UserStateTracker.SetTemporaryData(chatId, "habit_execution_day", message);
            await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Введите число того, сколько раз за " +
              "день вы хотите выполнять привычку:");
            UserStateTracker.SetUserState(chatId, "awaiting_habit_frequency");
          }
        }
        else if (message.Contains("/endless"))
        {
          UserStateTracker.SetTemporaryData(chatId, "habit_execution_day", "null");
          await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Введите число того, сколько раз за " +
            "день вы хотите выполнять привычку:");
          UserStateTracker.SetUserState(chatId, "awaiting_habit_frequency");
        }
        else
        {
          await HabitTracker.TelegramBotHandler.SendMessageAsync(botClient, chatId, $"Введено некорректное значение. Повторите попытку.");
        }
      }
      else if (state == "awaiting_habit_frequency")
      {
        var habitsModel = new CommonHabitsModel(_dbContextFactory, _args);
        var title = UserStateTracker.GetTemporaryData(chatId, "habit_title");
        var numberOfExecutions = 0;
        if (int.TryParse(message, out numberOfExecutions))
        {
          if (numberOfExecutions > 0)
          {
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
          else
          {
            await HabitTracker.TelegramBotHandler.SendMessageAsync(botClient, chatId, $"Количество повторений привычки за день не может " +
              $"быть отрицательным или равно 0.\nПовторите попытку.");
          }
        }
        else
        {
          await HabitTracker.TelegramBotHandler.SendMessageAsync(botClient, chatId, $"Введено некорректное значение количества повторений привычки." +
            $"\nПовторите попытку.");
        }

      }
    }

    /// <summary>
    /// Отредактировать данные привычки.
    /// </summary>
    /// <param name="botClient">Клиент Telegram-бота.</param>
    /// <param name="habitId">Уникальный идентификатор привычки.</param>
    /// <param name="chatId">Уникальный идентификатор чата пользователя.</param>
    /// <param name="message">Текст полученного сообщения.</param>
    /// <param name="state">Данные о состоянии редактирования данных привычки.</param>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    private async Task EditHabit(ITelegramBotClient botClient, string habitId, long chatId, string message, string state)
    {
      var habitsModel = new CommonHabitsModel(_dbContextFactory, _args);
      var habit = await GetHabitById(chatId, Guid.Parse(habitId));
      if (state.Contains("awaiting_habit_rename_"))
      {
        habitsModel.Update(habit.Id, message, habit.LastExecutionDate, habit.Status, habit.ProgressDays,
          habit.ExpirationDate, habit.NumberOfExecutions, habit.IsPaused);
      }
      else if (state.Contains("awaiting_newex_date_"))
      {
        if (int.TryParse(message, out int value))
        {
          var creationDate = habit.CreationDate;
          var newExpirationDate = creationDate.AddDays(value);
          habit.ExpirationDate = newExpirationDate.ToDateTime(TimeOnly.MinValue);
        }
        else if (message.Contains("/endless"))
        {
          habit.ExpirationDate = null;
        }
        habitsModel.Update(habit.Id, habit.Title, habit.LastExecutionDate, habit.Status, habit.ProgressDays,
            habit.ExpirationDate, habit.NumberOfExecutions, habit.IsPaused);
      }
      else if (state.Contains("awaiting_habit_newfreq_"))
      {
        if (int.TryParse(message, out int value))
        {
          habit.NumberOfExecutions = value;
          habitsModel.Update(habit.Id, habit.Title, habit.LastExecutionDate, habit.Status, habit.ProgressDays,
            habit.ExpirationDate, habit.NumberOfExecutions, habit.IsPaused);
        }
      }
    }

    /// <summary>
    /// Обрабатывает процесс настройки времени рассылки уведомлений пользователя.
    /// </summary>
    /// <param name="botClient"></param>
    /// <param name="chatId"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    public async Task ProcessSetNotificationsTimeAsync(ITelegramBotClient botClient, long chatId, string message)
    {
      var state = UserStateTracker.GetUserState(chatId);
      if (state == "awaiting_new_start_time")
      {
        if (TimeOnly.TryParse(message, out TimeOnly result))
        {
          UserStateTracker.SetTemporaryData(chatId, "notification_start", message);
          await HabitTracker.TelegramBotHandler.SendMessageAsync(botClient, chatId, $"Введите время окончания рассылки уведомлений:");

          UserStateTracker.SetUserState(chatId, "awaiting_notification_end_time");
        }
        else
        {
          await HabitTracker.TelegramBotHandler.SendMessageAsync(botClient, chatId, $"Введен неправильный формат времени.\n" +
            $"Введите время в формате hh:mm");
        }
      }
      else if (state == "awaiting_notification_start_time")
      {
        if (TimeOnly.TryParse(message, out TimeOnly result))
        {
          UserStateTracker.SetTemporaryData(chatId, "notification_start", message);
          await HabitTracker.TelegramBotHandler.SendMessageAsync(botClient, chatId, $"Введите время окончания рассылки уведомлений:");

          UserStateTracker.SetUserState(chatId, "awaiting_notification_end_time");
        }
        else
        {
          await HabitTracker.TelegramBotHandler.SendMessageAsync(botClient, chatId, $"Введен неправильный формат времени.\n" +
            $"Введите время в формате hh:mm");
        }
      }

      else if (state == "awaiting_notification_end_time")
      {
        if (TimeOnly.TryParse(message, out TimeOnly result))
        {
          var notificationsSettings = new CommonNotificationModel(_dbContextFactory, _args);
          var foundNotification = await notificationsSettings.GetByUserChatId(chatId);
          if (foundNotification != null)
          {
            foundNotification.TimeStart = TimeOnly.Parse(UserStateTracker.GetTemporaryData(chatId, "notification_start"));
            if (foundNotification.TimeStart >= result)
            {
              await HabitTracker.TelegramBotHandler.SendMessageAsync(botClient, chatId, $"Время окончания периода не может " +
                $"быть меньше времени начала периода.\nВведите время в формате hh:mm");
            }
            else
            {
              foundNotification.TimeEnd = result;
              notificationsSettings.Update(chatId, foundNotification.TimeStart, foundNotification.TimeEnd);
              UserStateTracker.ClearTemporaryData(chatId);
              UserStateTracker.SetUserState(chatId, null);
              var keyboard = new InlineKeyboardMarkup(new[]
              {
            new[]
            {
              InlineKeyboardButton.WithCallbackData("На главную", "/start")
            }
             });
              await HabitTracker.TelegramBotHandler.SendMessageAsync(botClient, chatId, $"Период отправки уведомлений изменен на " +
                $"{foundNotification.TimeStart} - {foundNotification.TimeEnd}", keyboard);

              return;
            }
          }

          else
          {
            var userModel = new CommonUserModel(_dbContextFactory, _args);
            var foundUser = await userModel.GetByChatId(chatId);
            if (foundUser != null)
            {
              TimeOnly timeStart = TimeOnly.Parse(UserStateTracker.GetTemporaryData(chatId, "notification_start"));
              if (timeStart >= result)
              {
                await HabitTracker.TelegramBotHandler.SendMessageAsync(botClient, chatId, $"Время окончания периода не может " +
                  $"быть меньше времени начала периода.\nВведите время в формате hh:mm");
              }
              else
              {
                TimeOnly timeEnd = result;
                notificationsSettings.Add(chatId, timeStart, timeEnd);
                UserStateTracker.ClearTemporaryData(chatId);
                UserStateTracker.SetUserState(chatId, null);
                var keyboard = new InlineKeyboardMarkup(new[]
                {
                    new[]
                    {
                      InlineKeyboardButton.WithCallbackData("На главную", "/start")
                    }
                  });
                await HabitTracker.TelegramBotHandler.SendMessageAsync(botClient, chatId, $"Настроен период отправки уведомлений: " +
                  $"{timeStart} - {timeEnd}", keyboard);

                return;
              }
            }
          }
        }
        else
        {
          await HabitTracker.TelegramBotHandler.SendMessageAsync(botClient, chatId, $"Введен неправильный формат времени.\n" +
            $"Введите время в формате hh:mm");
        }
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
        await HabitTracker.TelegramBotHandler.SendMessageAsync(botClient, chatId, $"Данные о привычке успешно обновлены!", keyboard);

        return;
      }
    }

    #endregion

    #endregion

    #region Конструкторы

    public User(string name, long chatId, DbContextFactory dbContextFactory, string[] args)
          : base(name, chatId)
    {

      this._dbContextFactory = dbContextFactory;
      this._args = args;
    }

    #endregion
  }
}
