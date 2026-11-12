using System.Diagnostics;
using IndependentWork19.Services;
using Polly;
using Polly.CircuitBreaker;

namespace IndependentWork19.Scenarios;

public static class CircuitBreakerScenario
{
    public static ResiliencePipeline CreatePipeline(CircuitBreakerStateProvider stateProvider) =>
        new ResiliencePipelineBuilder()
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions
            {
                ShouldHandle = new PredicateBuilder().Handle<PaymentGatewayException>(),
                FailureRatio = 0.5,
                MinimumThroughput = 4,
                SamplingDuration = TimeSpan.FromSeconds(10),
                BreakDuration = TimeSpan.FromSeconds(1.5),
                StateProvider = stateProvider,
                OnOpened = args =>
                {
                    Log.Polly($"Circuit OPEN: забагато помилок ({args.Outcome.Exception?.Message}). " +
                              $"Запити блокуються на {args.BreakDuration.TotalSeconds:F1} с.");
                    return default;
                },
                OnHalfOpened = _ =>
                {
                    Log.Polly("Circuit HALF-OPEN: пропускаємо пробний запит, щоб перевірити, чи шлюз відновився.");
                    return default;
                },
                OnClosed = _ =>
                {
                    Log.Polly("Circuit CLOSED: шлюз відновився, запити знову проходять.");
                    return default;
                }
            })
            .Build();

    public static async Task RunAsync()
    {
        Console.WriteLine("=== Сценарій 2: оплата через платіжний шлюз — Circuit Breaker ===");
        Log.Restart();

        var gateway = new PaymentGateway();
        var stateProvider = new CircuitBreakerStateProvider();
        ResiliencePipeline pipeline = CreatePipeline(stateProvider);
        int attempts = 0;

        Console.WriteLine("--- 2a. Шлюз працює ---");
        for (int order = 1; order <= 2; order++)
        {
            attempts++;
            await PayAsync(pipeline, gateway, stateProvider, order);
        }

        Console.WriteLine("--- 2b. Шлюз впав ---");
        gateway.IsDown = true;
        for (int order = 3; order <= 9; order++)
        {
            attempts++;
            await PayAsync(pipeline, gateway, stateProvider, order);
        }

        Log.Info($"Спроб оплати: {attempts}, реальних звернень до шлюзу: {gateway.RealCalls}.");

        Console.WriteLine("--- 2c. Шлюз відновився, чекаємо завершення BreakDuration ---");
        gateway.IsDown = false;
        await Task.Delay(TimeSpan.FromSeconds(1.6));

        for (int order = 10; order <= 12; order++)
        {
            attempts++;
            await PayAsync(pipeline, gateway, stateProvider, order);
        }

        Log.Info($"Разом спроб оплати: {attempts}, реальних звернень до шлюзу: {gateway.RealCalls}.");
        Console.WriteLine();
    }

    private static async Task PayAsync(ResiliencePipeline pipeline, PaymentGateway gateway, CircuitBreakerStateProvider state, int orderId)
    {
        decimal amount = 500m + orderId * 125m;
        var stopwatch = Stopwatch.StartNew();

        try
        {
            string transaction = await pipeline.ExecuteAsync(
                async token => await gateway.ChargeAsync(orderId, amount, token),
                CancellationToken.None);

            Log.Success($"Замовлення #{orderId}: оплачено {amount} грн, {transaction} [{state.CircuitState}, {stopwatch.ElapsedMilliseconds} мс]");
        }
        catch (BrokenCircuitException)
        {
            Log.Warn($"Замовлення #{orderId}: відхилено миттєво — BrokenCircuitException, шлюз не викликався. " +
                     $"Замовлення поставлено в чергу на повторну оплату [{state.CircuitState}, {stopwatch.ElapsedMilliseconds} мс]");
        }
        catch (PaymentGatewayException ex)
        {
            Log.Error($"Замовлення #{orderId}: {ex.Message} [{state.CircuitState}, {stopwatch.ElapsedMilliseconds} мс]");
        }
    }
}
