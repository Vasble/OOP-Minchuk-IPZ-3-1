using System.Diagnostics;

namespace InvertedIndexDemo;

public record PerformanceResult(
    int DocumentCount,
    string Word,
    int LinearFound,
    int IndexFound,
    double LinearMicroseconds,
    double IndexMicroseconds,
    double BuildMilliseconds)
{
    public double Speedup => IndexMicroseconds > 0 ? LinearMicroseconds / IndexMicroseconds : double.PositiveInfinity;
}

public static class PerformanceTest
{
    public static PerformanceResult CompareSearchMethods(List<Document> documents, string searchWord, int iterations = 200)
    {
        string normalizedWord = TextProcessor.Normalize(searchWord).First();

        List<Document> LinearSearch() => documents
            .Where(d => TextProcessor.Normalize(d.Content).Contains(normalizedWord))
            .ToList();

        var buildWatch = Stopwatch.StartNew();
        var index = new InvertedIndex();
        foreach (Document doc in documents)
        {
            index.AddDocument(doc);
        }
        buildWatch.Stop();

        List<Document> linearResults = LinearSearch();
        List<Document> indexResults = index.SearchSingleWord(searchWord);

        int linearIterations = Math.Max(5, iterations * 10 / Math.Max(10, documents.Count));
        var linearWatch = Stopwatch.StartNew();
        for (int i = 0; i < linearIterations; i++)
        {
            linearResults = LinearSearch();
        }
        linearWatch.Stop();

        var indexWatch = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            indexResults = index.SearchSingleWord(searchWord);
        }
        indexWatch.Stop();

        return new PerformanceResult(
            documents.Count,
            searchWord,
            linearResults.Count,
            indexResults.Count,
            linearWatch.Elapsed.TotalMicroseconds / linearIterations,
            indexWatch.Elapsed.TotalMicroseconds / iterations,
            buildWatch.Elapsed.TotalMilliseconds);
    }

    public static void RunBenchmark(int[] sizes, string[] words)
    {
        Console.WriteLine("=== Вимірювання продуктивності: лінійний пошук vs Inverted Index ===");
        Console.WriteLine("Нормалізація однакова для обох підходів: TextProcessor.Normalize (нижній регістр, без пунктуації, без стоп-слів).");
        Console.WriteLine("Час пошуку — середнє за багато повторів після прогріву.");
        Console.WriteLine();
        Console.WriteLine($"{"Документів",10} | {"Слово",-12} | {"Знайдено (лін/інд)",18} | {"Лінійний, мкс",14} | {"Індекс, мкс",12} | {"Прискорення",12} | {"Побудова індексу, мс",21}");
        Console.WriteLine(new string('-', 118));

        CompareSearchMethods(DocumentGenerator.Generate(100), words[0]);

        foreach (int size in sizes)
        {
            List<Document> documents = size == 5
                ? DocumentGenerator.CreateSampleDocuments()
                : DocumentGenerator.Generate(size);

            foreach (string word in words)
            {
                PerformanceResult r = CompareSearchMethods(documents, word);
                string found = $"{r.LinearFound} / {r.IndexFound}";
                string check = r.LinearFound == r.IndexFound ? "" : "  ПОМИЛКА: результати різні!";
                Console.WriteLine($"{r.DocumentCount,10} | {r.Word,-12} | {found,18} | {r.LinearMicroseconds,14:F2} | {r.IndexMicroseconds,12:F3} | {r.Speedup,11:F0}x | {r.BuildMilliseconds,21:F2}{check}");
            }
        }
    }
}
