using HabitTracker.Data;
using HabitTracker.Data.Repositories;

namespace HabitTracker.Core
{
  /// <summary>
  /// Статический класс, предоставляющий общие методы работы с моделью привычки.
  /// </summary>
  public class CommonHabitsModel
  {
    #region Поля и свойства

    private HabitTrackerContext _dbContext;

    #endregion

    #region Методы

    /// <summary>
    /// Получить список привычек с выбранным статусом.
    /// </summary>
    /// <param name="status">Статус задачи.</param>
    /// <param name="chatId">Уникальный идентификатор чата пользователя.</param>
    /// <returns></returns>
    public async Task<List<HabitEntity>?> GetByStatus(long chatId,HabitStatus status)
    {
      var habitsRepository = new HabitsRepository(_dbContext);
      var usersRepository = new UsersRepository(_dbContext);
      UserEntity? foundUser = await usersRepository.GetByChatId(chatId);
      if (foundUser != null)
      {
        return await habitsRepository.GetByFilter(foundUser.Id, string.Empty, null, null, status);
      }
      else
      {
        throw new Exception("Пользователь с таким chatId не найден в базе данных");
      }
    }

    /// <summary>
    /// Получить список всех привычек пользователя.
    /// </summary>
    /// <param name="status">Статус задачи.</param>
    /// <param name="chatId">Уникальный идентификатор чата пользователя.</param>
    /// <returns></returns>
    public async Task<List<HabitEntity>?> GetAll(long chatId)
    {
      var habitsRepository = new HabitsRepository(_dbContext);
      var usersRepository = new UsersRepository(_dbContext);
      UserEntity? foundUser = await usersRepository.GetByChatId(chatId);
      if (foundUser != null)
      {
        return await habitsRepository.GetByFilter(foundUser.Id, string.Empty, null, null, null);
      }
      else
      {
        throw new Exception("Пользователь с таким chatId не найден в базе данных");
      }
    }

    /// <summary>
    /// Добавить новую привычку в базу данных.
    /// </summary>
    /// <param name="chatId">Уникальный идентификатор чата пользователя.</param>
    /// <param name="title">Название привычки.</param>
    public async void Add(HabitTrackerContext dbContext,long chatId, string title)
    {
      var habitsRepository = new HabitsRepository(dbContext);
      var usersRepository = new UsersRepository(dbContext);
      UserEntity? foundUser = await usersRepository.GetByChatId(chatId);
      if (foundUser != null)
      {
        var result = await habitsRepository.GetByFilter(foundUser.Id, title.ToLower(), null, null, null);
        if (result.Count == 0)
        {
          var habit = new HabitEntity(Guid.NewGuid(), foundUser.Id, title);
          await habitsRepository.Add(habit);
        }
        else if (result.Count > 0)
        {
          //предложить продолжить уже существующую привычку
        }
      }
      else
      {
        throw new Exception("Пользователь с таким chatId не существует в базе данных");
      }
    }

    /// <summary>
    /// Обновить данные о привычке.
    /// </summary>
    /// <param name="id">Уникальный идентификатор привычки.</param>
    /// <param name="name">Название привычки.</param>
    /// <param name="lastDay">Дата последнего выполнения привычки.</param>
    /// <param name="status">Статус привычки.</param>
    /// <param name="progressDays">Количество дней прогресса привычки.</param>
    public async void Update(Guid id, string name, DateOnly lastDay, HabitStatus status, long progressDays)
    {
      var habitsRepository = new HabitsRepository(_dbContext);
      var result = await habitsRepository.GetById(id);
      if (result != null)
      {
        var habit = new HabitEntity(id, name, lastDay, status, progressDays);
        await habitsRepository.Update(habit);
      }      
    }

    /// <summary>
    /// Удалить привычку из базы данных.
    /// </summary>
    /// <param name="id">Уникальный идентификатор привычки.</param>
    public async void Delete(Guid id)
    {
      var habitsRepository = new HabitsRepository(_dbContext);
      var result = await habitsRepository.GetById(id);
      if (result != null)
      {
        await habitsRepository.Delete(id);
      }
    }

    #endregion

    #region Конструкторы
    public CommonHabitsModel(HabitTrackerContext dbContext)
    {
      _dbContext = dbContext;
    }
    #endregion
  }
}
