using HabitTracker.Data;
using HabitTracker.Interfaces;
using Telegram.Bot;

namespace HabitTracker
{
  public class User : UserEntity, IMessageHandler
  {
    #region Методы
    public Task ProcessCallbackAsync(ITelegramBotClient botClient, long chatId, string callbackData, int messageId)
    {
      throw new NotImplementedException();
    }

    public Task ProcessMessageAsync(ITelegramBotClient botClient, long chatId, string message)
    {
      throw new NotImplementedException();
    }

    #endregion

    #region Конструкторы

    public User(string name, long chatId)
      : base(name, chatId) { }

    #endregion
  }
}
