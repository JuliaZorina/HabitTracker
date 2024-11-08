using HabitTracker.Core;
using HabitTracker.Data;
using HabitTracker.Models;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace HabitTracker
{
  /// <summary>
  /// Класс для взаимодействия пользователя с Telegram ботом.
  /// </summary>
  public class TelegramBotHandler
  {
    #region Поля и свойства

    /// <summary>
    /// Клиент Telegram бота.
    /// </summary>
    private readonly ITelegramBotClient _botClient;
    
    private HabitTrackerContext _dbContext;
    #endregion

    #region Методы

    /// <summary>
    /// Запускает бота и начинает обработку сообщений.
    /// </summary>
    public async Task StartBotAsync()
    {
      var cts = new CancellationTokenSource();
      var receiverOptions = new ReceiverOptions
      {
        AllowedUpdates = Array.Empty<UpdateType>()
      };

      _botClient.StartReceiving(
          updateHandler: HandleUpdateAsync,
          pollingErrorHandler: HandlePollingErrorAsync,
          receiverOptions: receiverOptions,
          cancellationToken: cts.Token
      );

      var me = await _botClient.GetMeAsync();
      Console.WriteLine($"Start listening for @{me.Username}");

      while (true)
      {
        ChangeHabitStatus.GetData(_dbContext);
        await SendNotification.SendNotificationToUser(_dbContext, _botClient);
        await Task.Delay(60000, cts.Token);
      }
    }

    /// <summary>
    /// Обрабатывает ошибки, возникающие при получении обновлений от Telegram.
    /// </summary>
    /// <param name="client">Клиент Telegram бота.</param>
    /// <param name="exception">Возникшее исключение.</param>
    /// <param name="token">Токен отмены.</param>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    private Task HandlePollingErrorAsync(ITelegramBotClient client, Exception exception, CancellationToken token)
    {
      var ErrorMessage = exception switch
      {
        ApiRequestException apiRequestException
            => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
        _ => exception.ToString()
      };

      Console.WriteLine(ErrorMessage);
      return Task.CompletedTask;
    }

    /// <summary>
    /// Обрабатывает входящие сообщения от Telegram.
    /// </summary>
    /// <param name="client">Клиент Telegram бота.</param>
    /// <param name="update">Объект обновления.</param>
    /// <param name="token">Токен отмены.</param>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    private async Task HandleUpdateAsync(ITelegramBotClient client, Update update, CancellationToken token)
    {
      if (update.Type == UpdateType.Message && update.Message?.Text != null)
      {
        await HandleMessageAsync(update.Message, token);
      }
      else if (update.Type == UpdateType.CallbackQuery)
      {
        await HandleCallbackQueryAsync(update.CallbackQuery, token);
      }
    }

    /// <summary>
    /// Обрабатывает входящие callback-запросы.
    /// </summary>
    /// <param name="callbackQuery">Данные callback-запроса.</param>
    /// <param name="token">Токен отмены.</param>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    private async Task HandleCallbackQueryAsync(CallbackQuery? callbackQuery, CancellationToken token)
    {
      var chatId = callbackQuery.From.Id;
      var userName = callbackQuery.From.Username;
      var userModel = new CommonUserModel(_dbContext);
      var userData = await userModel.GetByChatId(chatId);

      if (userData != null)
      {
        var user = new User(userName, chatId, _dbContext);
        await user.ProcessCallbackAsync(_botClient, chatId, callbackQuery.Data, callbackQuery.Message.MessageId);
      }
      else
      {
        await _botClient.SendTextMessageAsync(chatId, "Ошибка: Данные пользователя не найдены!");
      }      
    }

    /// <summary>
    /// Обрабатывает входящие сообщения.
    /// </summary>
    /// <param name="message">Данные входящего сообщения.</param>
    /// <param name="token">Токен онмены.</param>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    private async Task HandleMessageAsync(Message message, CancellationToken token)
    {
      var chatId = message.Chat.Id;
      var messageText = message.Text;
      var userName = message.From.Username;
      var userModel = new CommonUserModel(_dbContext);
      var userData = await userModel.GetByChatId(chatId);

      if (userData == null)
      {
        userModel.Add(userName, chatId);
        await _botClient.SendTextMessageAsync(chatId, $"{userName}, добро пожаловать!");
        UserStateTracker.SetUserState(chatId, "awaiting_notification_settings");
      }
      var user = new User(userName, chatId, _dbContext);
      await user.ProcessMessageAsync(_botClient, chatId, messageText);
    }

    /// <summary>
    /// Асинхронно отправляет или редактирует сообщение пользователю через Telegram бота.
    /// </summary>
    /// <param name="botClient">Клиент Telegram бота.</param>
    /// <param name="chatId">Идентификатор чата пользователя.</param>
    /// <param name="message">Текст сообщения для отправки или редактирования.</param>
    /// <param name="inlineKeyboardMarkup">Опциональная встроенная клавиатура.</param>
    /// <param name="messageId">Идентификатор сообщения для редактирования (если есть).</param>
    /// <returns>Задача, представляющая асинхронную операцию отправки или редактирования сообщения.</returns>
    public static async Task SendMessageAsync(ITelegramBotClient botClient, long chatId, string message, InlineKeyboardMarkup inlineKeyboardMarkup = null, int? messageId = null)
    {
      if (inlineKeyboardMarkup == null)
      {
        await botClient.SendTextMessageAsync(chatId, message);
      }
      else if (messageId.HasValue)
      {
        await botClient.EditMessageTextAsync(chatId, messageId.Value, message, replyMarkup: inlineKeyboardMarkup);
      }
      else
      {
        await botClient.SendTextMessageAsync(chatId, message, replyMarkup: inlineKeyboardMarkup);
      }
    }

    #endregion

    #region Конструкторы
    public TelegramBotHandler(string botToken, HabitTrackerContext dbContext) 
    { 
      this._botClient = new TelegramBotClient(botToken);
      this._dbContext = dbContext;
    }

    #endregion
  }
}
