using System.Text;
using lab18v11.Helpers;
using lab18v11.Services;

namespace lab18v11;

internal class Program
{
    private const string ApiUrl = "https://api.shop.example/products";
    private static readonly TimeSpan InitialDelay = TimeSpan.FromMilliseconds(300);
    private static readonly Func<Exception, bool> ShouldRetry =
        ex => ex is FileNotFoundException or HttpRequestException;

    private static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;

        string productsFile = Path.Combine(AppContext.BaseDirectory, "products.txt");
        CreateProductsFile(productsFile);

        try
        {
            Scenario1_FileProcessor(productsFile);
            Scenario2_NetworkClient();
            Scenario3_NotRetriedIOException(productsFile);
            Scenario4_RetriesExhausted();
        }
        finally
        {
            if (File.Exists(productsFile))
                File.Delete(productsFile);

            Console.WriteLine("\nТимчасовий файл products.txt видалено. Роботу завершено.");
        }
    }

    private static void Scenario1_FileProcessor(string path)
    {
        PrintHeader("Сценарій 1. FileProcessor.LoadProductNames — FileNotFoundException x2, потім успіх");

        var fileProcessor = new FileProcessor(transientFailures: 2);

        try
        {
            List<string> names = RetryHelper.ExecuteWithRetry(
                () => fileProcessor.LoadProductNames(path),
                retryCount: 3,
                initialDelay: InitialDelay,
                shouldRetry: ShouldRetry);

            PrintList("Продукти з файлу", names);
        }
        catch (Exception ex)
        {
            PrintError(ex);
        }
        finally
        {
            Console.WriteLine($"  Усього викликів LoadProductNames: {fileProcessor.CallCount}");
        }
    }

    private static void Scenario2_NetworkClient()
    {
        PrintHeader("Сценарій 2. NetworkClient.GetProductsFromApi — HttpRequestException x3, потім успіх");

        var networkClient = new NetworkClient(transientFailures: 3);

        try
        {
            List<string> products = RetryHelper.ExecuteWithRetry(
                () => networkClient.GetProductsFromApi(ApiUrl),
                retryCount: 3,
                initialDelay: InitialDelay,
                shouldRetry: ShouldRetry);

            PrintList("Продукти з API", products);
        }
        catch (HttpRequestException ex)
        {
            PrintError(ex);
        }
        finally
        {
            Console.WriteLine($"  Усього запитів до API: {networkClient.CallCount}");
        }
    }

    private static void Scenario3_NotRetriedIOException(string path)
    {
        PrintHeader("Сценарій 3. shouldRetry — IOException (файл заблоковано) не повторюється");

        var lockedFileProcessor = new FileProcessor(
            transientFailures: 1,
            errorFactory: p => new IOException($"Файл {Path.GetFileName(p)} заблоковано іншим процесом (імітація)."));

        try
        {
            List<string> names = RetryHelper.ExecuteWithRetry(
                () => lockedFileProcessor.LoadProductNames(path),
                retryCount: 3,
                initialDelay: InitialDelay,
                shouldRetry: ShouldRetry);

            PrintList("Продукти з файлу", names);
        }
        catch (IOException ex)
        {
            PrintError(ex);
        }
        finally
        {
            Console.WriteLine($"  Усього викликів LoadProductNames: {lockedFileProcessor.CallCount} (без повторів)");
        }
    }

    private static void Scenario4_RetriesExhausted()
    {
        PrintHeader("Сценарій 4. Файлу не існує — повтори вичерпано");

        var fileProcessor = new FileProcessor(transientFailures: 0);
        string missingPath = Path.Combine(AppContext.BaseDirectory, "missing_products.txt");

        try
        {
            List<string> names = RetryHelper.ExecuteWithRetry(
                () => fileProcessor.LoadProductNames(missingPath),
                retryCount: 2,
                initialDelay: InitialDelay,
                shouldRetry: ShouldRetry);

            PrintList("Продукти з файлу", names);
        }
        catch (FileNotFoundException ex)
        {
            PrintError(ex);
        }
        finally
        {
            Console.WriteLine($"  Усього викликів LoadProductNames: {fileProcessor.CallCount}");
        }
    }

    private static void CreateProductsFile(string path)
    {
        try
        {
            File.WriteAllLines(path, new[]
            {
                "Молоко 2.5%",
                "Хліб житній",
                "Сир твердий",
                "Яблука Голден",
                "Кава мелена",
                "Чай зелений",
            });
        }
        catch (IOException ex)
        {
            Console.WriteLine($"Не вдалося створити тестовий файл: {ex.Message}");
            throw;
        }
    }

    private static void PrintHeader(string title)
    {
        Console.WriteLine();
        Console.WriteLine(new string('=', 80));
        Console.WriteLine($" {title}");
        Console.WriteLine(new string('=', 80));
    }

    private static void PrintList(string title, List<string> items)
    {
        Console.WriteLine($"\n  {title} ({items.Count}):");
        for (int i = 0; i < items.Count; i++)
            Console.WriteLine($"    {i + 1}. {items[i]}");
    }

    private static void PrintError(Exception ex)
    {
        ConsoleColor previous = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine($"\n  Операцію не виконано: {ex.GetType().Name} — {ex.Message}");
        Console.ForegroundColor = previous;
    }
}
