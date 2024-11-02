using Aspose.Cells;
using Aspose.Cells.Charts;
using HabitTracker.Data;
using System.Collections.Generic;
using System.Drawing;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.IO;

namespace HabitTracker.Core
{
  /// <summary>
  /// Функционал для создания отчета по статистике пользователя по привычкам.
  /// </summary>
  public class UserHabitsStatistics
  {
    #region Поля и свойства

    /// <summary>
    /// Контекст базы данных.
    /// </summary>
    private HabitTrackerContext _dbContext;

    #endregion

    #region Методы

    public async Task GetStatistics(List<HabitEntity> habits)
    {
      Workbook wb = new Workbook();
      Worksheet sheet = wb.Worksheets[0];
      Worksheet worksheet = wb.Worksheets[0];
      var i = 1;
      
      foreach(var habit in habits)
      {
        worksheet.Cells[$"A{i}"].PutValue(habit.ProgressDays);
        worksheet.Cells[$"B{i}"].PutValue(habit.Title);
        i++;
      }
      int chartIndex = worksheet.Charts.Add(ChartType.Column, 0, 5, 25, 17);
      Chart chart = worksheet.Charts[chartIndex];
      chart.NSeries.Add($"A1:A{i}", true);
      
      chart.NSeries[0].Name = "Название привычки";
      chart.NSeries[0].XValues = $"B1:B{i}";      
      chart.ValueAxis.Title.Text = "Количество дней прогресса"; 
      chart.PlotArea.Area.ForegroundColor = Color.WhiteSmoke;
      foreach (Series series in chart.NSeries)
      {
        series.DataLabels.IsAutoText = false; 
      }

      chart.Title.Text = $"Статистика по прогрессу ваших привычек на {DateOnly.FromDateTime(DateTime.UtcNow)}";
      chart.Legend.Position = LegendPositionType.Bottom;
      
      string dataDir = Directory.GetCurrentDirectory();
      var str = "Excel_Chart.xlsx";
      var saveDirectory = Path.Combine(dataDir, str);

      wb.Save($"{saveDirectory}", SaveFormat.Xlsx);
    }

    #endregion

    #region Конструкторы
    public UserHabitsStatistics(HabitTrackerContext dbContext)
    {
      _dbContext = dbContext;
    }
    #endregion

  }
}
