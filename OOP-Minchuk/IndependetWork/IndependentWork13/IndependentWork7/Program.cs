using IndependentWork7;

Console.OutputEncoding = System.Text.Encoding.UTF8;

Console.WriteLine("=== Демонстрація Cache<string, string> (FIFO) ===");

var cache = new Cache<string, string>(3);
Console.WriteLine($"Створено кеш ємністю {cache.Capacity}.");

cache.Add("ua", "Україна");
cache.Add("pl", "Польща");
cache.Add("de", "Німеччина");
Console.WriteLine($"Додано 3 елементи. Count = {cache.Count}");

Console.WriteLine($"Get(\"ua\") -> {cache.Get("ua")}");
Console.WriteLine($"Get(\"pl\") -> {cache.Get("pl")}");

Console.WriteLine();
Console.WriteLine("Додаємо \"fr\" — кеш повний, тому видаляється найстаріший (\"ua\").");
cache.Add("fr", "Франція");
PrintState(cache, "ua", "pl", "de", "fr");

Console.WriteLine();
Console.WriteLine("Додаємо \"it\" — тепер видаляється \"pl\".");
cache.Add("it", "Італія");
PrintState(cache, "pl", "de", "fr", "it");

Console.WriteLine();
Console.WriteLine("Спроба отримати видалений ключ \"ua\":");
try
{
    cache.Get("ua");
}
catch (KeyNotFoundException ex)
{
    Console.WriteLine($"KeyNotFoundException: {ex.Message}");
}

Console.WriteLine();
Console.WriteLine("=== Демонстрація Cache<int, Student> ===");

var students = new Cache<int, Student>(2);
students.Add(1, new Student("Олена", 4.8));
students.Add(2, new Student("Андрій", 4.2));
students.Add(3, new Student("Марія", 4.5)); 

for (int id = 1; id <= 3; id++)
{
    string info = students.ContainsKey(id) ? students.Get(id).ToString() : "немає в кеші";
    Console.WriteLine($"Студент #{id}: {info}");
}
Console.WriteLine($"Count = {students.Count}");

static void PrintState(Cache<string, string> cache, params string[] keys)
{
    foreach (string key in keys)
    {
        string status = cache.ContainsKey(key) ? $"є -> {cache.Get(key)}" : "немає";
        Console.WriteLine($"  ContainsKey(\"{key}\"): {status}");
    }
    Console.WriteLine($"  Count = {cache.Count}");
}

public record Student(string Name, double AverageGrade)
{
    public override string ToString() => $"{Name}, середній бал {AverageGrade}";
}
