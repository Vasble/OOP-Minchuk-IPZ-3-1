namespace IndependentWork8;

public class SearchIndex
{
    private static readonly char[] Separators =
        { ' ', '\t', '\n', '\r', '.', ',', ';', ':', '!', '?', '(', ')', '"', '\'', '-', '—' };

    private readonly Dictionary<string, HashSet<int>> _invertedIndex =
        new(StringComparer.OrdinalIgnoreCase);

    public void AddDocument(int docId, string content)
    {
        string[] words = content.Split(Separators, StringSplitOptions.RemoveEmptyEntries);

        foreach (string word in words)
        {
            if (!_invertedIndex.TryGetValue(word, out HashSet<int>? documents))
            {
                documents = new HashSet<int>();
                _invertedIndex[word] = documents;
            }

            documents.Add(docId);
        }
    }

    public HashSet<int> Search(string word) =>
        _invertedIndex.TryGetValue(word, out HashSet<int>? documents)
            ? new HashSet<int>(documents)
            : new HashSet<int>();

    public HashSet<int> SearchMultiple(params string[] words)
    {
        if (words.Length == 0)
        {
            return new HashSet<int>();
        }

        List<HashSet<int>> sets = words
            .Select(Search)
            .OrderBy(set => set.Count)
            .ToList();

        HashSet<int> result = sets[0];

        for (int i = 1; i < sets.Count && result.Count > 0; i++)
        {
            result.IntersectWith(sets[i]);
        }

        return result;
    }
}
