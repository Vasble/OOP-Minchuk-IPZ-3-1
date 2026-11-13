namespace lab18v11.Helpers;

public static class RetryHelper
{
    private static readonly TimeSpan DefaultInitialDelay = TimeSpan.FromMilliseconds(300);

    public static T ExecuteWithRetry<T>(
        Func<T> operation,
        int retryCount = 3,
        TimeSpan initialDelay = default,
        Func<Exception, bool>? shouldRetry = null)
    {
        ArgumentNullException.ThrowIfNull(operation);
        if (retryCount < 0)
            throw new ArgumentOutOfRangeException(nameof(retryCount), "Кількість повторів не може бути від'ємною.");

        if (initialDelay <= TimeSpan.Zero)
            initialDelay = DefaultInitialDelay;

        int totalAttempts = retryCount + 1;

        for (int retryAttempt = 0; ; retryAttempt++)
        {
            int attemptNumber = retryAttempt + 1;
            Log($"Спроба {attemptNumber}/{totalAttempts}...", ConsoleColor.Gray);

            try
            {
                T result = operation();
                Log($"Спроба {attemptNumber}: успіх.", ConsoleColor.Green);
                return result;
            }
            catch (Exception ex)
            {
                Log($"Спроба {attemptNumber}: невдача — {ex.GetType().Name}: {ex.Message}", ConsoleColor.Red);

                bool retryAllowedForException = shouldRetry?.Invoke(ex) ?? true;
                if (!retryAllowedForException)
                {
                    Log($"shouldRetry повернув false для {ex.GetType().Name} — повтор не виконується.", ConsoleColor.Yellow);
                    throw; // зберігаємо оригінальний стек виклику
                }

                if (retryAttempt >= retryCount)
                {
                    Log($"Вичерпано всі {totalAttempts} спроби — операцію не виконано.", ConsoleColor.Yellow);
                    throw;
                }

                TimeSpan delay = TimeSpan.FromMilliseconds(initialDelay.TotalMilliseconds * Math.Pow(2, retryAttempt));
                Log($"Очікування {delay.TotalMilliseconds:F0} мс перед наступною спробою " +
                    $"({initialDelay.TotalMilliseconds:F0} * 2^{retryAttempt}).", ConsoleColor.DarkCyan);

                Thread.Sleep(delay);
            }
        }
    }

    private static void Log(string message, ConsoleColor color)
    {
        ConsoleColor previous = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine($"  [{DateTime.Now:HH:mm:ss.fff}] [Retry] {message}");
        Console.ForegroundColor = previous;
    }
}
