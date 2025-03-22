/*
 * ФИО: Низамутдинов Азат Артурович
 * Группа: 245-2
 * Вариант: 3
 */
using Spectre.Console;

/// <summary>
/// Класс для вывода и работы с таблицей
/// </summary>
/// <remarks>
/// Конструктор
/// </remarks>
/// <param name="_data">данные</param>
public class TableManager(Data _data)
{
    private readonly Data data = _data; // данные
    private readonly Dictionary<string, string> filters = []; // фильтры текущие
    private string? sort_field; // поле для сортировки
    private bool sort_less = true; // способ сортировки

    /// <summary>
    /// Вывод таблицы на экран, с возможностью взаимодействия
    /// </summary>
    public void ShowTable()
    {
        while (true)
        {
            Table table = new Table().AddColumns("ID", "Дата", "ID товара", "Наименование", "Количество", "Цена за ед.", "Регион", "Валюта", "Сумма", "Сумма в руб.");
            IEnumerable<Sale> filteredSales = ApplyFiltersAndSorting(data.Sales);

            foreach (Sale sale in filteredSales)
            {
                _ = table.AddRow(sale.Id.ToString(), sale.Date.ToString("dd-MM-yyyy"), sale.ProductId.ToString(), sale.ProductName,
                             sale.Quantity.ToString(), sale.Price.ToString("F3"), sale.Region, sale.Currency,
                             sale.Sum.ToString("F3"), sale.RubSum.ToString("F3"));
            }
            AnsiConsole.Write(table);

            string action = AnsiConsole.Prompt(new SelectionPrompt<string>().Title("Действие с таблицей:")
                .AddChoices("Фильтровать", "Сортировать", "Сбросить сортировку", "Отменить фильтр", "Назад"));

            if (action == "Назад")
            {
                break;
            }
            if (action == "Фильтровать")
            {
                AddFilter();
            }
            if (action == "Сортировать")
            {
                SetSort();
            }
            if (action == "Сбросить сортировку")
            {
                sort_field = null;
                sort_less = true;
            }
            if (action == "Отменить фильтр")
            {
                RemoveFilter();
            }
        }
    }
    /// <summary>
    /// Применение фильтров и сортировки
    /// </summary>
    /// <param name="sales">данные с продажами</param>
    /// <returns>данные посел применения фильтров и сортировки</returns>
    public IEnumerable<Sale> ApplyFiltersAndSorting(IReadOnlyList<Sale> sales)
    {
        IEnumerable<Sale> result = sales.AsEnumerable().Where(s => s.Del == false);

        if (filters.ContainsKey("Регион"))
        {
            result = result.Where(s => s.Region == filters["Регион"]);
        }
        if (filters.Keys.Contains("Валюта"))
        {
            result = result.Where(s => s.Currency == filters["Валюта"]);
        }
        if (filters.Keys.Contains("ID товара"))
        {
            result = result.Where(s => s.ProductId == filters["ID товара"]);
        }
        if (filters.Keys.Contains("Наименование"))
        {
            result = result.Where(s => s.ProductName == filters["Наименование товара"]);
        }
        if (filters.Keys.Contains("Количество"))
        {
            result = result.Where(s => s.Quantity == int.Parse(filters["Количество"]));
        }
        if (filters.Keys.Contains("Цена"))
        {
            result = result.Where(s => s.Price == int.Parse(filters["Цена"]));
        }
        if (filters.Keys.Contains("Сумма"))
        {
            result = result.Where(s => s.Sum == int.Parse(filters["Сумма"]));
        }
        if (filters.Keys.Contains("Сумма в руб."))
        {
            result = result.Where(s => s.RubSum == int.Parse(filters["Сумма в руб."]));
        }

        if (sort_field == "Регион")
        {
            result = sort_less ? result.OrderBy(s => s.Region) : result.OrderByDescending(s => s.Region);
        }
        else if (sort_field == "Валюта")
        {
            result = sort_less ? result.OrderBy(s => s.Currency) : result.OrderByDescending(s => s.Currency);
        }
        else if (sort_field == "ID товара")
        {
            result = sort_less ? result.OrderBy(s => s.ProductId) : result.OrderByDescending(s => s.ProductId);
        }
        else if (sort_field == "Наименование")
        {
            result = sort_less ? result.OrderBy(s => s.ProductName) : result.OrderByDescending(s => s.ProductName);
        }
        else if (sort_field == "Количество")
        {
            result = sort_less ? result.OrderBy(s => s.Quantity) : result.OrderByDescending(s => s.Quantity);
        }
        else if (sort_field == "Цена")
        {
            result = sort_less ? result.OrderBy(s => s.Price) : result.OrderByDescending(s => s.Price);
        }
        else if (sort_field == "Сумма")
        {
            result = sort_less ? result.OrderBy(s => s.Sum) : result.OrderByDescending(s => s.Sum);
        }
        else if (sort_field == "Сумма в руб.")
        {
            result = sort_less ? result.OrderBy(s => s.RubSum) : result.OrderByDescending(s => s.RubSum);
        }
        return result;
    }

    /// <summary>
    /// Добавление фильтра
    /// </summary>
    private void AddFilter()
    {
        string field = AnsiConsole.Prompt(new SelectionPrompt<string>().Title("Выберите поле для фильтрации:").AddChoices("Регион", "Валюта", "ID товара", "Наименование",
            "Количество", "Цена", "Сумма", "Сумма в руб.", "Назад"));
        string? field_val;

        while (true) {
            Console.WriteLine("Введите значение поля (для выхода из режима добавления фильтра введите пробел):");
            field_val = Console.ReadLine();
            if (field_val == " ")
            {
                return;
            }
            if (field_val is not null or "")
            {
                break;
            }
            Console.WriteLine("Пустое поле. Ошибка.");
        }
        while (field is "Количество" or "Цена" or "Сумма" or "Сумма в руб.")
        {
            if (field_val != null && int.Parse(field_val) >= 0)
            {
                break;
            }
            Console.WriteLine("Введите неотрицательное значение поля:");
            field_val = Console.ReadLine();
        }
        field_val ??= "";
        filters[field] = field_val;
    }

    /// <summary>
    /// Добавление сортировки
    /// </summary>
    private void SetSort()
    {
        string? s = AnsiConsole.Prompt(new SelectionPrompt<string>().Title("Выберите поле для сортировки:").AddChoices("Регион", "Валюта", "ID товара", 
            "Наименование", "Количество", "Цена", "Сумма", "Сумма в руб.", "Назад"));
        if (s == "Назад")
        {
            return;
        }
        sort_field = s;
        sort_less = AnsiConsole.Confirm("Сортировать по возрастанию?");
    }
    /// <summary>
    /// Удаление фильтра
    /// </summary>
    private void RemoveFilter()
    {
        if (filters.Any())
        {
            string field = AnsiConsole.Prompt(new SelectionPrompt<string>().Title("Выберите фильтр для отмены:").AddChoices(filters.Keys));
            _ = filters.Remove(field);
        }
    }
}