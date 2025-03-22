using System.Diagnostics;

/// <summary>
/// Класс для хранения продажи (единицы)
/// </summary>
/// <param name="date">Дата продажи</param>
/// <param name="productId">ID товара</param>
/// <param name="name">Наименование товара</param>
/// <param name="cnt">Количество</param>
/// <param name="price">Цена</param>
/// <param name="region">Регион продажи</param>
/// <param name="currency">Валюта, в которой была произведена продажа</param>
/// <param name="rate">Курс валюты в указанную дату</param>
public class Sale(DateTime date, string productId, string name, int cnt, decimal price, string region, string currency, decimal rate)
{
    public static int ID_cnt { get; private set; } = 0;

    public int Id { get; private set; } = ++ID_cnt; // ID продажи
    public DateTime Date { get; private set; } = date; // Дата продажи
    public string ProductId { get; private set; } = productId; // ID товара
    public string ProductName { get; private set; } = name; // Наименование товара
    public int Quantity { get; private set; } = cnt; // Количество
    public decimal Price { get; private set; } = price; // Цена
    public string Region { get; private set; } = region; // Регион
    public string Currency { get; private set; } = currency; // Валюта
    public decimal Sum { get; private set; } = price * cnt; // Сумма в исходной валюте
    public decimal RubSum { get; private set; } = currency == "RUB" ? cnt * price :
            cnt * price * rate; // Сумма в рублях
    public bool Del { get; private set; } // Флаг - удалена ли продажа

    /// <summary>
    /// Получение курса валюты в конкретную дату
    /// </summary>
    /// <param name="currency">Валюта</param>
    /// <param name="date">Дата</param>
    /// <returns>Курс</returns>
    private async Task<decimal> GetRate(string currency, DateTime date)
    {
        return await CurrencyManager.GetExchangeRateAsync(currency, date);
    }

    /// <summary>
    /// Создание продажи по входным данным
    /// </summary>
    /// <param name="date">Дата продажи</param>
    /// <param name="productId">ID товара</param>
    /// <param name="name">Наименование товара</param>
    /// <param name="cnt">Количество</param>
    /// <param name="price">Цена</param>
    /// <param name="region">Регион продажи</param>
    /// <param name="currency">Валюта, в которой была произведена продажа</param>
    /// <returns></returns>
    public static async Task<Sale> CreateSale(DateTime date, string productId, string name, int cnt, decimal price, string region, string currency)
    {
        decimal rate = currency == "RUB" ? 1 : await CurrencyManager.GetExchangeRateAsync(currency, date);

        return new(date, productId, name, cnt, price, region, currency, rate);
    }

    /// <summary>
    /// Изменение даты продажи
    /// </summary>
    /// <param name="new_date">Новая дата</param>
    public async void DateChange(DateTime new_date)
    {
        RubSum = RubSum * await CurrencyManager.GetExchangeRateAsync(Currency, new_date) / await CurrencyManager.GetExchangeRateAsync(Currency, Date);
        Date = new_date;
    }
    /// <summary>
    /// Изменения ID продажи
    /// </summary>
    /// <param name="new_ID">новый ID</param>
    public void ProductIDChange(string new_ID)
    {
        ProductId = new_ID;
    }
    /// <summary>
    /// Изменение наименования товара
    /// </summary>
    /// <param name="new_name">новое наименование</param>
    public void ProductNameChange(string new_name)
    {
        ProductName = new_name;
    }
    /// <summary>
    /// Изменения количества проданных товаров в продаже
    /// </summary>
    /// <param name="new_cnt">Новое количество</param>
    public void QuantityChange(int new_cnt)
    {
        RubSum = RubSum * new_cnt / Quantity;
        Quantity = new_cnt;
        Sum = Quantity * Price;
    }
    /// <summary>
    /// Изменения цены товара в продаже
    /// </summary>
    /// <param name="new_price">Новая цена</param>
    public void PriceChange(int new_price)
    {
        RubSum = RubSum * new_price / Price;
        Price = new_price;
        Sum = Price * Quantity;
    }
    /// <summary>
    /// Изменения региона продажи
    /// </summary>
    /// <param name="new_region">Новый регион</param>
    public void RegionChange(string new_region)
    {
        Region = new_region;
    }
    /// <summary>
    /// Изменение валюты, в которой была произведена продажа
    /// </summary>
    /// <param name="new_currency">Новая валюта</param>
    public async void CurrencyChange(string new_currency)
    {
        Currency = new_currency;
        RubSum = new_currency == "RUB" ? Quantity * Price:
            Quantity * Price * await CurrencyManager.GetExchangeRateAsync(new_currency, Date);
    }
    /// <summary>
    /// Метод для удаления продажи
    /// </summary>
    public void Delete()
    {
        Del = true;
    }

}