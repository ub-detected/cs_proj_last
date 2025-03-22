using Spectre.Console;
using System.Globalization;
using System.Security;

/// <summary>
/// класс для хранения продаж
/// </summary>
public class Data
{
    private readonly List<Sale> sales = [];
    private string filePath = "";
    public IReadOnlyList<Sale> Sales => sales.AsReadOnly();

    /// <summary>
    /// конструктор
    /// </summary>
    /// <param name="path">путь к файлу</param>
    /// <param name="flag">удалось ли получить данные из файла (bool)</param>
    public Data(string? path, out Task<bool> flag)
    {
        if (path is null or "")
        {
            flag = Task.FromResult(false);
            Console.WriteLine("Введите корректный путь до файла.");
            return;
        } 
        filePath = path;
        flag = LoadData();
    }
    /// <summary>
    /// проверка строки файла на корректность
    /// </summary>
    /// <param name="fields">поля строки</param>
    /// <returns>Корректна или нет (bool)</returns>
    private bool Check(string[] fields)
    {
        if (fields.Length != 7)
        {
            Console.WriteLine($"Некорректное количество полей в записи.\n" +
                $"Возможно наличие лишних разделяющих запятых (суммарно их должно быть 6 для 7 полей.)\n" +
                $"Поля не должны содержат запятые.");
            return false;
        }
        
        if (!DateTime.TryParseExact(fields[0], "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out _))
        {
            Console.WriteLine("Некорректный формат даты.");
            return false;
        }

        if (fields[1] == "")
        {
            Console.WriteLine("Пустой ID товара.");
            return false;
        }

        if (fields[2] == "")
        {
            Console.WriteLine("Пустое наименование товара.");
            return false;
        }

        if (!int.TryParse(fields[3], out int cnt) || cnt < 0)
        {
            Console.WriteLine("Проверьте корректность поля <Кол-во товара> (+ Число должно быть неотрицательным).");
            return false;
        }

        if (!int.TryParse(fields[4], out int price) || price < 0)
        {
            Console.WriteLine("Проверьте корректность поля <Цена единицы> (+ Число должно быть неотрицательным).");
            return false;
        }

        if (fields[5] == "")
        {
            Console.WriteLine("Пустой регион продажи.");
            return false;
        }

        if (fields[6] == "" || !CurrencyManager.Currencies.Contains(fields[6]))
        {
            Console.WriteLine("Такой валюты не существует.");
            return false;
        }
        return true;
    }
    /// <summary>
    /// Добавление продажи
    /// </summary>
    /// <param name="fields">Строка для добавления</param>
    /// <returns>Удалось добавить или нет (bool)</returns>
    public async Task<bool> AddSale(string[] fields)
    {
        if (Check(fields))
        {
            Sale sale = await Sale.CreateSale(
                    date: DateTime.ParseExact(fields[0], "dd-MM-yyyy", CultureInfo.InvariantCulture),
                    productId: fields[1],
                    name: fields[2],
                    cnt: int.Parse(fields[3]),
                    price: decimal.Parse(fields[4], CultureInfo.InvariantCulture),
                    region: fields[5],
                    currency: fields[6]
                );
            sales.Add(sale);
            return true;
        }
        return false;
    }
    /// <summary>
    /// Загрузка данных из файла
    /// </summary>
    /// <returns>Удалось или нет</returns>
    private async Task<bool> LoadData()
    {
        try
        {
            filePath = Path.GetFullPath(filePath);
            IEnumerable<string> lines = File.ReadAllLines(filePath);
            int lines_cnt = 0;

            foreach (string line in lines)
            {
                ++lines_cnt;
                string[] fields = line.Split(',');

                if (!Check(fields))
                {
                    Console.WriteLine($"Ошибка в строке под номером {lines_cnt}");
                    return false;
                }

                Sale sale = await Sale.CreateSale(
                    date: DateTime.ParseExact(fields[0], "dd-MM-yyyy", CultureInfo.InvariantCulture),
                    productId: fields[1],
                    name: fields[2],
                    cnt: int.Parse(fields[3]),
                    price: decimal.Parse(fields[4], CultureInfo.InvariantCulture),
                    region: fields[5],
                    currency: fields[6]
                );
                sales.Add(sale);
            }
            return true;
        }
        catch (FileNotFoundException)
        {
            Console.WriteLine("Файл по указанному пути не найден.");
            return false;
        }
        catch (SecurityException)
        {
            Console.WriteLine("Нет доступа к файлу.");
            return false;
        }
        catch (DirectoryNotFoundException)
        {
            Console.WriteLine("Ошибка в указанном пути.");
            return false;
        }
        catch (IOException)
        {
            Console.WriteLine("Ошибка ввода/вывода.");
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            Console.WriteLine("Произошла ошибка при получении доступа к файлу.");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Загрузка данных в файл
    /// </summary>
    /// <returns>Удалось или нет</returns>
    public bool SaveToFile(TableManager t)
    {
        try {
            List<string> lines = [];
            Console.WriteLine("В файл запишутся строки в формате:\n" +
                "Дата,ID товара,Наименование,Количество,Цена,Регион,Валюта,Сумма,Сумма в руб.");
            IEnumerable<Sale> res = t.ApplyFiltersAndSorting(sales);
                
            lines.AddRange(res.Select(s => $"{s.Date:dd-MM-yyyy},{s.ProductId},{s.ProductName},{s.Quantity}," +
            $"{s.Price.ToString(CultureInfo.InvariantCulture)},{s.Region},{s.Currency}," +
            $"{s.Sum.ToString(CultureInfo.InvariantCulture)},{s.RubSum.ToString(CultureInfo.InvariantCulture)}"));
            
            File.WriteAllLines(filePath, lines);
            return true;
        }
        catch (FileNotFoundException)
        {
            Console.WriteLine("Файл по указанному ранее пути не найден.");
            return false;
        }
        catch (SecurityException)
        {
            Console.WriteLine("Нет доступа к файлу.");
            return false;
        }
        catch (IOException)
        {
            Console.WriteLine("Ошибка ввода/вывода.");
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            Console.WriteLine("Произошла ошибка при получении доступа к файлу.");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return false;
        }
    }
    /// <summary>
    /// Добавление продажи
    /// </summary>
    /// <param name="sale">продажа</param>
    public void AddSale(Sale sale)
    {
        sales.Add(sale);
    }
    /// <summary>
    /// Удаление продажи
    /// </summary>
    /// <param name="id">id продажи</param>
    public void RemoveSale(int id)
    {
        if (sales[id-1].Del)
        {
            Console.WriteLine("Продажа с указанным id уже была удалена ранее.");
            return;
        }
        sales[id - 1].Delete();
    }
}