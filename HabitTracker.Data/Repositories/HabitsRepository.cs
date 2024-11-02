using Microsoft.EntityFrameworkCore;

namespace HabitTracker.Data.Repositories
{
  public class HabitsRepository
  {
    #region Поля и свойства

    /// <summary>
    /// Контекст базы данных.
    /// </summary>
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
    /// Получить привычки пользователя по списку фильтров.
    /// </summary>
    /// <returns>Асинхронно возвращает привычки, удовлетворяющие заданным фильтрам.</returns>
    public async Task<List<HabitEntity>?> GetByFilter(Guid userId,string title, DateOnly? createDate, DateOnly? lastExecutionDate,HabitStatus? status)
    {
      var query = _dbContext.Habits.AsNoTracking();

      if (!string.IsNullOrEmpty(title))
      {
        query = query.Where(h => h.Title.Contains(title));
      }
      if (createDate != null)
      {
        query = query.Where(h =>h.CreationDate ==  createDate);
      }
      if (lastExecutionDate != null)
      {
        query = query.Where(h =>h.LastExecutionDate == lastExecutionDate);
      }
      if(status!= null)
      {
        query = query.Where(h => h.Status == status);
      }
      query = query.Where(h => h.UserId == userId);

      return await query.ToListAsync();
    }

    /// <summary>
    /// Добавить новую привычку в базу данных.
    /// </summary>
    /// <param name="habit">Экземпляр класса привычки.</param>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    public async Task Add(HabitEntity habit)
    {
      var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == habit.UserId) 
        ?? throw new ArgumentNullException("Не найден пользователь с указанным Id.");

      var newHabit = new HabitEntity(habit.Id, habit.UserId, habit.Title);

      user.Habbits.Add(newHabit);

      await _dbContext.AddAsync(newHabit);
      await _dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Обновить данные о привычке.
    /// </summary>
    /// <param name="habit">Экземпляр класса привычки.</param>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    public async Task Update(HabitEntity habit)
    {
      await _dbContext.Habits
        .Where(h => h.Id == habit.Id)
        .ExecuteUpdateAsync(s => s
          .SetProperty(h => h.Title, habit.Title)
          .SetProperty(h => h.LastExecutionDate, habit.LastExecutionDate)
          .SetProperty(h => h.Status, habit.Status)
          .SetProperty(h => h.ProgressDays, habit.ProgressDays));
    }
    /// <summary>
    /// Удалить привычку из базы данных.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
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
