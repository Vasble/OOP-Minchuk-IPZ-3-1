namespace lab18v11.Services;

public class FileProcessor
{
    private readonly int _transientFailures;
    private readonly Func<string, Exception> _errorFactory;
    private int _callCount;

    public FileProcessor(int transientFailures = 2, Func<string, Exception>? errorFactory = null)
    {
        _transientFailures = transientFailures;
        _errorFactory = errorFactory
            ?? (path => new FileNotFoundException("Файл тимчасово недоступний (імітація).", path));
    }

    public int CallCount => _callCount;

    public List<string> LoadProductNames(string path)
    {
        _callCount++;

        if (_callCount <= _transientFailures)
            throw _errorFactory(path);

        return File.ReadAllLines(path)
            .Select(line => line.Trim())
            .Where(line => line.Length > 0)
            .ToList();
    }
}
