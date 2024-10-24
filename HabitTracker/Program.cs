using HabitTracker.Core;
using HabitTracker.Data.Context;

namespace HabitTracker
{
  internal class Program
  {
    static void Main(string[] args)
    {
      using (HabitTrackerContext db = new HabitTrackerContext())
      {
        var users = db.Users.ToList();
        Console.WriteLine("Список объектов:");
        foreach (User u in users)
        {
          Console.WriteLine($"{u.Id}.{u.Name}");
        }
      }
    }
  }
}
