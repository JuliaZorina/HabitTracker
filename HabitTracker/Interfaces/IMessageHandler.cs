using Telegram.Bot;

namespace HabitTracker.Interfaces
{
  public interface IMessageHandler
  {
    /// <summary>
    /// Обрабатывает входящее сообщение от пользователя.
    /// </summary>
    /// <param name="botClient">Клиент Telegram бота.</param>
    /// <param name="chatId">Уникальный идентификатор чата пользователя.</param>
    /// <param name="message">Текст сообщения от пользователя.</param>
    /// <returns></returns>
    Task ProcessMessageAsync(ITelegramBotClient botClient, long chatId, string message);

    /// <summary>
    /// Обрабаотывает callback-запросы от пользователя.
    /// </summary>
    /// <param name="botClient">Клиент telegram-бота.</param>
    /// <param name="chatId">Уникальный идентификатор чата пользователя.</param>
    /// <param name="callbackData">Текст callback-запроса.</param>
    /// <param name="messageId">Уникальный идентификатор сообщения.</param>
    /// <returns></returns>
    Task ProcessCallbackAsync(ITelegramBotClient botClient, long chatId, string callbackData, int messageId);
  }
}