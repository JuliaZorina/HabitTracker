using HabitTracker.Data;
using HabitTracker.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace HabitTracker
{
  public class User : UserEntity, IMessageHandler
  {
    #region Методы
    public Task ProcessCallbackAsync(ITelegramBotClient botClient, long chatId, string callbackData, int messageId)
    {
      throw new NotImplementedException();
    }

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
        InlineKeyboardButton.WithCallbackData("Получить список привычек", "/getAllHabits")
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
    }

    #endregion

    #region Конструкторы

    public User(string name, long chatId)
      : base(name, chatId) { }

    #endregion
  }
}
