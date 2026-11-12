namespace InvertedIndexDemo;

public static class DocumentGenerator
{
    private static readonly string[] Vocabulary =
    {
        "algorithm", "array", "class", "compiler", "database", "debugger", "delegate", "framework",
        "generic", "interface", "iterator", "lambda", "library", "memory", "method", "module",
        "network", "object", "parser", "pattern", "pointer", "query", "queue", "runtime",
        "server", "stack", "string", "syntax", "thread", "variable", "vector", "web"
    };

    public static List<Document> CreateSampleDocuments() => new()
    {
        new Document
        {
            Id = 1,
            Title = "Introduction to C#",
            Content = "C# is a modern object-oriented programming language developed by Microsoft."
        },
        new Document
        {
            Id = 2,
            Title = "Java Basics",
            Content = "Java is a popular programming language known for its portability and performance."
        },
        new Document
        {
            Id = 3,
            Title = "Python Overview",
            Content = "Python is a high-level programming language with simple syntax."
        },
        new Document
        {
            Id = 4,
            Title = "C# Advanced Features",
            Content = "C# supports LINQ, async/await, and many modern programming paradigms."
        },
        new Document
        {
            Id = 5,
            Title = "Comparing Languages",
            Content = "C#, Java, and Python are among the most popular programming languages today."
        }
    };

    public static List<Document> Generate(int count, int seed = 42)
    {
        var random = new Random(seed);
        List<Document> samples = CreateSampleDocuments();
        var documents = new List<Document>(count);

        for (int i = 0; i < count; i++)
        {
            Document template = samples[i % samples.Count];
            int extraWords = random.Next(5, 16);
            var extra = new string[extraWords];

            for (int j = 0; j < extraWords; j++)
            {
                extra[j] = Vocabulary[random.Next(Vocabulary.Length)];
            }

            documents.Add(new Document
            {
                Id = i + 1,
                Title = $"{template.Title} #{i + 1}",
                Content = template.Content + " " + string.Join(' ', extra) + "."
            });
        }

        return documents;
    }
}
