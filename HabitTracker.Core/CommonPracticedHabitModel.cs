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
    private HabitTrackerContext _dbContext;

    #endregion

    #region Методы

    /// <summary>
    /// Добавить новую запись о выполнении привычки в базу данных.
    /// </summary>
    /// <param name="dbContext">Контекст базы данных.</param>
    /// <param name="habitId">Уникальный идентификатор привычки.</param>
    /// <param name="dateTime">Дата и время последнего выполнения привычки.</param>
    /// <exception cref="ArgumentNullException">Выбрасывает исключение, если привычка с указанным Id не найдена в базе данных.</exception>
    public async void Add( Guid habitId, DateTime dateTime)
    {
      var habitsRepository = new HabitsRepository(_dbContext);
      var practicedHabitsRepository = new PracticedHabitRepository(_dbContext);
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

    /// <summary>
    /// Удалить записи о выполнении привычки, найденной по ее уникальному идентификатору, из базы данных.
    /// </summary>
    /// <param name="id">Уникальный идентификатор привычки.</param>
    public async void Delete(Guid id)
    {
      var practicedHabitsRepository = new PracticedHabitRepository(_dbContext);
      var result = await practicedHabitsRepository.GetById(id);
      if (result != null)
      {
        await practicedHabitsRepository.Delete(id);
      }
    }

    #endregion

    #region Конструкторы
    public CommonPracticedHabitModel(HabitTrackerContext dbContext)
    {
      _dbContext = dbContext;
    }
    #endregion
  }
}
