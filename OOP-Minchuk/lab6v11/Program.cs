using System.Text;

Console.OutputEncoding = Encoding.UTF8;

Console.WriteLine("=== Інтерфейс IComparer<string> ===");

var comparers = new List<Lab6v11.IComparer<string>>
{
    new Lab6v11.StringLengthComparer(),
    new Lab6v11.AlphabeticalComparer()
};

var words = new[] { "banana", "kiwi", "Apple", "fig", "cherry" };

foreach (var comparer in comparers)
{
    Console.WriteLine($"\n{comparer.GetType().Name}:");
    Console.WriteLine($"  Compare(\"kiwi\", \"banana\") = {comparer.Compare("kiwi", "banana")}");
    Console.WriteLine($"  Compare(\"Apple\", \"fig\")   = {comparer.Compare("Apple", "fig")}");
    Console.WriteLine($"  Compare(\"fig\", \"fig\")     = {comparer.Compare("fig", "fig")}");

    var sorted = words.ToList();
    var c = comparer;
    sorted.Sort((a, b) => c.Compare(a, b));
    Console.WriteLine($"  Відсортовано: {string.Join(", ", sorted)}");
}

Console.WriteLine("\n=== Абстрактний клас FileProcessor ===");

string dir = Path.Combine(Path.GetTempPath(), "lab6v11");
Directory.CreateDirectory(dir);

string logIn = Path.Combine(dir, "app.log");
string cfgIn = Path.Combine(dir, "app.cfg");

File.WriteAllLines(logIn, new[]
{
    "2026-10-10 10:05:00 INFO  Запуск застосунку",
    "2026-10-10 10:07:12 ERROR Не вдалося підключитися до БД",
    "2026-10-10 10:06:30 WARN  Повільна відповідь сервера",
    "2026-10-10 10:08:01 INFO  Повторне підключення"
});

File.WriteAllLines(cfgIn, new[]
{
    "# Налаштування",
    "Host = localhost",
    "",
    "PORT=8080",
    "  Mode =  Debug  ",
    "некоректний рядок"
});

var processors = new List<(Lab6v11.FileProcessor Processor, string Input, string Output)>
{
    (new Lab6v11.LogFileProcessor(),    logIn, Path.Combine(dir, "errors.log")),
    (new Lab6v11.ConfigFileProcessor(), cfgIn, Path.Combine(dir, "clean.cfg"))
};

foreach (var (processor, input, output) in processors)
{
    Console.WriteLine($"\n{processor.GetType().Name}:");
    processor.Run(input, output);
    Console.WriteLine("  Вміст результату:");
    foreach (var line in File.ReadAllLines(output))
        Console.WriteLine($"    {line}");
}

namespace Lab6v11
{
    public interface IComparer<in T>
    {
        int Compare(T x, T y);
    }

    public class StringLengthComparer : IComparer<string>
    {
        public int Compare(string x, string y) => x.Length.CompareTo(y.Length);
    }

    public class AlphabeticalComparer : IComparer<string>
    {
        public int Compare(string x, string y) =>
            string.Compare(x, y, StringComparison.OrdinalIgnoreCase);
    }

    public abstract class FileProcessor
    {
        protected List<string> Content = new();

        public abstract void ReadFile(string path);
        public abstract void ProcessContent();

        public void WriteResult(string path)
        {
            File.WriteAllLines(path, Content);
            Console.WriteLine($"  [{GetType().Name}] Результат ({Content.Count} рядк.) записано у '{path}'");
        }

        public void Run(string inputPath, string outputPath)
        {
            ReadFile(inputPath);
            ProcessContent();
            WriteResult(outputPath);
        }
    }

    public class LogFileProcessor : FileProcessor
    {
        public override void ReadFile(string path)
        {
            Content = File.ReadAllLines(path).ToList();
            Console.WriteLine($"  [LogFileProcessor] Прочитано {Content.Count} рядків логу");
        }

        public override void ProcessContent()
        {
            Content = Content
                .Where(l => l.Contains("ERROR") || l.Contains("WARN"))
                .OrderBy(l => l, StringComparer.Ordinal)
                .ToList();
            Console.WriteLine($"  [LogFileProcessor] Відфільтровано помилки/попередження: {Content.Count}");
        }
    }

    public class ConfigFileProcessor : FileProcessor
    {
        public override void ReadFile(string path)
        {
            Content = File.ReadAllLines(path).ToList();
            Console.WriteLine($"  [ConfigFileProcessor] Прочитано {Content.Count} рядків конфігурації");
        }

        public override void ProcessContent()
        {
            Content = Content
                .Select(l => l.Trim())
                .Where(l => l.Length > 0 && !l.StartsWith('#') && l.Contains('='))
                .Select(l =>
                {
                    var parts = l.Split('=', 2);
                    return $"{parts[0].Trim().ToLowerInvariant()}={parts[1].Trim()}";
                })
                .ToList();
            Console.WriteLine($"  [ConfigFileProcessor] Валідних параметрів: {Content.Count}");
        }
    }
}