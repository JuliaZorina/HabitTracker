using HabitTracker.Data;
using Aspose.Cells.Charts;
using Aspose.Cells.Rendering;
using Aspose.Cells;
using Telegram.Bot;
using System.Text;
using HabitTracker.Data.Data;
using YamlDotNet.Core.Tokens;

namespace HabitTracker.Core
{
  /// <summary>
  /// Функционал для создания отчета по статистике пользователя по привычкам.
  /// </summary>
  public class UserHabitsStatistics
  {
    #region Методы
    
    public static async Task SendStatistics(ITelegramBotClient botClient, long chatId, List<HabitEntity> habits)
    {
      var fileName = $"ExcelChartToImage_{chatId}.jpg";
      PlotGraph(habits, fileName);
      
      var filePath = Path.Combine(Directory.GetCurrentDirectory(), fileName);
      var token = BotConfigManager.ConfigApp.BotToken;

      if (!string.IsNullOrWhiteSpace(token) || System.IO.File.Exists(filePath))
      {
        using (var form = new MultipartFormDataContent())
        {
          form.Add(new StringContent(chatId.ToString(), Encoding.UTF8), "chat_id");

          using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
          {
            form.Add(new StreamContent(fileStream), "photo", filePath.Split('\\').Last());

            using (var client = new HttpClient())
            {
              await client.PostAsync($"https://api.telegram.org/bot{token}/sendPhoto", form);

              Console.WriteLine($"Statistics send successfully to{chatId}");
            }
          }
        }
      }
      System.IO.File.Delete(filePath);
    }

    /// <summary>
    /// Построить график по количеству дней прогресса привычки.
    /// </summary>
    /// <param name="habits">Коллекция привычек пользователя.</param>
    /// <returns></returns>
    private static void PlotGraph(List<HabitEntity> habits, string fileName)
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
      chart.PlotArea.Area.ForegroundColor = System.Drawing.Color.WhiteSmoke;
      foreach (Series series in chart.NSeries)
      {
        series.DataLabels.IsAutoText = false;
      }
      chart.Title.Text = $"Статистика по прогрессу ваших привычек на {DateOnly.FromDateTime(GetTime.GetNetworkTime("time.google.com"))}";
      chart.Legend.Position = LegendPositionType.Bottom;

      ImageOrPrintOptions imageOrPrintOptions = new ImageOrPrintOptions();
      imageOrPrintOptions.ImageType = Aspose.Cells.Drawing.ImageType.Jpeg;

      chart.ToImage(fileName, imageOrPrintOptions);
    }
    #endregion

  }
}
