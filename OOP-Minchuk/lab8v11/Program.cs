using System.Text;
using lab8v11;

Console.OutputEncoding = Encoding.UTF8;


var numbers = new List<int> { -5, 12, -3, 8, 0, 7, -1, 20 };

Console.WriteLine("=== List<int> ===");
Console.WriteLine($"Числа: {string.Join(", ", numbers)}");
Console.WriteLine($"Кількість від'ємних чисел:  {CollectionHelper<int>.Count(numbers, n => n < 0)}");
Console.WriteLine($"Кількість парних чисел:     {CollectionHelper<int>.Count(numbers, n => n % 2 == 0)}");
Console.WriteLine($"Кількість чисел більших за 10: {CollectionHelper<int>.Count(numbers, n => n > 10)}");
Console.WriteLine();


var words = new List<string> { "Apple", "banana", "Avocado", "cherry", "apricot", "Almond" };

Console.WriteLine("=== List<string> ===");
Console.WriteLine($"Слова: {string.Join(", ", words)}");
Console.WriteLine($"Рядків, що починаються на \"A\" (з урахуванням регістру): " +
                  $"{CollectionHelper<string>.Count(words, w => w.StartsWith('A'))}");
Console.WriteLine($"Рядків, що починаються на \"a\"/\"A\" (без урахування регістру): " +
                  $"{CollectionHelper<string>.Count(words, w => w.StartsWith("a", StringComparison.OrdinalIgnoreCase))}");
Console.WriteLine($"Рядків довших за 5 символів: {CollectionHelper<string>.Count(words, w => w.Length > 5)}");
Console.WriteLine();


var products = new List<Product>
{
    new() { Id = 1, Name = "Laptop",   Category = "Electronics", Price = 1200m },
    new() { Id = 2, Name = "Mouse",    Category = "Electronics", Price = 25m },
    new() { Id = 3, Name = "Keyboard", Category = "Electronics", Price = 80m },
    new() { Id = 4, Name = "Apple",    Category = "Food",        Price = 2m },
    new() { Id = 5, Name = "Bread",    Category = "Food",        Price = 1.5m },
    new() { Id = 6, Name = "Milk",     Category = "Food",        Price = 1.2m },
    new() { Id = 7, Name = "Novel",    Category = "Books",       Price = 15m },
    new() { Id = 8, Name = "Monitor",  Category = "Electronics", Price = 300m }
};

Console.WriteLine("=== List<Product> ===");
Console.WriteLine($"Усього продуктів: {products.Count}");
Console.WriteLine($"Категорія Electronics: {CollectionHelper<Product>.Count(products, p => p.Category == "Electronics")}");
Console.WriteLine($"Категорія Food:        {CollectionHelper<Product>.Count(products, p => p.Category == "Food")}");
Console.WriteLine($"Категорія Books:       {CollectionHelper<Product>.Count(products, p => p.Category == "Books")}");
Console.WriteLine($"Продуктів з ціною > 100: {CollectionHelper<Product>.Count(products, p => p.Price > 100m)}");
Console.WriteLine();


Console.WriteLine("=== Граничні випадки ===");

var empty = new List<int>();
Console.WriteLine($"Порожня колекція:           {CollectionHelper<int>.Count(empty, n => n < 0)}");
Console.WriteLine($"Жоден елемент не підходить: {CollectionHelper<int>.Count(numbers, n => n > 1000)}");
Console.WriteLine($"Усі елементи підходять:     {CollectionHelper<int>.Count(numbers, _ => true)}");

try
{
    CollectionHelper<int>.Count(null!, n => n < 0);
}
catch (ArgumentNullException ex)
{
    Console.WriteLine($"collection = null  -> ArgumentNullException (параметр: {ex.ParamName})");
}

try
{
    CollectionHelper<int>.Count(numbers, null!);
}
catch (ArgumentNullException ex)
{
    Console.WriteLine($"predicate = null   -> ArgumentNullException (параметр: {ex.ParamName})");
}