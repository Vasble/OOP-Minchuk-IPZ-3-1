namespace IndependentWork11;

public static class DataGenerator
{
    private static readonly Dictionary<string, (string[] Names, decimal MinPrice, decimal MaxPrice)> Catalog = new()
    {
        ["Електроніка"] = (new[] { "Смартфон", "Ноутбук", "Планшет", "Навушники", "Смарт-годинник", "Павербанк", "Монітор", "Колонка", "Фотокамера", "Роутер" }, 500m, 45000m),
        ["Книги"] = (new[] { "Кобзар", "Тигролови", "Місто", "Інтернат", "Захар Беркут", "Чистий код", "C# in Depth", "Рефакторинг", "Алгоритми", "Сто років самотності" }, 150m, 1200m),
        ["Одяг"] = (new[] { "Футболка", "Джинси", "Куртка", "Худі", "Кросівки", "Шарф", "Светр", "Сорочка", "Шорти", "Кепка" }, 250m, 6000m),
        ["Дім"] = (new[] { "Чайник", "Ковдра", "Лампа", "Подушка", "Сковорода", "Кавоварка", "Пилосос", "Рушник", "Тостер", "Блендер" }, 200m, 12000m),
        ["Спорт"] = (new[] { "Гантелі", "Йога-мат", "М'яч", "Велосипед", "Скакалка", "Ракетка", "Рюкзак", "Фітнес-браслет", "Пляшка", "Намет" }, 100m, 25000m),
        ["Продукти"] = (new[] { "Кава", "Чай", "Мед", "Шоколад", "Оливкова олія", "Горіхи", "Паста", "Сир", "Сухофрукти", "Спеції" }, 40m, 900m)
    };

    private static readonly string[] Cities =
    {
        "Київ", "Львів", "Харків", "Одеса", "Дніпро", "Рівне", "Луцьк", "Тернопіль", "Вінниця", "Полтава"
    };

    private static readonly double[] CityWeights = { 0.22, 0.14, 0.11, 0.10, 0.10, 0.09, 0.07, 0.07, 0.05, 0.05 };

    private static readonly string[] FirstNames =
    {
        "Олена", "Андрій", "Марія", "Іван", "Оксана", "Дмитро", "Наталія", "Тарас", "Ірина", "Богдан",
        "Софія", "Максим", "Юлія", "Олег", "Анна", "Василь"
    };

    private static readonly string[] LastNames =
    {
        "Коваленко", "Мельник", "Шевченко", "Бондар", "Ткаченко", "Кравченко", "Поліщук", "Савчук",
        "Лисенко", "Романюк", "Гнатюк", "Петренко"
    };

    public static SalesData Generate(int productCount = 60, int customerCount = 80, int saleCount = 1000, int seed = 2026)
    {
        var random = new Random(seed);

        List<Product> products = GenerateProducts(random, productCount);
        List<Customer> customers = GenerateCustomers(random, customerCount);
        List<Sale> sales = GenerateSales(random, saleCount, products, customers);

        return new SalesData(products, customers, sales);
    }

    private static List<Product> GenerateProducts(Random random, int count)
    {
        var products = new List<Product>(count);
        string[] categories = Catalog.Keys.ToArray();

        for (int i = 0; i < count; i++)
        {
            string category = categories[i % categories.Length];
            (string[] names, decimal min, decimal max) = Catalog[category];
            string baseName = names[i / categories.Length % names.Length];
            int series = i / (categories.Length * names.Length);
            string name = series == 0 ? baseName : $"{baseName} {series + 1}";
            decimal price = Math.Round(min + (max - min) * (decimal)Math.Pow(random.NextDouble(), 2), 0);

            products.Add(new Product(i + 1, name, category, price));
        }

        return products;
    }

    private static List<Customer> GenerateCustomers(Random random, int count)
    {
        var customers = new List<Customer>(count);

        for (int i = 0; i < count; i++)
        {
            string name = $"{FirstNames[random.Next(FirstNames.Length)]} {LastNames[random.Next(LastNames.Length)]}";
            customers.Add(new Customer(i + 1, name, PickWeighted(random, Cities, CityWeights)));
        }

        return customers;
    }

    private static List<Sale> GenerateSales(Random random, int count, List<Product> products, List<Customer> customers)
    {
        var sales = new List<Sale>(count);
        var start = new DateTime(2025, 1, 1);
        int days = (new DateTime(2026, 10, 31) - start).Days + 1;

        for (int i = 0; i < count; i++)
        {
            int productIndex = (int)(Math.Pow(random.NextDouble(), 1.6) * products.Count);
            Product product = products[productIndex];
            int maxQuantity = product.UnitPrice > 5000 ? 2 : product.UnitPrice > 1000 ? 4 : 8;

            sales.Add(new Sale(
                i + 1,
                product.Id,
                random.Next(1, maxQuantity + 1),
                start.AddDays(random.Next(days)).AddMinutes(random.Next(24 * 60)),
                customers[random.Next(customers.Count)].Id));
        }

        return sales;
    }

    private static string PickWeighted(Random random, string[] items, double[] weights)
    {
        double roll = random.NextDouble() * weights.Sum();

        for (int i = 0; i < items.Length; i++)
        {
            roll -= weights[i];

            if (roll <= 0)
            {
                return items[i];
            }
        }

        return items[^1];
    }
}
