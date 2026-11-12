namespace IndependentWork8;

public class FileMetadataCache
{
    private readonly Dictionary<string, FileMetadata> _cache;

    public FileMetadataCache()
    {
        _cache = new Dictionary<string, FileMetadata>(StringComparer.OrdinalIgnoreCase);
    }

    public int Count => _cache.Count;

    public void Add(FileMetadata metadata)
    {
        ArgumentNullException.ThrowIfNull(metadata);

        if (string.IsNullOrWhiteSpace(metadata.FileName))
        {
            throw new ArgumentException("Ім'я файлу не може бути порожнім.", nameof(metadata));
        }

        _cache[metadata.FileName] = metadata;
    }

    public FileMetadata Get(string fileName)
    {
        if (_cache.TryGetValue(fileName, out FileMetadata? metadata))
        {
            return metadata;
        }

        throw new KeyNotFoundException($"Метадані для файлу '{fileName}' не знайдено.");
    }

    public bool TryGet(string fileName, out FileMetadata? metadata) =>
        _cache.TryGetValue(fileName, out metadata);

    public bool Remove(string fileName) => _cache.Remove(fileName);

    public bool Contains(string fileName) => _cache.ContainsKey(fileName);
}
