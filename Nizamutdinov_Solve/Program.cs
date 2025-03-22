/*
 * ФИО: Низамутдинов Азат Артурович
 * Группа: 245-2
 * Вариант: 3
 * P.S. выполнена полность
 */
using Spectre.Console;

public class Program
{
    /// <summary>
    /// Основная функция программы
    /// </summary>
    private static async Task Main()
    {
        while (true)
        {
            AnsiConsole.MarkupLine("[yellow]Добро пожаловать в Анализатор продаж![/]");
            AnsiConsole.MarkupLine("Перед началом подготовьте файл с данными в формате CSV.");
            AnsiConsole.MarkupLine("Формат файла:\nФайл состоит из строк вида:");
            AnsiConsole.MarkupLine("[grey]Дата продажи (dd-MM-yyyy),ID товара,Наименование товара,Кол-во товара,Цена единицы,Регион продажи,В какой валюте была продажа (RUB, USD, ...)[/]");
            AnsiConsole.MarkupLine("Пример строки: [grey]01-05-2022,101,Товар А,12,123,Россия,RUB[/]");
            AnsiConsole.MarkupLine("Сохраните данные в файл формата .txt (например, sales.txt).");

            Data? data;
            await CurrencyManager.PrepareAsync();
            while (true)
            {
                Console.WriteLine("Введите полный путь до файла с данными:");
                string? path = Console.ReadLine();

                data = new(path, out Task<bool> flag);
                if (await flag == true)
                {
                    break;
                }
            }

            UI ui = new(data);
            ui.Run();
        }
    }
}