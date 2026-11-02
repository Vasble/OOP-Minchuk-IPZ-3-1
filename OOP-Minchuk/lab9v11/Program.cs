using System.Diagnostics;
using System.Text;
using Lab9v11;

Console.OutputEncoding = Encoding.UTF8;

Console.WriteLine("=== Лабораторна робота №9, варіант 11: Система кешування ===");
Console.WriteLine();

var cache = new CacheSystem(capacity: 3);

Header("1. HashSet: додавання унікальних ключів");
cache.Put("user:1", "Іван");
cache.Put("user:2", "Олена");
cache.Put("config", "dark-theme");
cache.PrintState();

Header("2. HashSet: спроба додати дублікат ключа");
cache.Put("user:1", "Іван Петренко");
Console.WriteLine($"  Кількість ключів не змінилась: {cache.Count}");
cache.PrintState();

Header("3. HashSet: перевірка наявності (Contains)");
foreach (var key in new[] { "user:2", "user:99" })
    Console.WriteLine($"  Contains(\"{key}\") = {cache.ContainsKey(key)}");

Header("4. Queue: звернення до ключів змінюють порядок LRU");
Console.WriteLine($"  До звернень: {string.Join(" -> ", cache.EvictionOrder)}");
cache.Get("user:2");
cache.Get("user:99");
Console.WriteLine($"  Після звернень: {string.Join(" -> ", cache.EvictionOrder)}");
Console.WriteLine($"  Peek — першим буде видалено: {cache.PeekNextToEvict()}");

Header("5. Queue: переповнення кешу — автоматичне витіснення (Dequeue)");
cache.Put("session:A", "token-123");
cache.PrintState();
cache.Put("session:B", "token-456");
cache.PrintState();

Header("6. HashSet: видалення ключа (Remove)");
cache.Remove("session:A");
cache.Remove("unknown");
cache.PrintState();

Header("7. HashSet: операції над множинами");
var requested = new[] { "user:2", "config", "session:B", "user:1" };
Console.WriteLine($"  Запитані ключі: {{ {string.Join(", ", requested)} }}");
Console.WriteLine($"  Є в кеші (IntersectWith): {{ {string.Join(", ", cache.CommonKeys(requested))} }}");
Console.WriteLine($"  Треба завантажити (ExceptWith): {{ {string.Join(", ", cache.MissingKeys(requested))} }}");

var backupKeys = new HashSet<string> { "user:1", "user:3" };
var allKeys = cache.GetKeys();
allKeys.UnionWith(backupKeys);
Console.WriteLine($"  Ключі кешу ∪ резервні ключі (UnionWith): {{ {string.Join(", ", allKeys)} }}");

Header("8. Stack: історія звернень (LIFO — від останнього до першого)");
Console.WriteLine($"  Записів в історії: {cache.HistoryCount}");
int n = 1;
foreach (var record in cache.History)
    Console.WriteLine($"  {n++,2}. {record}");

Header("9. Stack: Peek та Pop");
Console.WriteLine($"  Peek (вершина, не видаляється): {cache.PeekLastAccess()}");
Console.WriteLine($"  Записів після Peek: {cache.HistoryCount}");
Console.WriteLine("  Переглядаємо 3 останні звернення через Pop:");
for (int i = 0; i < 3; i++)
    Console.WriteLine($"    Pop -> {cache.PopLastAccess()}");
Console.WriteLine($"  Записів після Pop: {cache.HistoryCount}");
Console.WriteLine($"  Нова вершина: {cache.PeekLastAccess()}");

Header("10. Порівняння продуктивності: List.Contains vs HashSet.Contains");
ComparePerformance(100_000);

Header("11. Порядок обробки: Queue (FIFO) vs Stack (LIFO)");
string[] items = { "A", "B", "C", "D" };
var queue = new Queue<string>(items);
var stack = new Stack<string>(items);
Console.WriteLine($"  Додано в порядку:  {string.Join(", ", items)}");
Console.WriteLine($"  Queue віддає:      {string.Join(", ", DrainQueue(queue))}");
Console.WriteLine($"  Stack віддає:      {string.Join(", ", DrainStack(stack))}");

Console.WriteLine();
Console.WriteLine("=== Кінець демонстрації ===");


static void Header(string title)
{
    Console.WriteLine();
    Console.WriteLine($"--- {title} ---");
}

static void ComparePerformance(int size)
{
    var list = new List<string>(size);
    var set = new HashSet<string>(size);
    for (int i = 0; i < size; i++)
    {
        string key = $"key:{i}";
        list.Add(key);
        set.Add(key);
    }

    const int lookups = 1_000;
    var rnd = new Random(42);
    var probes = new string[lookups];
    for (int i = 0; i < lookups; i++)
        probes[i] = $"key:{rnd.Next(size * 2)}";

    int foundList = 0, foundSet = 0;

    var sw = Stopwatch.StartNew();
    foreach (var p in probes)
        if (list.Contains(p)) foundList++;
    sw.Stop();
    double listMs = sw.Elapsed.TotalMilliseconds;

    sw.Restart();
    foreach (var p in probes)
        if (set.Contains(p)) foundSet++;
    sw.Stop();
    double setMs = sw.Elapsed.TotalMilliseconds;

    Console.WriteLine($"  Колекції по {size:N0} ключів, {lookups:N0} пошуків.");
    Console.WriteLine($"  List<string>.Contains    (O(n)): {listMs,10:F3} мс, знайдено {foundList}");
    Console.WriteLine($"  HashSet<string>.Contains (O(1)): {setMs,10:F3} мс, знайдено {foundSet}");
    if (setMs > 0)
        Console.WriteLine($"  HashSet швидший приблизно у {listMs / setMs:F0} разів.");
}

static IEnumerable<string> DrainQueue(Queue<string> q)
{
    while (q.Count > 0)
        yield return q.Dequeue();
}

static IEnumerable<string> DrainStack(Stack<string> s)
{
    while (s.Count > 0)
        yield return s.Pop();
}
