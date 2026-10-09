using System.Runtime.CompilerServices;

Console.OutputEncoding = System.Text.Encoding.UTF8;

Console.WriteLine("=== 1. Об'єкт з using ===");
using (var pool1 = new ThreadPool(4))
{
    pool1.ExecuteTask("Обробка зображень");
    pool1.ExecuteTask("Надсилання листів");
} 

Console.WriteLine("\n=== 2. Явний виклик Dispose() ===");
var pool2 = new ThreadPool(8);
pool2.ExecuteTask("Архівація файлів");
pool2.Dispose();
pool2.Dispose(); 

try
{
    pool2.ExecuteTask("Завдання після Dispose");
}
catch (ObjectDisposedException ex)
{
    Console.WriteLine($"Очікуваний виняток: {ex.GetType().Name}");
}

Console.WriteLine("\n=== 3. Без Dispose(): робота деструктора ===");
CreateLeakedPool();
GC.Collect();
GC.WaitForPendingFinalizers();
Console.WriteLine("Збирання сміття завершено.");

Console.WriteLine("\nГотово.");

[MethodImpl(MethodImplOptions.NoInlining)]
static void CreateLeakedPool()
{
    var leaked = new ThreadPool(2);
    leaked.ExecuteTask("Завдання без Dispose");
}