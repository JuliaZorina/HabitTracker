using HabitTracker.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace HabitTracker.Data.Repositories
{
  public class PracticedHabitRepository
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
    public async Task<List<PracticedHabitEntity>> Get()
    {
      return await _dbContext.PracticedHabits
        .AsNoTracking()
        .ToListAsync();
    }

    /// <summary>
    /// Получить привычку по ее уникальному идентификатору.
    /// </summary>
    /// <returns>Асинхронно возвращает список данных о выполнении привычки, найденных по ее уникальному идентификатору.</returns>
    public async Task<List<PracticedHabitEntity>?> GetById(Guid id)
    {
      return await _dbContext.PracticedHabits
        .AsNoTracking().Where(ph => ph.HabitId == id)
        .ToListAsync();
    }

    /// <summary>
    /// Добавить новую привычку в базу данных.
    /// </summary>
    /// <param name="habit">Экземпляр класса привычки.</param>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    public async Task Add(PracticedHabitEntity habit)
    {
      var user = await _dbContext.Habits.FirstOrDefaultAsync(h => h.Id == habit.HabitId)
        ?? throw new ArgumentNullException("Не найдена привычка с указанным Id.");

      var newPracticedHabit = new PracticedHabitEntity(habit.Id, habit.HabitId, habit.LastExecutionDate);

      await _dbContext.AddAsync(newPracticedHabit);
      await _dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Удалить привычку из базы данных.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task Delete(Guid id)
    {
      await _dbContext.PracticedHabits
        .Where(h => h.HabitId == id)
        .ExecuteDeleteAsync();
    }

    #endregion

    #region Конструкторы

    public PracticedHabitRepository(HabitTrackerContext dbContext)
    {
      _dbContext = dbContext;
    }

    #endregion
  }
}
