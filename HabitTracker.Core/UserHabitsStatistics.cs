using Aspose.Cells;
using Aspose.Cells.Charts;
using HabitTracker.Data;
using System.Collections.Generic;
using System.Drawing;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.IO;
using NPOI.SS.UserModel.Charts;
using NPOI.SS.Util;
using NPOI.SS.Formula.Functions;

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
      IWorkbook workbook = new XSSFWorkbook();
      ISheet sheet = workbook.CreateSheet("Статистика");
      var i = 0;

      foreach(var habit in habits)
      {
        IRow row = sheet.CreateRow(i);
        ICell cell = row.CreateCell(0);
        cell.SetCellValue(habit.Title);
        cell = row.CreateCell(1);
        cell.SetCellValue(habit.ProgressDays);
        i++;
      }

      sheet.SetColumnWidth(0, 20 * 256);
      sheet.SetColumnWidth(1, 10 * 256);

      IDrawing drawing = sheet.CreateDrawingPatriarch();
      IClientAnchor anchor = drawing.CreateAnchor(0, 0, 0, 0, 5, 0, 15, 20);
      IChart chart = drawing.CreateChart(anchor);
      IChartLegend legend = chart.GetOrCreateLegend();
      legend.Position = LegendPosition.Bottom;
      chart.SetTitle($"Статистика прогресса ваших привычек на {DateOnly.FromDateTime(DateTime.UtcNow)}");

      IColumnChartData<double, double> data = chart.ChartDataFactory.CreateColumnChartData<double, double>();
      //Ось X.
      IChartAxis bottomAxis = chart.ChartAxisFactory.CreateCategoryAxis(AxisPosition.Bottom);
      //Ось Y.
      IValueAxis leftAxis = chart.ChartAxisFactory.CreateValueAxis(AxisPosition.Left);
      leftAxis.Crosses = AxisCrosses.AutoZero;
      IChartDataSource<double> xs = DataSources.FromNumericCellRange(sheet, new CellRangeAddress(0, i-1, 0, 0));
      IChartDataSource<double> ys = DataSources.FromNumericCellRange(sheet, new CellRangeAddress(0, i-1, 1, 1));

      var s1 = data.AddSeries(xs, ys);
      s1.SetTitle($"Количество дней");

      chart.Plot(data, bottomAxis, leftAxis);

      string dataDir = Directory.GetCurrentDirectory();
      var str = "Excel_Chart.xlsx";
      var saveDirectory = Path.Combine(dataDir, str);

      using (var fileData = new FileStream(saveDirectory, FileMode.Create))
      {
        workbook.Write(fileData);
      }
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
