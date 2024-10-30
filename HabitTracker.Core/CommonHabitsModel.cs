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
    /// <param name="userId">Уникальный идентификатор пользователя.</param>
    /// <returns></returns>
    public async Task<List<HabitEntity>?> GetByStatus(Guid userId,HabitStatus status)
    {
      var habitsRepository = new HabitsRepository(_dbContext);
      return await habitsRepository.GetByFilter(userId, string.Empty, null, null, status);
    }

    public async void Add(Guid userID, string title)
    {
      var habitsRepository = new HabitsRepository(_dbContext);
      var result = await habitsRepository.GetByFilter(userID, title.ToLower(), null, null, null);
      if(result==null)
      {
        var habit = new HabitEntity(Guid.NewGuid(), userID, title);
        await habitsRepository.Add(habit);
      }
      else if(result!=null && result.Count>0)
      {
        //предложить продолжить уже существующую привычку
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
    CommonHabitsModel(HabitTrackerContext dbContext)
    {
      _dbContext = dbContext;
    }
    #endregion
  }
}
