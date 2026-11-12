using System.Text.RegularExpressions;

namespace InvertedIndexDemo;

public static partial class TextProcessor
{
    private static readonly HashSet<string> StopWords = new()
    {
        "a", "an", "the", "is", "are", "was", "were",
        "in", "on", "at", "to", "for", "of", "and", "or"
    };

    [GeneratedRegex(@"\w+(?:[#+]+)?", RegexOptions.CultureInvariant)]
    private static partial Regex WordRegex();

    public static IEnumerable<string> Tokenize(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            yield break;
        }

        foreach (Match match in WordRegex().Matches(text.ToLowerInvariant()))
        {
            yield return match.Value;
        }
    }

    public static IEnumerable<string> RemoveStopWords(IEnumerable<string> words) =>
        words.Where(w => !StopWords.Contains(w));

    public static IEnumerable<string> Normalize(string text) =>
        RemoveStopWords(Tokenize(text));

    public static bool IsStopWord(string word) => StopWords.Contains(word);
}
