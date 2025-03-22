using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

/// <summary>
/// Класс для взаимодействия с cbr.ru
/// </summary>
public static class CurrencyManager
{
    private static readonly HttpClient http_client = new(); // http клиент
    private static readonly Dictionary<(string Currency, DateTime Date), decimal> mp = []; // Словарь для хранения курса валют в зависимости от даты
    public static List<string?> Currencies { get; private set; } = []; // Список всех валют с cbr.ru
    /// <summary>
    /// Вызывается однажды, при запуске программы (Program.cs)
    /// Получает список валют (Currencies)
    /// </summary>
    /// <returns></returns>
    public static async Task PrepareAsync()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        string url = $"http://www.cbr.ru/scripts/XML_daily.asp?date_req={DateTime.Now:dd/MM/yyyy}";

        byte[] responseBytes = await http_client.GetByteArrayAsync(url);

        string response = Encoding.GetEncoding("windows-1251").GetString(responseBytes);
        XDocument xml = XDocument.Parse(response);
        Currencies = xml.Descendants("Valute").Select(v => v.Element("CharCode")?.Value)
            .Where(code => !string.IsNullOrEmpty(code)).ToList();
            
        if (!Currencies.Contains("RUB"))
        {
            Currencies.Add("RUB");
        }
    }

    /// <summary>
    /// Получение курса валюты к рублю по дате
    /// </summary>
    /// <param name="currency">валюта</param>
    /// <param name="date">дата</param>
    /// <returns>курс валюты к рублю (число)</returns>
    public async static Task<decimal> GetExchangeRateAsync(string currency, DateTime date)
    {
        if (currency == "RUB")
        {
            return 1m;
        }
        (string currency, DateTime Date) key = (currency, date.Date);

        if (mp.TryGetValue(key, out decimal rate))
        {
            return rate;
        }
        string url = $"http://www.cbr.ru/scripts/XML_daily.asp?date_req={date:dd/MM/yyyy}";

        byte[] responseBytes = await http_client.GetByteArrayAsync(url);

        string response = Encoding.GetEncoding("windows-1251").GetString(responseBytes);
        
        XDocument xml = XDocument.Parse(response);
        string? value = null;
        while (value == null)
        {
            value = xml.Descendants("Valute")?
            .First(v => v.Element("CharCode")?.Value == currency)?
            .Element("Value")?.Value;
        }
        rate = decimal.Parse(value.Replace(',', '.'), CultureInfo.InvariantCulture);
        mp[key] = rate;
        return rate;
    }
}
