using IndependentWork19.Services;
using Polly;
using Polly.Retry;

namespace IndependentWork19.Scenarios;

public static class RetryScenario
{
    public static ResiliencePipeline CreatePipeline() =>
        new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                ShouldHandle = new PredicateBuilder()
                    .Handle<HttpRequestException>()
                    .Handle<TimeoutException>(),
                MaxRetryAttempts = 4,
                Delay = TimeSpan.FromMilliseconds(200),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                OnRetry = args =>
                {
                    Log.Polly($"Retry #{args.AttemptNumber + 1}: {args.Outcome.Exception?.GetType().Name} — " +
                              $"{args.Outcome.Exception?.Message} Чекаємо {args.RetryDelay.TotalMilliseconds:F0} мс...");
                    return default;
                }
            })
            .Build();

    public static async Task RunAsync()
    {
        Console.WriteLine("=== Сценарій 1: курс валют з API НБУ — Retry з експоненційною затримкою та jitter ===");
        Log.Restart();

        ResiliencePipeline pipeline = CreatePipeline();

        Console.WriteLine("--- 1a. API тричі збоїть, потім відповідає ---");
        var flakyApi = new ExchangeRateApi(failuresBeforeSuccess: 3);
        await CallAsync(pipeline, flakyApi, "USD");

        Console.WriteLine("--- 1b. Помилка у вхідних даних — повторювати немає сенсу ---");
        var api = new ExchangeRateApi(failuresBeforeSuccess: 0);
        await CallAsync(pipeline, api, "US");

        Console.WriteLine("--- 1c. API лежить довго — спроби вичерпано ---");
        var deadApi = new ExchangeRateApi(failuresBeforeSuccess: int.MaxValue);
        await CallAsync(pipeline, deadApi, "EUR");

        Console.WriteLine();
    }

    private static async Task CallAsync(ResiliencePipeline pipeline, ExchangeRateApi api, string currency)
    {
        Log.Info($"Запит курсу {currency}...");

        try
        {
            decimal rate = await pipeline.ExecuteAsync(
                async token => await api.GetRateAsync(currency, token),
                CancellationToken.None);

            Log.Success($"Курс {currency} = {rate} грн. Викликів API: {api.Calls}");
        }
        catch (ArgumentException ex)
        {
            Log.Error($"Не повторюємо (ArgumentException не входить до ShouldHandle): {ex.Message} Викликів API: {api.Calls}");
        }
        catch (Exception ex) when (ex is HttpRequestException or TimeoutException)
        {
            Log.Error($"Остаточна невдача після {api.Calls} спроб: {ex.GetType().Name} — {ex.Message} Показуємо останній збережений курс.");
        }
    }
}
