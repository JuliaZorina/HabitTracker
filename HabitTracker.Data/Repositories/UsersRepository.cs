using HabitTracker.Core;
using HabitTracker.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace HabitTracker.Data.Repositories
{
  public class UsersRepository
  {
    #region Поля и свойства

    private readonly HabitTrackerContext _dbContext;

    #endregion

    #region Методы

    /// <summary>
    /// Получить список всех пользователей.
    /// </summary>
    /// <returns>Асинхронно возвращает коллекцию всех привычек в базе данных.</returns>
    public async Task<List<UserEntity>> Get()
    {
      return await _dbContext.Users
        .AsNoTracking()
        .ToListAsync();
    }

    /// <summary>
    /// Получить пользователя по его уникальному идентификатору.
    /// </summary>
    /// <returns>Асинхронно возвращает привычку, найденную по ее уникальному идентификатору.</returns>
    public async Task<UserEntity?> GetById(Guid id)
    {
      return await _dbContext.Users
        .AsNoTracking()
        .FirstOrDefaultAsync(u => id == u.Id);
    }


    /// <summary>
    /// Получить пользователя по идентификатору его чата в Telegram.
    /// </summary>
    /// <returns>Асинхронно возвращает привычку, найденную по ее уникальному идентификатору.</returns>
    public async Task<UserEntity?> GetByChatId(long id)
    {
      return await _dbContext.Users
        .AsNoTracking()
        .FirstOrDefaultAsync(u => id == u.ChatId);
    }

    /// <summary>
    /// Добавить нового пользователя в базу данных.
    /// </summary>
    /// <param name="id">Уникальный идентификатор пользоваетля.</param>
    /// <param name="name">Имя пользователя.</param>
    /// <param name="chatId">Уникальный идентификатор чата пользователя в Telegram.</param>
    /// <returns></returns>
    public async Task Add(Guid id, string name, long chatId)
    {      
      var habit = new UserEntity
      {
        Id = id,
        Name = name,
        ChatId = chatId
      };

      await _dbContext.AddAsync(habit);
      await _dbContext.SaveChangesAsync();
    }

    #endregion

    #region Конструкторы

    public UsersRepository(HabitTrackerContext dbContext)
    {
      _dbContext = dbContext;
    }

    #endregion
  }
}
