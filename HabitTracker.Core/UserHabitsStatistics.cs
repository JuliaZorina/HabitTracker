using HabitTracker.Data;
using Aspose.Cells.Charts;
using Aspose.Cells.Rendering;
using Aspose.Cells;
using System.Drawing;

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

    /// <summary>
    /// Построить график по количеству дней прогресса привычки.
    /// </summary>
    /// <param name="habits">Коллекция привычек пользователя.</param>
    /// <returns></returns>
    public async Task PlotGraph(List<HabitEntity> habits)
    {

      Workbook wb = new Workbook();
      Worksheet sheet = wb.Worksheets[0];
      Worksheet worksheet = wb.Worksheets[0];
      var i = 1;

      foreach (var habit in habits)
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

      ImageOrPrintOptions imageOrPrintOptions = new ImageOrPrintOptions();
      imageOrPrintOptions.ImageType = Aspose.Cells.Drawing.ImageType.Jpeg;

      // Save chart as JPEG image
      chart.ToImage("ExcelChartToImage.jpg", imageOrPrintOptions);
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
