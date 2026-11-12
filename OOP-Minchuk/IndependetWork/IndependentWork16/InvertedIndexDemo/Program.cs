using System.Text.RegularExpressions;
using InvertedIndexDemo;

Console.OutputEncoding = System.Text.Encoding.UTF8;

bool runBenchmark = !args.Contains("--no-benchmark");
bool runInteractive = !args.Contains("--no-interactive");

var index = new InvertedIndex();
LoadDocuments(index);

Console.WriteLine($"Loaded {index.DocumentCount} documents");
Console.WriteLine($"Index contains {index.UniqueWords} unique words");
Console.WriteLine();

RunDemoQueries(index);

if (runBenchmark)
{
    PerformanceTest.RunBenchmark(new[] { 10, 100, 1000, 10000 }, new[] { "python", "await", "compiler", "rust" });
    Console.WriteLine();
}

if (runInteractive)
{
    RunInteractive(index);
}

static void LoadDocuments(InvertedIndex index)
{
    foreach (Document doc in DocumentGenerator.CreateSampleDocuments())
    {
        index.AddDocument(doc);
    }
}

static void RunDemoQueries(InvertedIndex index)
{
    Console.WriteLine("=== Демонстрація методів пошуку ===");

    PrintIds("SearchSingleWord(\"C#\")", index.SearchSingleWord("C#"));
    PrintIds("SearchSingleWord(\"Language\")", index.SearchSingleWord("Language"));
    PrintIds("SearchSingleWord(\"the\") — стоп-слово", index.SearchSingleWord("the"));
    PrintIds("SearchMultipleWords(\"programming\", \"language\") — AND", index.SearchMultipleWords("programming", "language"));
    PrintIds("SearchMultipleWords(\"c#\", \"modern\") — AND", index.SearchMultipleWords("c#", "modern"));
    PrintIds("SearchAnyWord(\"java\", \"python\") — OR", index.SearchAnyWord("java", "python"));
    PrintIds("SearchAnyWord(\"linq\", \"syntax\", \"rust\") — OR", index.SearchAnyWord("linq", "syntax", "rust"));
    PrintIds("SearchPhrase(\"programming language\")", index.SearchPhrase("programming language"));
    PrintIds("SearchPhrase(\"language programming\")", index.SearchPhrase("language programming"));

    Console.WriteLine("SearchRanked(\"c#\", \"modern\", \"programming\") — TF-IDF:");
    foreach ((Document doc, double score) in index.SearchRanked("c#", "modern", "programming"))
    {
        Console.WriteLine($"  {score:F3}  {doc}");
    }

    Console.WriteLine();
}

static void PrintIds(string label, List<Document> documents)
{
    string ids = documents.Count == 0 ? "—" : string.Join(", ", documents.Select(d => d.Id));
    Console.WriteLine($"{label,-58} -> [{ids}]");
}

static void RunInteractive(InvertedIndex index)
{
    Console.WriteLine("=== Інтерактивний пошук ===");
    Console.WriteLine("Формат запиту: слова через пробіл — AND; or: слова — OR; \"фраза\" — точна фраза; rank: слова — ранжування TF-IDF.");
    Console.WriteLine();

    while (true)
    {
        Console.Write("Enter search query (or 'exit'): ");
        string? query = Console.ReadLine();

        if (query is null)
        {
            Console.WriteLine();
            break;
        }

        if (Console.IsInputRedirected)
        {
            Console.WriteLine(query);
        }

        query = query.Trim();

        if (query.Equals("exit", StringComparison.OrdinalIgnoreCase))
        {
            break;
        }

        if (query.Length == 0)
        {
            continue;
        }

        ExecuteQuery(index, query);
    }
}

static void ExecuteQuery(InvertedIndex index, string query)
{
    string mode;
    List<Document> results;
    Dictionary<int, double>? scores = null;
    string termsText;

    if (query.StartsWith("or:", StringComparison.OrdinalIgnoreCase))
    {
        termsText = query[3..];
        mode = "OR";
        results = index.SearchAnyWord(SplitWords(termsText));
    }
    else if (query.StartsWith("rank:", StringComparison.OrdinalIgnoreCase))
    {
        termsText = query[5..];
        mode = "RANKED, TF-IDF";
        var ranked = index.SearchRanked(SplitWords(termsText));
        results = ranked.Select(r => r.Document).ToList();
        scores = ranked.ToDictionary(r => r.Document.Id, r => r.Score);
    }
    else if (query.Length > 1 && query.StartsWith('"') && query.EndsWith('"'))
    {
        termsText = query[1..^1];
        mode = "PHRASE";
        results = index.SearchPhrase(termsText);
    }
    else
    {
        termsText = query;
        mode = "AND";
        results = index.SearchMultipleWords(SplitWords(termsText));
    }

    HashSet<string> highlightTerms = TextProcessor.Tokenize(termsText).ToHashSet();

    Console.WriteLine($"\n--- Search Results ({mode}) ---");

    foreach (Document doc in results)
    {
        string scoreText = scores is null ? "" : $"  (score {scores[doc.Id]:F3})";
        Console.WriteLine($"{doc}{scoreText}");
        string snippet = doc.Content.Length > 100 ? doc.Content[..100] + "..." : doc.Content;
        Console.WriteLine($"  {Highlight(snippet, highlightTerms)}");
    }

    Console.WriteLine($"\nFound {results.Count} documents\n");
}

static string[] SplitWords(string text) =>
    text.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

static string Highlight(string text, HashSet<string> terms) =>
    Regex.Replace(text, @"\w+(?:[#+]+)?", match =>
        terms.Contains(match.Value.ToLowerInvariant()) ? $"[{match.Value}]" : match.Value);
