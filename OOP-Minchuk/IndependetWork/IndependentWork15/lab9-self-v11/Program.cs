using Lab9Self;

Console.OutputEncoding = System.Text.Encoding.UTF8;

DemoNumbers();
Console.WriteLine();
DemoWords();
Console.WriteLine();
DemoProducts();
Console.WriteLine();
DemoStudents();
Console.WriteLine();
DemoDelegateVariables();

static void DemoNumbers()
{
    Console.WriteLine("=== 1. List<int> ===");

    var numbers = new List<int> { 7, -3, 12, 0, 25, 8, -15, 4, 19 };
    Console.WriteLine("Числа: " + string.Join(", ", numbers));

    var squares = CollectionUtils.Map(numbers, n => n * n);
    Console.WriteLine("Map (квадрати): " + string.Join(", ", squares));

    var descriptions = CollectionUtils.Map(numbers, n => n % 2 == 0 ? $"{n}-парне" : $"{n}-непарне");
    Console.WriteLine("Map (int -> string): " + string.Join(", ", descriptions));

    var positiveEven = CollectionUtils.Filter(numbers, n => n > 0 && n % 2 == 0);
    Console.WriteLine("Filter (додатні парні): " + string.Join(", ", positiveEven));

    int maxByAbs = CollectionUtils.MaxBy(numbers, n => Math.Abs(n));
    Console.WriteLine($"MaxBy (найбільший за модулем): {maxByAbs}");

    int sum = 0;
    CollectionUtils.ForEach(numbers, n => sum += n);
    Console.WriteLine($"ForEach (сума через замикання): {sum}");
}

static void DemoWords()
{
    Console.WriteLine("=== 2. List<string> ===");

    var words = new List<string> { "делегат", "лямбда", "generic", "LINQ", "колекція", "предикат", "C#" };
    Console.WriteLine("Слова: " + string.Join(", ", words));

    var upper = CollectionUtils.Map(words, w => w.ToUpperInvariant());
    Console.WriteLine("Map (верхній регістр): " + string.Join(", ", upper));

    var lengths = CollectionUtils.Map(words, w => w.Length);
    Console.WriteLine("Map (довжини): " + string.Join(", ", lengths));

    var longWords = CollectionUtils.Filter(words, w => w.Length > 6);
    Console.WriteLine("Filter (довші за 6 символів): " + string.Join(", ", longWords));

    string longest = CollectionUtils.MaxBy(words, w => w.Length);
    Console.WriteLine($"MaxBy (найдовше слово): {longest}");

    string lastAlphabetically = CollectionUtils.MaxBy(words, w => w);
    Console.WriteLine($"MaxBy (останнє за алфавітом): {lastAlphabetically}");

    Console.WriteLine("ForEach (нумерований список):");
    int index = 1;
    CollectionUtils.ForEach(words, w => Console.WriteLine($"  {index++}. {w}"));
}

static void DemoProducts()
{
    Console.WriteLine("=== 3. List<Product> ===");

    var products = new List<Product>
    {
        new() { Name = "Laptop", Price = 25000, Category = "Комп'ютери" },
        new() { Name = "Mouse", Price = 500, Category = "Аксесуари" },
        new() { Name = "Keyboard", Price = 1500, Category = "Аксесуари" },
        new() { Name = "Monitor", Price = 7200, Category = "Комп'ютери" },
        new() { Name = "Headphones", Price = 2300, Category = "Аудіо" },
        new() { Name = "USB Cable", Price = 150, Category = "Аксесуари" }
    };

    var names = CollectionUtils.Map(products, p => p.Name);
    Console.WriteLine("Map (назви): " + string.Join(", ", names));

    var withDiscount = CollectionUtils.Map(products, p => new { p.Name, NewPrice = p.Price * 0.9m });
    Console.WriteLine("Map (ціна зі знижкою 10%): " +
        string.Join(", ", CollectionUtils.Map(withDiscount, x => $"{x.Name}={x.NewPrice:F0}")));

    Product mostExpensive = CollectionUtils.MaxBy(products, p => p.Price);
    Console.WriteLine($"MaxBy (найдорожчий): {mostExpensive.Name} - {mostExpensive.Price} грн");

    Product longestName = CollectionUtils.MaxBy(products, p => p.Name.Length);
    Console.WriteLine($"MaxBy (найдовша назва): {longestName.Name}");

    var expensive = CollectionUtils.Filter(products, p => p.Price > 1000);
    Console.WriteLine("Filter (дорожчі за 1000 грн):");
    CollectionUtils.ForEach(expensive, p => Console.WriteLine($"  - {p.Name}"));

    var accessories = CollectionUtils.Filter(products, p => p.Category == "Аксесуари");
    Console.WriteLine("Filter (категорія «Аксесуари»): " +
        string.Join(", ", CollectionUtils.Map(accessories, p => p.Name)));

    Console.WriteLine("ForEach (усі продукти):");
    CollectionUtils.ForEach(products, p => Console.WriteLine($"  {p.Name,-11} {p.Price,7} грн  [{p.Category}]"));
}

static void DemoStudents()
{
    Console.WriteLine("=== 4. Student[] (масив теж є IEnumerable<T>) ===");

    Student[] students =
    {
        new() { FullName = "Олена Коваль", Course = 2, AverageGrade = 4.7 },
        new() { FullName = "Андрій Мельник", Course = 3, AverageGrade = 3.9 },
        new() { FullName = "Марія Бондар", Course = 2, AverageGrade = 4.9 },
        new() { FullName = "Іван Шевчук", Course = 1, AverageGrade = 4.1 }
    };

    Student best = CollectionUtils.MaxBy(students, s => s.AverageGrade);
    Console.WriteLine($"MaxBy (найвищий бал): {best.FullName} ({best.AverageGrade})");

    var secondCourse = CollectionUtils.Filter(students, s => s.Course == 2);
    Console.WriteLine("Filter (2 курс): " + string.Join(", ", CollectionUtils.Map(secondCourse, s => s.FullName)));

    var initials = CollectionUtils.Map(students, s =>
    {
        string[] parts = s.FullName.Split(' ');
        return $"{parts[1]} {parts[0][0]}.";
    });
    Console.WriteLine("Map (блочна лямбда, прізвище + ініціал): " + string.Join(", ", initials));
}

static void DemoDelegateVariables()
{
    Console.WriteLine("=== 5. Делегати як змінні та методи замість лямбд ===");

    var numbers = Enumerable.Range(1, 20).ToList();

    Predicate<int> isPrime = IsPrime;
    Func<int, string> toBinary = n => Convert.ToString(n, 2);
    Action<string> print = s => Console.Write(s + " ");

    var primes = CollectionUtils.Filter(numbers, isPrime);
    Console.WriteLine("Filter з іменованим методом IsPrime: " + string.Join(", ", primes));

    Console.Write("Map + ForEach (прості числа у двійковому вигляді): ");
    CollectionUtils.ForEach(CollectionUtils.Map(primes, toBinary), print);
    Console.WriteLine();

    try
    {
        CollectionUtils.MaxBy(new List<int>(), n => n);
    }
    catch (InvalidOperationException ex)
    {
        Console.WriteLine($"MaxBy для порожньої колекції: {ex.Message}");
    }
}

static bool IsPrime(int n)
{
    if (n < 2)
    {
        return false;
    }

    for (int i = 2; i * i <= n; i++)
    {
        if (n % i == 0)
        {
            return false;
        }
    }

    return true;
}
