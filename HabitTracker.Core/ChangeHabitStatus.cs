﻿using HabitTracker.Data;
using Telegram.Bot;

namespace HabitTracker.Core
{
  public static class ChangeHabitStatus
  {
    /// <summary>
    /// Получить данные о статусах привычек пользователей.
    /// Если наступает новый день, то статус привычки изменяется на "Undone".
    /// </summary>
    /// <param name="dbContextFactory">Фабрика создания контекста базы данных</param>
    /// <param name="args">Аргументы командной строки.</param>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    public static async void GetData(DbContextFactory dbContextFactory, string[] args)
    {
      var habitsModel = new CommonHabitsModel(dbContextFactory, args);
      var foundHabits = await habitsModel.GetAll();
      if (foundHabits != null)
      {
        foreach (var habit in foundHabits)
        {
          var ntpTime = GetTime.GetNetworkTime("time.google.com");
          //var ntpTime = DateTime.Now.AddDays(1);
          if ((TimeOnly.FromDateTime(ntpTime) == TimeOnly.MinValue
            || TimeOnly.FromDateTime(ntpTime).IsBetween(TimeOnly.MinValue, TimeOnly.MinValue.AddMinutes(1.5)))
            || DateOnly.FromDateTime(ntpTime) > habit.LastExecutionDate)
          {
            if (habit.Status == HabitStatus.Done || habit.Status == HabitStatus.InProgress)
            {
              habit.Status = HabitStatus.Undone;
              habitsModel.Update(habit.Id, habit.Title, habit.LastExecutionDate, habit.Status, habit.ProgressDays, habit.ExpirationDate,
                habit.NumberOfExecutions, habit.IsPaused);
            }
          }
          if (habit.ExpirationDate != null)
          {
            if (habit.ExpirationDate == ntpTime || DateOnly.FromDateTime((DateTime)habit.ExpirationDate) < DateOnly.FromDateTime(ntpTime)
              || habit.ExpirationDate < ntpTime)
            {
              habitsModel.Delete(habit.Id);
            }            
          }
        }

      }
    } 
  }
}
