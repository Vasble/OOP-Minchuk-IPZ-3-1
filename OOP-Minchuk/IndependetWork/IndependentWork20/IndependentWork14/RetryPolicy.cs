namespace IndependentWork14;

public class RetryPolicy
{
    private readonly int _maxRetries;
    private readonly TimeSpan _initialDelay;
    private readonly double _jitterFactor;
    private readonly Random _random;
    private readonly Action<TimeSpan> _sleep;

    public RetryPolicy(int maxRetries, TimeSpan initialDelay, double jitterFactor = 0.2)
        : this(maxRetries, initialDelay, jitterFactor, Random.Shared, Thread.Sleep)
    {
    }

    public RetryPolicy(int maxRetries, TimeSpan initialDelay, double jitterFactor, Random random, Action<TimeSpan> sleep)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(maxRetries);

        if (initialDelay < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(initialDelay), initialDelay, "Початкова затримка не може бути від'ємною.");
        }

        if (jitterFactor is < 0 or > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(jitterFactor), jitterFactor, "Коефіцієнт jitter має бути в межах [0; 1].");
        }

        _maxRetries = maxRetries;
        _initialDelay = initialDelay;
        _jitterFactor = jitterFactor;
        _random = random ?? throw new ArgumentNullException(nameof(random));
        _sleep = sleep ?? throw new ArgumentNullException(nameof(sleep));
    }

    public int MaxRetries => _maxRetries;

    public TimeSpan InitialDelay => _initialDelay;

    public double JitterFactor => _jitterFactor;

    public T Execute<T>(Func<T> operation, Func<Exception, bool>? shouldRetry = null)
    {
        ArgumentNullException.ThrowIfNull(operation);

        int totalAttempts = _maxRetries + 1;

        for (int attempt = 1; ; attempt++)
        {
            try
            {
                Console.WriteLine($"  [RetryPolicy] Спроба {attempt}/{totalAttempts}...");
                T result = operation();
                Console.WriteLine($"  [RetryPolicy] Спроба {attempt}: успіх.");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  [RetryPolicy] Спроба {attempt}: невдача — {ex.GetType().Name}: {ex.Message}");

                if (shouldRetry is not null && !shouldRetry(ex))
                {
                    Console.WriteLine("  [RetryPolicy] shouldRetry повернув false — виняток не тимчасовий, повтор не виконується.");
                    throw;
                }

                if (attempt > _maxRetries)
                {
                    Console.WriteLine($"  [RetryPolicy] Вичерпано всі {_maxRetries} повторні спроби — виняток передається далі.");
                    throw;
                }

                int retryAttempt = attempt - 1;
                TimeSpan delay = CalculateDelay(retryAttempt);
                Console.WriteLine($"  [RetryPolicy] Повтор #{attempt} через {delay.TotalMilliseconds:F0} мс " +
                                  $"(база {_initialDelay.TotalMilliseconds * Math.Pow(2, retryAttempt):F0} мс ± {_jitterFactor:P0}).");
                _sleep(delay);
            }
        }
    }

    public void Execute(Action operation, Func<Exception, bool>? shouldRetry = null)
    {
        ArgumentNullException.ThrowIfNull(operation);

        Execute<object?>(() =>
        {
            operation();
            return null;
        }, shouldRetry);
    }

    public TimeSpan CalculateDelay(int retryAttempt)
    {
        double jitterMultiplier = 1 + _random.NextDouble() * _jitterFactor * 2 - _jitterFactor;
        double milliseconds = _initialDelay.TotalMilliseconds * Math.Pow(2, retryAttempt) * jitterMultiplier;
        return TimeSpan.FromMilliseconds(milliseconds);
    }
}
