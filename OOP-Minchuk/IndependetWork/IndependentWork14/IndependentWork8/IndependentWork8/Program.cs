using IndependentWork8;

Console.OutputEncoding = System.Text.Encoding.UTF8;

DemoFileMetadataCache();
Console.WriteLine();
DemoTagManager();
Console.WriteLine();
DemoSearchIndex();

static void DemoFileMetadataCache()
{
    Console.WriteLine("=== Сценарій 1: FileMetadataCache (Dictionary) ===");

    var cache = new FileMetadataCache();

    cache.Add(new FileMetadata
    {
        FileName = "report.docx",
        Size = 48_213,
        LastModified = new DateTime(2026, 11, 10, 14, 30, 0),
        Hash = "a3f9c1"
    });
    cache.Add(new FileMetadata
    {
        FileName = "photo.png",
        Size = 1_204_551,
        LastModified = new DateTime(2026, 11, 11, 9, 15, 0),
        Hash = "7be402"
    });
    cache.Add(new FileMetadata
    {
        FileName = "notes.txt",
        Size = 932,
        LastModified = new DateTime(2026, 11, 12, 8, 0, 0),
        Hash = "c0ffee"
    });

    Console.WriteLine($"Додано файлів: {cache.Count}");
    Console.WriteLine($"Get(\"photo.png\")   -> {cache.Get("photo.png")}");
    Console.WriteLine($"Get(\"REPORT.DOCX\") -> {cache.Get("REPORT.DOCX")}");
    Console.WriteLine($"Contains(\"notes.txt\") -> {cache.Contains("notes.txt")}");

    cache.Add(new FileMetadata
    {
        FileName = "notes.txt",
        Size = 1_450,
        LastModified = new DateTime(2026, 11, 12, 15, 40, 0),
        Hash = "d00d42"
    });
    Console.WriteLine($"Після оновлення notes.txt -> {cache.Get("notes.txt")}");

    Console.WriteLine($"Remove(\"photo.png\") -> {cache.Remove("photo.png")}");
    Console.WriteLine($"Contains(\"photo.png\") -> {cache.Contains("photo.png")}");
    Console.WriteLine($"Кількість файлів у кеші: {cache.Count}");

    try
    {
        cache.Get("photo.png");
    }
    catch (KeyNotFoundException ex)
    {
        Console.WriteLine($"KeyNotFoundException: {ex.Message}");
    }
}

static void DemoTagManager()
{
    Console.WriteLine("=== Сценарій 2: TagManager (HashSet) ===");

    var manager = new TagManager();

    manager.AddTag("post-1", "csharp");
    manager.AddTag("post-1", "dotnet");
    manager.AddTag("post-1", "collections");
    manager.AddTag("post-1", "CSharp");

    manager.AddTag("post-2", "dotnet");
    manager.AddTag("post-2", "csharp");
    manager.AddTag("post-2", "linq");

    manager.AddTag("post-3", "unity");
    manager.AddTag("post-3", "gamedev");
    manager.AddTag("post-3", "csharp");

    Console.WriteLine($"Теги post-1: {Format(manager.GetTags("post-1"))}");
    Console.WriteLine($"Теги post-2: {Format(manager.GetTags("post-2"))}");
    Console.WriteLine($"Теги post-3: {Format(manager.GetTags("post-3"))}");
    Console.WriteLine($"Усі унікальні теги (UnionWith): {Format(manager.GetAllUniqueTags())}");
    Console.WriteLine($"Спільні теги post-1 і post-2 (IntersectWith): {Format(manager.FindCommonTags("post-1", "post-2"))}");
    Console.WriteLine($"Є в post-1, але немає в post-2 (ExceptWith): {Format(manager.FindDifferentTags("post-1", "post-2"))}");

    manager.RemoveTag("post-1", "collections");
    Console.WriteLine($"Після RemoveTag(\"post-1\", \"collections\"): {Format(manager.GetTags("post-1"))}");
}

static void DemoSearchIndex()
{
    Console.WriteLine("=== Сценарій 3: SearchIndex (Dictionary + HashSet) ===");

    var index = new SearchIndex();

    string[] documents =
    {
        "Dictionary зберігає пари ключ значення",
        "HashSet зберігає лише унікальні значення",
        "Dictionary і HashSet використовують хеш таблицю",
        "List зберігає елементи за індексом"
    };

    for (int i = 0; i < documents.Length; i++)
    {
        index.AddDocument(i + 1, documents[i]);
        Console.WriteLine($"Документ {i + 1}: {documents[i]}");
    }

    Console.WriteLine($"Search(\"зберігає\") -> {Format(index.Search("зберігає"))}");
    Console.WriteLine($"Search(\"hashset\") -> {Format(index.Search("hashset"))}");
    Console.WriteLine($"Search(\"масив\") -> {Format(index.Search("масив"))}");
    Console.WriteLine($"SearchMultiple(\"зберігає\", \"значення\") -> {Format(index.SearchMultiple("зберігає", "значення"))}");
    Console.WriteLine($"SearchMultiple(\"Dictionary\", \"HashSet\") -> {Format(index.SearchMultiple("Dictionary", "HashSet"))}");
}

static string Format<T>(IEnumerable<T> items)
{
    List<T> list = items.OrderBy(item => item).ToList();
    return list.Count == 0 ? "{ }" : "{ " + string.Join(", ", list) + " }";
}
