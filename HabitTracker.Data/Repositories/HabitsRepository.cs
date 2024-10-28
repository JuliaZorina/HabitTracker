using HabitTracker.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace HabitTracker.Data.Repositories
{
  public class HabitsRepository
  {
    #region Поля и свойства

    private readonly HabitTrackerContext _dbContext;

    #endregion

    #region Методы

    /// <summary>
    /// Получить список всех привычек.
    /// </summary>
    /// <returns>Асинхронно возвращает коллекцию всех привычек в базе данных.</returns>
    public async Task<List<HabitEntity>> Get()
    {
      return await _dbContext.Habits
        .AsNoTracking()
        .ToListAsync();
    }
    
    /// <summary>
    /// Получить привычку по ее уникальному идентификатору.
    /// </summary>
    /// <returns>Асинхронно возвращает привычку, найденную по ее уникальному идентификатору.</returns>
    public async Task<HabitEntity?> GetById(Guid id)
    {
      return await _dbContext.Habits
        .AsNoTracking()
        .FirstOrDefaultAsync(h => id == h.Id);
    }

    /// <summary>
    /// Получить привычки по списку фильтров.
    /// </summary>
    /// <returns>Асинхронно возвращает привычки, удовлетворяющие заданным фильтрам.</returns>
    public async Task<List<HabitEntity>?> GetByFilter(string title, DateOnly? createDate, DateOnly? lastExecutionDate)
    {
      var query = _dbContext.Habits.AsNoTracking();

      if (!string.IsNullOrEmpty(title))
      {
        query = query.Where(h => h.Name.Contains(title));
      }
      if (createDate != null)
      {
        query = query.Where(h =>h.CreationDate ==  createDate);
      }
      if (lastExecutionDate != null)
      {
        query = query.Where(h =>h.LastExecutionDate == lastExecutionDate);
      }

      return await query.ToListAsync();
    }

    /// <summary>
    /// Добавить новую привычку в базу данных.
    /// </summary>
    /// <param name="id">Уникальный идентификатор привычки.</param>
    /// <param name="userId">Уникальный идентификатор пользователя-владельца привычки.</param>
    /// <param name="name">Название привычки.</param>
    /// <param name="creationDay">Дата создания привычки.</param>
    /// <param name="lastDay">Дата последнего выполнения привычки.</param>
    /// <param name="status">Статус привычки.</param>
    /// <param name="progressDays">Количество дней прогресса привычки.</param>
    /// <returns></returns>
    public async Task Add(Guid id, Guid userId, string name, DateOnly creationDay, DateOnly lastDay, HabitStatus status, long progressDays)
    {
      var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == id) 
        ?? throw new ArgumentNullException("Не найден пользователь с указанным Id.");

      var habit = new HabitEntity
      {
        Id = id,
        UserId = userId,
        Name = name,
        Status = status,
        ProgressDays = progressDays,
        CreationDate = creationDay,
        LastExecutionDate = lastDay
      };

      user.Habbits.Add(habit);

      await _dbContext.AddAsync(habit);
      await _dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Обновить данные о привычке.
    /// </summary>
    /// <param name="id">Уникальный идентификатор привычки.</param>
    /// <param name="name">Название привычки.</param>
    /// <param name="lastDay">Дата последнего выполнения привычки.</param>
    /// <param name="status">Статус привычки.</param>
    /// <param name="progressDays">Количество дней прогресса привычки.</param>
    /// <returns></returns>
    public async Task Update(Guid id, string name, DateOnly lastDay, HabitStatus status, long progressDays)
    {
      await _dbContext.Habits
        .Where(h => h.Id == id)
        .ExecuteUpdateAsync(s => s
          .SetProperty(h => h.Name, name)
          .SetProperty(h => h.LastExecutionDate, lastDay)
          .SetProperty(h => h.Status, status)
          .SetProperty(h => h.ProgressDays, progressDays));
    }
    
    public async Task Delete(Guid id)
    {
      await _dbContext.Habits
        .Where(h => h.Id == id)
        .ExecuteDeleteAsync();
    }

    #endregion

    #region Конструкторы

    public HabitsRepository(HabitTrackerContext dbContext) 
    {
      _dbContext = dbContext;
    }

    #endregion
  }
}
