using Spectre.Console;
using System.Globalization;

/// <summary>
/// Класс для взаимодейтсвия с пользователем
/// </summary>
/// <remarks>
/// конструктор
/// </remarks>
/// <param name="_data">данные</param>
public class UI(Data _data)
{
    private readonly Data data = _data; // данные
    private readonly TableManager table = new(_data); // табличный менеджер

    /// <summary>
    /// Основной метод, циклично запрашивает у пользователя действия
    /// </summary>
    public void Run()
    {
        while (true)
        {
            string selection = AnsiConsole.Prompt(
                new SelectionPrompt<string>().Title("Выберите действие:").AddChoices("Таблица продаж", "Добавить продажу", "Удалить продажу", "Редактировать продажу",
                                "Гистограмма продаж", "Breakdown Chart", "Сохранить в файл", "Сумма по валютам",
                                "Аналитика по регионам", "ABC-анализ", "XYZ-анализ", "Генерация отчета", "Выход")
            );

            switch (selection)
            {
                case "Таблица продаж": table.ShowTable(); break;
                case "Добавить продажу": AddSale(); break;
                case "Удалить продажу": RemoveSale(); break;
                case "Редактировать продажу": EditSale(); break;
                case "Гистограмма продаж": View.ShowHistogram(data); break;
                case "Breakdown Chart": View.ShowBreakdownChart(data); break;
                case "Сохранить в файл":
                    bool flag = data.SaveToFile(table);
                    if (!flag)
                    {
                        Console.WriteLine("Сохранить не удалось.");
                    }
                    break;
                case "Сумма по валютам": ShowTotalByCurrency(); break;
                case "Выход":
                    flag = data.SaveToFile(table);
                    if (flag)
                    {
                        return;
                    }
                    break;
            }
        }
    }

    /// <summary>
    /// Добавление продажи, вызывает AddSale из класса Data (data)
    /// </summary>
    private async void AddSale()
    {
        while (true)
        {
            AnsiConsole.MarkupLine("Введите данные о продаже в формате (в одну строку):");
            AnsiConsole.MarkupLine("[grey]Дата продажи (dd-MM-yyy),ID товара,Наименование товара,Кол-во товара,Цена единицы,Регион продажи,В какой валюте была продажа (RUB, USD, ...)[/]");
            AnsiConsole.MarkupLine("Пример строки: [grey]01-05-2022,101,Товар А,12,123,Россия,RUB[/]");
            AnsiConsole.MarkupLine("Для выхода из режима добавления продажи введите пробел.");

            string? s = Console.ReadLine();
            if (s == " ")
            {
                return;
            }
            if (s == null)
            {
                Console.WriteLine("Введите непустую строку.");
                continue;
            }

            string[] fields = s.Split(',');
            if (await data.AddSale(fields))
            {
                break;
            }
            Console.WriteLine("Введите корректные данные.");
        }
    }
    /// <summary>
    /// Метод для удаления продажи, проверяет корректность введенных данных
    /// Вызывает RemoveSale из класса Data (data)
    /// </summary>
    private void RemoveSale()
    {
        while (true)
        {
            Console.WriteLine("Введите ID продажи для удаления (для выхода из режима удаления введите пробел):");
            string? s = Console.ReadLine();
            if (s == " ")
            {
                return;
            }
            if (!int.TryParse(s, out int id) || id <= 0 || id > Sale.ID_cnt)
            {
                Console.WriteLine("Продажи с таким ID нет.");
                continue;
            }
            data.RemoveSale(id);
            break;
        }
    }

