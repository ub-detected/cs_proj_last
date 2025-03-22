using Spectre.Console;
using System;
using System.Globalization;
using System.Linq;

/// <summary>
/// Класс для отображения Гистограммы и Breakdown Chart
/// </summary>
public static class View
{
    /// <summary>
    /// Метод для отображения гистограммы
    /// </summary>
    /// <param name="data">Данные для отображения</param>
    public static void ShowHistogram(Data data)
    {
        string startDate = AnsiConsole.Ask<string>("Дата начала (dd-MM-yyyy):");
        string endDate = AnsiConsole.Ask<string>("Дата конца (dd-MM-yyyy):");
        DateTime start = DateTime.ParseExact(startDate, "dd-MM-yyyy", CultureInfo.InvariantCulture);
        DateTime end = DateTime.ParseExact(endDate, "dd-MM-yyyy", CultureInfo.InvariantCulture);

        IEnumerable<BarChartItem> sp = data.Sales
            .Where(s => s.Date >= start && s.Date <= end)
            .GroupBy(s => s.Date)
            .Select(g => new BarChartItem(g.Key.ToString("dd-MM-yyyy"), (double)g.Sum(s => s.RubSum)));

        BarChart bc = new BarChart().Width(69);

        foreach (BarChartItem i in sp)
        {
            _ = bc.AddItem(i);
        }
        AnsiConsole.Write(bc);
    }

    /// <summary>
    /// Метод для генерации нужного количества цветов
    /// </summary>
    /// <param name="cnt">Необхожимое количество цветов</param>
    /// <returns>Список цветов (Colors)</returns>
    private static List<Color> GetColors(int cnt)
    {
        List<Color> result = [];
        Random random = new();
        for (int i = 0; i < cnt; ++i)
        {
            int r = random.Next(0, 256);
            int g = random.Next(0, 256);
            int b = random.Next(0, 256);
            result.Add(new Color((byte)r, (byte)g, (byte)b)); 
        }
        return result;
    }

    /// <summary>
    /// Метод для отображение BreakdownChart
    /// </summary>
    /// <param name="data">Данные для отображения</param>
    public static void ShowBreakdownChart(Data data)
    {
        string startDate = AnsiConsole.Ask<string>("Дата начала (dd-MM-yyyy):");
        string endDate = AnsiConsole.Ask<string>("Дата конца (dd-MM-yyyy):");
        DateTime start = DateTime.ParseExact(startDate, "dd-MM-yyyy", CultureInfo.InvariantCulture);
        DateTime end = DateTime.ParseExact(endDate, "dd-MM-yyyy", CultureInfo.InvariantCulture);

        var sp = data.Sales
                .Where(s => s.Date >= start && s.Date <= end)
                .GroupBy(s => s.ProductName)
                .Select(g => new
                {
                    ProductName = g.Key,
                    Total = (double)g.Sum(s => s.RubSum)
                })
                .OrderByDescending(g => g.Total)
                .ToList();


        List<Color> result = GetColors(sp.Count());
        BreakdownChart bc = new BreakdownChart().Width(69);

        int it = 0;
        foreach (var i in sp)
        {
            _ = bc.AddItem(i.ProductName, i.Total, result[it++]);
        }
        AnsiConsole.Write(bc);
    }
}