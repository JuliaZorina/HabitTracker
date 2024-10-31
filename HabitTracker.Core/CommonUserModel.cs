using HabitTracker.Data;
using HabitTracker.Data.Repositories;

namespace HabitTracker.Core
{
  /// <summary>
  /// Статический класс, предоставляющий общие методы работы с моделью пользователя.
  /// </summary>
  public class CommonUserModel
  {
    #region Поля и свойства

    private HabitTrackerContext _dbContext;

    #endregion

    #region Методы

    /// <summary>
    /// Получить пользователя по его уникальному идентификатору.
    /// </summary>
    /// <param name="id">Уникальный идентификатор пользователя.</param>
    /// <returns></returns>
    public async Task<UserEntity?> GetById(Guid id)
    {
      var usersRepository = new UsersRepository(_dbContext);
      return await usersRepository.GetById(id);  
    }

    /// <summary>
    /// Получить пользователя по идентификатору его чата.
    /// </summary>
    /// <param name="chatId">Уникальный идентификатор чата.</param>
    /// <returns></returns>
    public async Task<UserEntity?> GetByChatId(long chatId)
    {
      var usersRepository = new UsersRepository(_dbContext);
      return await usersRepository.GetByChatId(chatId);  
    }

    /// <summary>
    /// Добавить нового пользователя в систему.
    /// </summary>
    /// <param name="name">Имя пользователя.</param>
    /// <param name="chatId">Уникальный идентификатор чата пользователя.</param>
    /// <exception cref="Exception"></exception>
    public async void Add(string name, long chatId)
    {
      var usersRepository = new UsersRepository(_dbContext);
      var getUser = await GetByChatId(chatId);
      if (getUser == null)
      {
        var newUser = new UserEntity(name, chatId);
        await usersRepository.Add(newUser);
      }
      else
      {
        throw new Exception("Пользователь с таким chatId уже существует в системе");
      }
    }

    #endregion

    #region Конструкторы
    public CommonUserModel(HabitTrackerContext dbContext)
    {
      _dbContext = dbContext;
    }
    #endregion
  }
}