    /// <summary>
    /// Метод для изменения продажи
    /// </summary>
    private void EditSale()
    {
        int id;
        string? s;
        while (true)
        {
            Console.WriteLine("Введите ID продажи для изменения (для выхода из режима изменения введите пробел):");
            s = Console.ReadLine();
            if (s == " ")
            {
                return;
            }
            if (!int.TryParse(s, out id) || id <= 0 || id > Sale.ID_cnt)
            {
                Console.WriteLine("Продажи с таким ID нет.");
                continue;
            }
            break;
        }

        string field = AnsiConsole.Prompt(new SelectionPrompt<string>().Title("Выберите поле для сортировки:").AddChoices(
            "Регион", "Валюта", "Дата", "ID товара", "Наименование", "Количество", "Цена"));
        
        if (field == "Регион")
        {
            while (true)
            {
                Console.WriteLine("Введите регион (для выхода из режима изменения введите пробел):");
                s = Console.ReadLine();
                if (s == " ")
                {
                    return;
                }

                if (s == null || s.Length == 0)
                {
                    Console.WriteLine("Было введено пустое поле. Попробуйте снова.");
                }
                else
                {
                    data.Sales[id - 1].RegionChange(s);
                    break;
                }
            }
        }
        else if (field == "Валюта") // проверка на валюты
        {
            while (true)
            {
                Console.WriteLine("Введите валюту (для выхода из режима изменения введите пробел):");
                s = Console.ReadLine();
                if (s == " ")
                {
                    return;
                }

                if (s == null || !CurrencyManager.Currencies.Contains(s))
                {
                    Console.WriteLine("Такой валюты не существует. Попробуйте снова.");
                }
                else
                {
                    data.Sales[id - 1].CurrencyChange(s);
                    break;
                }
            }
        }
        else if (field == "Дата")
        {
            while (true)
            {
                Console.WriteLine("Введите дату в формате dd-MM-yyyy (для выхода из режима изменения введите пробел):");
                s = Console.ReadLine();
                if (s == " ")
                {
                    return;
                }

                if (s == null || s.Length == 0 || !DateTime.TryParseExact(s, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out DateTime date))
                {
                    Console.WriteLine("Некорректный формат даты.");
                }
                else
                {
                    data.Sales[id - 1].DateChange(date);
                    break;
                }
            }
        }
        else if (field == "ID товара")
        {
            while (true)
            {
                Console.WriteLine("Введите ID товара (для выхода из режима изменения введите пробел):");
                s = Console.ReadLine();
                if (s == " ")
                {
                    return;
                }

                if (s == null || s.Length == 0)
                {
                    Console.WriteLine("Введите непустую строку.");
                }
                else
                {
                    data.Sales[id - 1].ProductIDChange(s);
                    break;
                }
            }
        }
        else if (field == "Наименование")
        {
            while (true)
            {
                Console.WriteLine("Введите наименование товара (для выхода из режима изменения введите пробел):");
                s = Console.ReadLine();
                if (s == " ")
                {
                    return;
                }

                if (s == null || s.Length == 0)
                {
                    Console.WriteLine("Введите непустую строку.");
                }
                else
                {
                    data.Sales[id - 1].ProductNameChange(s);
                    break;
                }
            }
        }
        else if (field == "Количество")
        {
            while (true)
            {
                Console.WriteLine("Введите количество товара (для выхода из режима изменения введите пробел):");
                s = Console.ReadLine();
                if (s == " ")
                {
                    return;
                }

                if (s == null || !int.TryParse(s, out int new_cnt) || new_cnt < 0)
                {
                    Console.WriteLine("Ошибка. Проверьте, что была введена непустая строка, являющаяся неотрицательным числом.");
                }
                else
                {
                    data.Sales[id - 1].QuantityChange(new_cnt);
                    break;
                }
            }
        }
        else if (field == "Цена")
        {
            while (true)
            {
                Console.WriteLine("Введите цену товара (для выхода из режима изменения введите пробел):");
                s = Console.ReadLine();
                if (s == " ")
                {
                    return;
                }

                if (s == null || !int.TryParse(s, out int price) || price < 0)
                {
                    Console.WriteLine("Ошибка. Проверьте, что была введена непустая строка, являющаяся неотрицательным числом.");
                }
                else
                {
                    data.Sales[id - 1].PriceChange(price);
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Вывод таблиц по валютам с столбцами "Валюта", "Сумма (исходная)", "Сумма (RUB)"
    /// </summary>
    private void ShowTotalByCurrency()
    {
        var totals = data.Sales
            .GroupBy(s => s.Currency)
            .Select(g => new { Currency = g.Key, TotalOriginal = g.Sum(s => s.Sum), TotalRub = g.Sum(s => s.RubSum) });

        foreach (var t in totals)
        {
            Table table = new Table().AddColumns("Валюта", "Сумма (исходная)", "Сумма (RUB)").AddRow(t.Currency, t.TotalOriginal.ToString("F2"), t.TotalRub.ToString("F2"));
            AnsiConsole.Write(table);
        }
    }
}