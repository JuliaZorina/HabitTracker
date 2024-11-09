using HabitTracker.Data.Repositories;
using HabitTracker.Data;
using HabitTracker.Data.Models;

namespace HabitTracker.Core
{
  /// <summary>
  /// Общие методы работы с записями о выполнении привычки.
  /// </summary>
  public class CommonPracticedHabitModel
  {
    #region Поля и свойства

    /// <summary>
    /// Контекст базы данных.
    /// </summary>
    private readonly DbContextFactory _dbContextFactory;
    private readonly string[] _args;


    #endregion

    #region Методы

    /// <summary>
    /// Получить записи о выполнении привычки по уникальному идентификатору привычки и дате выполнения.
    /// </summary>
    /// <param name="id">уникальный идентификатор привычки.</param>
    /// <param name="date">Дата выполения привычки.</param>
    /// <returns></returns>
    public async Task<List<PracticedHabitEntity>?> GetByDateAndHabitId(Guid id, DateOnly date)
    {
      await using (var dbContext = _dbContextFactory.CreateDbContext(this._args))
      {
        var habitsRepository = new HabitsRepository(dbContext);
        var practicedHabitRepository = new PracticedHabitRepository(dbContext);
        HabitEntity? foundHabit = await habitsRepository.GetById(id);
        if (foundHabit != null)
        {
          var practicedHabits = await practicedHabitRepository.GetByDateAndHabitId(foundHabit.Id, date);
          return practicedHabits;
        }
        else
        {
          throw new Exception("Привычка с таким id не найдена в базе данных выполняемых привычек");
        }
      }
    }

    /// <summary>
    /// Добавить новую запись о выполнении привычки в базу данных.
    /// </summary>
    /// <param name="dbContext">Контекст базы данных.</param>
    /// <param name="habitId">Уникальный идентификатор привычки.</param>
    /// <param name="dateTime">Дата и время последнего выполнения привычки.</param>
    /// <exception cref="ArgumentNullException">Выбрасывает исключение, если привычка с указанным Id не найдена в базе данных.</exception>
    public async void Add(Guid habitId, DateTime dateTime)
    {
      await using (var dbContext = _dbContextFactory.CreateDbContext(this._args))
      {
        var habitsRepository = new HabitsRepository(dbContext);
        var practicedHabitsRepository = new PracticedHabitRepository(dbContext);
        HabitEntity? foundHabit = await habitsRepository.GetById(habitId);
        if (foundHabit != null)
        {
          var practicedHabit = new PracticedHabitEntity(Guid.NewGuid(), foundHabit.Id, dateTime);
          await practicedHabitsRepository.Add(practicedHabit);
        }
        else
        {
          throw new ArgumentNullException("Привычка с таким Id не существует в базе данных");
        }
      }
    }

    /// <summary>
    /// Удалить записи о выполнении привычки, найденной по ее уникальному идентификатору, из базы данных.
    /// </summary>
    /// <param name="id">Уникальный идентификатор привычки.</param>
    public async void Delete(Guid id)
    {
      await using (var dbContext = _dbContextFactory.CreateDbContext(this._args))
      {
        var practicedHabitsRepository = new PracticedHabitRepository(dbContext);
        var result = await practicedHabitsRepository.GetById(id);
        if (result != null)
        {
          await practicedHabitsRepository.Delete(id);
        }
      }
    }

    #endregion

    #region Конструкторы
    public CommonPracticedHabitModel(DbContextFactory dbContextFactory, string[] args)
    {
      this._dbContextFactory = dbContextFactory;
      this._args = args;
    }
    #endregion
  }
}
