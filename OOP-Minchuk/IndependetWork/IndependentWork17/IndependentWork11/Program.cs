using System.Globalization;
using IndependentWork11;

Console.OutputEncoding = System.Text.Encoding.UTF8;
CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("uk-UA");

SalesData data = DataGenerator.Generate();
var analytics = new SalesAnalytics(data);

PrintDatasetInfo(data);
RunTopProductsByCategory(analytics);
RunRevenueByCity(analytics);
RunSalesByMonth(analytics);
RunTopCustomers(analytics);
RunScalabilityTest();

static void PrintDatasetInfo(SalesData data)
{
    Console.WriteLine("=== Набір даних «Продажі» (Random seed = 2026) ===");
    Console.WriteLine($"Продуктів: {data.Products.Count}, клієнтів: {data.Customers.Count}, продажів: {data.Sales.Count}");
    Console.WriteLine($"Період: {data.Sales.Min(s => s.SaleDate):dd.MM.yyyy} — {data.Sales.Max(s => s.SaleDate):dd.MM.yyyy}");
    Console.WriteLine();
}

static void PrintTiming<T>(Measurement<T> measurement)
{
    Console.WriteLine($"⏱ Перший запуск: {measurement.FirstRunMs:F3} мс | середнє з 50 повторних: {measurement.AverageWarmMs:F3} мс");
    Console.WriteLine();
}

static void RunTopProductsByCategory(SalesAnalytics analytics)
{
    Console.WriteLine("=== Пайплайн 1: Найпопулярніші продукти за категоріями (топ-3 за кількістю) ===");

    Measurement<List<CategoryTopProducts>> m = Measure.Run(() => analytics.TopProductsByCategory());

    foreach (CategoryTopProducts category in m.Result)
    {
        Console.WriteLine($"{category.Category}:");
        int place = 1;
        foreach (ProductSales product in category.TopProducts)
        {
            Console.WriteLine($"  {place++}. {product.ProductName,-20} {product.TotalQuantity,5} шт.");
        }
    }

    PrintTiming(m);
}

static void RunRevenueByCity(SalesAnalytics analytics)
{
    Console.WriteLine("=== Пайплайн 2: Загальний дохід за містами клієнтів ===");

    Measurement<List<CityRevenue>> m = Measure.Run(analytics.RevenueByCity);
    decimal total = m.Result.Sum(c => c.Revenue);

    Console.WriteLine($"{"Місто",-10} | {"Дохід, грн",16} | {"Частка",7} | {"Продажів",8} | {"Клієнтів",8}");
    Console.WriteLine(new string('-', 61));
    foreach (CityRevenue city in m.Result)
    {
        Console.WriteLine($"{city.City,-10} | {city.Revenue,16:N2} | {city.Revenue / total,7:P1} | {city.SalesCount,8} | {city.CustomersCount,8}");
    }
    Console.WriteLine($"{"Разом",-10} | {total,16:N2}");

    List<CityRevenue> manual = analytics.RevenueByCityManual();
    bool same = manual.Select(c => (c.City, c.Revenue)).SequenceEqual(m.Result.Select(c => (c.City, c.Revenue)));
    Console.WriteLine($"Перевірка: результат збігається з ручною реалізацією на циклах — {(same ? "так" : "НІ")}");

    PrintTiming(m);
}

static void RunSalesByMonth(SalesAnalytics analytics)
{
    Console.WriteLine("=== Пайплайн 3: Продажі за місяцями та роками ===");

    Measurement<List<MonthlySales>> m = Measure.Run(analytics.SalesByMonth);

    Console.WriteLine($"{"Рік",4} | {"Місяць",-9} | {"Одиниць",7} | {"Продажів",8} | {"Дохід, грн",14}");
    Console.WriteLine(new string('-', 55));
    foreach (MonthlySales month in m.Result)
    {
        string monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month.Month);
        Console.WriteLine($"{month.Year,4} | {monthName,-9} | {month.TotalQuantity,7} | {month.SalesCount,8} | {month.Revenue,14:N2}");
    }

    foreach (var year in m.Result.GroupBy(x => x.Year))
    {
        Console.WriteLine($"Підсумок {year.Key}: {year.Sum(x => x.TotalQuantity)} од., {year.Sum(x => x.Revenue):N2} грн ({year.Count()} міс.)");
    }

    PrintTiming(m);
}

static void RunTopCustomers(SalesAnalytics analytics)
{
    Console.WriteLine("=== Пайплайн 4 (додатковий): Найцінніші клієнти та їхня улюблена категорія ===");
    Console.WriteLine("Клієнти з витратами вище середнього і щонайменше 5 замовленнями, топ-10 за сумою:");

    Measurement<List<CustomerInsight>> m = Measure.Run(() => analytics.TopCustomersWithFavoriteCategory());

    Console.WriteLine($"{"Клієнт",-20} | {"Місто",-9} | {"Замовл.",7} | {"Витрачено, грн",14} | {"Сер. чек",10} | Улюблена категорія");
    Console.WriteLine(new string('-', 92));
    foreach (CustomerInsight c in m.Result)
    {
        Console.WriteLine($"{c.CustomerName,-20} | {c.City,-9} | {c.OrdersCount,7} | {c.TotalSpent,14:N2} | {c.AverageOrder,10:N2} | {c.FavoriteCategory}");
    }

    PrintTiming(m);
}

static void RunScalabilityTest()
{
    Console.WriteLine("=== Масштабування: середній час пайплайнів (мс) при різній кількості продажів ===");
    Console.WriteLine($"{"Продажів",9} | {"П1 Топ-3",9} | {"П2 Міста",9} | {"П2 цикли",9} | {"П3 Місяці",9} | {"П4 Клієнти",10}");
    Console.WriteLine(new string('-', 70));

    foreach (int saleCount in new[] { 1_000, 10_000, 100_000 })
    {
        var analytics = new SalesAnalytics(DataGenerator.Generate(saleCount: saleCount));
        int runs = saleCount >= 100_000 ? 5 : 30;

        double p1 = Measure.AverageMs(() => analytics.TopProductsByCategory(), runs);
        double p2 = Measure.AverageMs(() => analytics.RevenueByCity(), runs);
        double p2Manual = Measure.AverageMs(() => analytics.RevenueByCityManual(), runs);
        double p3 = Measure.AverageMs(() => analytics.SalesByMonth(), runs);
        double p4 = Measure.AverageMs(() => analytics.TopCustomersWithFavoriteCategory(), runs);

        Console.WriteLine($"{saleCount,9} | {p1,9:F3} | {p2,9:F3} | {p2Manual,9:F3} | {p3,9:F3} | {p4,10:F3}");
    }
}
