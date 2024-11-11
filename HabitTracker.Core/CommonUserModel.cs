using HabitTracker.Data;
using HabitTracker.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

namespace HabitTracker.Core
{
  /// <summary>
  /// Статический класс, предоставляющий общие методы работы с моделью пользователя.
  /// </summary>
  public class CommonUserModel
  {
    #region Поля и свойства

    /// <summary>
    /// Фабрика создания контекста базы данных.
    /// </summary>
    private readonly DbContextFactory _dbContextFactory;
    /// <summary>
    /// Аргументы командной строки.
    /// </summary>
    private readonly string[] _args;

    #endregion

    #region Методы

    /// <summary>
    /// Получить пользователя по его уникальному идентификатору.
    /// </summary>
    /// <param name="id">Уникальный идентификатор пользователя.</param>
    /// <returns></returns>
    public async Task<UserEntity?> GetById(Guid id)
    {
      await using (var dbContext = _dbContextFactory.CreateDbContext(this._args))
      {
        var usersRepository = new UsersRepository(dbContext);
        return await usersRepository.GetById(id);
      }
    }

    /// <summary>
    /// Получить пользователя по идентификатору его чата.
    /// </summary>
    /// <param name="chatId">Уникальный идентификатор чата.</param>
    /// <returns></returns>
    public async Task<UserEntity?> GetByChatId(long chatId)
    {
      await using(var dbContext = _dbContextFactory.CreateDbContext(this._args))
      {
        var usersRepository = new UsersRepository(dbContext);
        return await usersRepository.GetByChatId(chatId);
      }       
    }

    /// <summary>
    /// Добавить нового пользователя в систему.
    /// </summary>
    /// <param name="name">Имя пользователя.</param>
    /// <param name="chatId">Уникальный идентификатор чата пользователя.</param>
    /// <exception cref="Exception"></exception>
    public async void Add(string name, long chatId)
    {
      await using (var dbContext = _dbContextFactory.CreateDbContext(this._args))
      {
        var usersRepository = new UsersRepository(dbContext);
        var getUser = await GetByChatId(chatId);
        if (getUser == null)
        {
          var newUser = new UserEntity(Guid.NewGuid(), name, chatId);
          await usersRepository.Add(newUser);
        }
        else
        {
          throw new Exception("Пользователь с таким chatId уже существует в системе");
        }
      }
    }

    #endregion

    #region Конструкторы
    public CommonUserModel(DbContextFactory dbContextFactory, string[] args)
    {
      this._dbContextFactory = dbContextFactory;
      this._args = args;
    }
    #endregion
  }
}
