
public class ThreadPool : IDisposable
{
    private readonly int _poolSize;
    private bool _isActive;
    private bool _disposed = false;

    public ThreadPool(int poolSize)
    {
        if (poolSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(poolSize), "Розмір пулу має бути більшим за 0.");

        _poolSize = poolSize;
        _isActive = true; 
        Console.WriteLine($"[ThreadPool] Створено пул із {_poolSize} потоків. Ресурс виділено.");
    }

    public int PoolSize => _poolSize;
    public bool IsActive => _isActive;

    public void ExecuteTask(string taskName)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(ThreadPool));

        if (!_isActive)
        {
            Console.WriteLine($"[ThreadPool] Пул неактивний. Завдання \"{taskName}\" не виконано.");
            return;
        }

        Console.WriteLine($"[ThreadPool] Виконується завдання \"{taskName}\" (потоків у пулі: {_poolSize}).");
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            Console.WriteLine("[ThreadPool] Dispose: звільнення керованих ресурсів.");
        }

        if (_isActive)
        {
            Console.WriteLine($"[ThreadPool] Завершення всіх потоків ({_poolSize}). Некерований ресурс звільнено.");
            _isActive = false;
        }

        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~ThreadPool()
    {
        Console.WriteLine("[ThreadPool] Викликано деструктор.");
        Dispose(false);
    }
}