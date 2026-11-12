namespace InvertedIndexDemo;

public class InvertedIndex
{
    private readonly Dictionary<string, HashSet<int>> _index;
    private readonly Dictionary<int, Document> _documents;
    private readonly Dictionary<int, Dictionary<string, int>> _termFrequencies;

    public InvertedIndex()
    {
        _index = new Dictionary<string, HashSet<int>>();
        _documents = new Dictionary<int, Document>();
        _termFrequencies = new Dictionary<int, Dictionary<string, int>>();
    }

    public int DocumentCount => _documents.Count;

    public int UniqueWords => _index.Keys.Count;

    public void AddDocument(Document doc)
    {
        ArgumentNullException.ThrowIfNull(doc);

        if (_documents.ContainsKey(doc.Id))
        {
            throw new ArgumentException($"Документ з Id={doc.Id} уже є в індексі.", nameof(doc));
        }

        _documents[doc.Id] = doc;
        var frequencies = new Dictionary<string, int>();

        foreach (string word in TextProcessor.Normalize(doc.Content))
        {
            if (!_index.TryGetValue(word, out HashSet<int>? ids))
            {
                ids = new HashSet<int>();
                _index[word] = ids;
            }

            ids.Add(doc.Id);
            frequencies[word] = frequencies.GetValueOrDefault(word) + 1;
        }

        _termFrequencies[doc.Id] = frequencies;
    }

    public List<Document> SearchSingleWord(string word)
    {
        string? normalized = TextProcessor.Normalize(word).FirstOrDefault();

        if (normalized is null || !_index.TryGetValue(normalized, out HashSet<int>? ids))
        {
            return new List<Document>();
        }

        return ToDocuments(ids);
    }

    public List<Document> SearchMultipleWords(params string[] words)
    {
        List<string> terms = NormalizeQuery(words);

        if (terms.Count == 0)
        {
            return new List<Document>();
        }

        var sets = new List<HashSet<int>>();

        foreach (string term in terms)
        {
            if (!_index.TryGetValue(term, out HashSet<int>? ids))
            {
                return new List<Document>();
            }

            sets.Add(ids);
        }

        sets.Sort((a, b) => a.Count.CompareTo(b.Count));
        var result = new HashSet<int>(sets[0]);

        for (int i = 1; i < sets.Count && result.Count > 0; i++)
        {
            result.IntersectWith(sets[i]);
        }

        return ToDocuments(result);
    }

    public List<Document> SearchAnyWord(params string[] words)
    {
        var result = new HashSet<int>();

        foreach (string term in NormalizeQuery(words))
        {
            if (_index.TryGetValue(term, out HashSet<int>? ids))
            {
                result.UnionWith(ids);
            }
        }

        return ToDocuments(result);
    }

    public List<Document> SearchPhrase(string phrase)
    {
        List<string> phraseTokens = TextProcessor.Tokenize(phrase).ToList();

        if (phraseTokens.Count == 0)
        {
            return new List<Document>();
        }

        List<Document> candidates = SearchMultipleWords(phraseTokens.ToArray());

        if (candidates.Count == 0 && phraseTokens.All(TextProcessor.IsStopWord))
        {
            candidates = _documents.Values.ToList();
        }

        return candidates
            .Where(doc => ContainsSequence(TextProcessor.Tokenize(doc.Content).ToList(), phraseTokens))
            .OrderBy(doc => doc.Id)
            .ToList();
    }

    public List<(Document Document, double Score)> SearchRanked(params string[] words)
    {
        List<string> terms = NormalizeQuery(words);
        var scores = new Dictionary<int, double>();

        foreach (string term in terms.Distinct())
        {
            if (!_index.TryGetValue(term, out HashSet<int>? ids))
            {
                continue;
            }

            double idf = Math.Log((double)(DocumentCount + 1) / (ids.Count + 1)) + 1;

            foreach (int id in ids)
            {
                Dictionary<string, int> frequencies = _termFrequencies[id];
                int totalTerms = frequencies.Values.Sum();
                double tf = (double)frequencies[term] / totalTerms;
                scores[id] = scores.GetValueOrDefault(id) + tf * idf;
            }
        }

        return scores
            .OrderByDescending(pair => pair.Value)
            .ThenBy(pair => pair.Key)
            .Select(pair => (_documents[pair.Key], pair.Value))
            .ToList();
    }

    public IEnumerable<string> GetTerms() => _index.Keys;

    private static List<string> NormalizeQuery(IEnumerable<string> words) =>
        words.SelectMany(TextProcessor.Normalize).ToList();

    private List<Document> ToDocuments(IEnumerable<int> ids) =>
        ids.OrderBy(id => id).Select(id => _documents[id]).ToList();

    private static bool ContainsSequence(List<string> tokens, List<string> sequence)
    {
        for (int start = 0; start <= tokens.Count - sequence.Count; start++)
        {
            bool match = true;

            for (int offset = 0; offset < sequence.Count; offset++)
            {
                if (tokens[start + offset] != sequence[offset])
                {
                    match = false;
                    break;
                }
            }

            if (match)
            {
                return true;
            }
        }

        return false;
    }
}
