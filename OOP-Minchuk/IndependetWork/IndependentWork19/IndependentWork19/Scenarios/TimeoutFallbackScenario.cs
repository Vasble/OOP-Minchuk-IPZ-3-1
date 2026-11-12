using System.Diagnostics;
using IndependentWork19.Services;
using Polly;
using Polly.Fallback;
using Polly.Retry;
using Polly.Timeout;

namespace IndependentWork19.Scenarios;

public static class TimeoutFallbackScenario
{
    private static readonly List<string> CachedBestSellers = new()
    {
        "Павербанк 20000 mAh (хіт продажів)",
        "USB-C кабель 2 м (хіт продажів)",
        "Килимок для мишки XL (хіт продажів)"
    };

    public static ResiliencePipeline<List<string>> CreatePipeline() =>
        new ResiliencePipelineBuilder<List<string>>()
            .AddFallback(new FallbackStrategyOptions<List<string>>
            {
                ShouldHandle = new PredicateBuilder<List<string>>()
                    .Handle<TimeoutRejectedException>()
                    .Handle<HttpRequestException>(),
                FallbackAction = _ => Outcome.FromResultAsValueTask(CachedBestSellers),
                OnFallback = args =>
                {
                    Log.Polly($"Fallback: сервіс недоступний ({args.Outcome.Exception?.GetType().Name}), " +
                              "повертаємо кешовані хіти продажів.");
                    return default;
                }
            })
            .AddRetry(new RetryStrategyOptions<List<string>>
            {
                ShouldHandle = new PredicateBuilder<List<string>>().Handle<TimeoutRejectedException>(),
                MaxRetryAttempts = 2,
                Delay = TimeSpan.FromMilliseconds(100),
                BackoffType = DelayBackoffType.Constant,
                OnRetry = args =>
                {
                    Log.Polly($"Retry #{args.AttemptNumber + 1} після тайм-ауту, чекаємо {args.RetryDelay.TotalMilliseconds:F0} мс.");
                    return default;
                }
            })
            .AddTimeout(new TimeoutStrategyOptions
            {
                Timeout = TimeSpan.FromMilliseconds(300),
                OnTimeout = args =>
                {
                    Log.Polly($"Timeout: сервіс не відповів за {args.Timeout.TotalMilliseconds:F0} мс, спробу скасовано.");
                    return default;
                }
            })
            .Build();

    public static async Task RunAsync()
    {
        Console.WriteLine("=== Сценарій 3: рекомендації на сторінці товару — Timeout + Retry + Fallback ===");
        Log.Restart();

        ResiliencePipeline<List<string>> pipeline = CreatePipeline();

        Console.WriteLine("--- 3a. Сервіс відповідає швидко ---");
        await ShowRecommendationsAsync(pipeline, new RecommendationService(120), customerId: 101);

        Console.WriteLine("--- 3b. Перша спроба зависла, друга вклалася в час ---");
        await ShowRecommendationsAsync(pipeline, new RecommendationService(2000, 150), customerId: 102);

        Console.WriteLine("--- 3c. Сервіс перевантажений: усі спроби завислі ---");
        await ShowRecommendationsAsync(pipeline, new RecommendationService(2000, 2000, 2000), customerId: 103);

        Console.WriteLine();
    }

    private static async Task ShowRecommendationsAsync(ResiliencePipeline<List<string>> pipeline, RecommendationService service, int customerId)
    {
        var stopwatch = Stopwatch.StartNew();

        List<string> items = await pipeline.ExecuteAsync(
            async token => await service.GetRecommendationsAsync(customerId, token),
            CancellationToken.None);

        Log.Success($"Клієнт {customerId}: {string.Join("; ", items)} " +
                    $"[викликів сервісу: {service.Calls}, загальний час: {stopwatch.ElapsedMilliseconds} мс]");
    }
}
