namespace IndependentWork19.Services;

public class ExchangeRateApi
{
    private readonly int _failuresBeforeSuccess;
    private int _calls;

    public ExchangeRateApi(int failuresBeforeSuccess)
    {
        _failuresBeforeSuccess = failuresBeforeSuccess;
    }

    public int Calls => _calls;

    public async ValueTask<decimal> GetRateAsync(string currency, CancellationToken token)
    {
        _calls++;
        await Task.Delay(50, token);

        if (currency.Length != 3)
        {
            throw new ArgumentException($"Некоректний код валюти '{currency}'.", nameof(currency));
        }

        if (_calls <= _failuresBeforeSuccess)
        {
            if (_calls % 2 == 0)
            {
                throw new TimeoutException("API НБУ не відповів вчасно.");
            }

            throw new HttpRequestException("503 Service Unavailable — API НБУ тимчасово недоступний.");
        }

        return currency.ToUpperInvariant() switch
        {
            "USD" => 41.85m,
            "EUR" => 48.62m,
            "PLN" => 11.43m,
            _ => 1m
        };
    }
}

public class PaymentGatewayException : Exception
{
    public PaymentGatewayException(string message)
        : base(message)
    {
    }
}

public class PaymentGateway
{
    public bool IsDown { get; set; }

    public int RealCalls { get; private set; }

    public async ValueTask<string> ChargeAsync(int orderId, decimal amount, CancellationToken token)
    {
        RealCalls++;
        await Task.Delay(30, token);

        if (IsDown)
        {
            throw new PaymentGatewayException("502 Bad Gateway — платіжний шлюз не відповідає.");
        }

        return $"TX-{orderId:D4}-{(int)amount}";
    }
}

public class RecommendationService
{
    private readonly Queue<int> _responseTimesMs;

    public RecommendationService(params int[] responseTimesMs)
    {
        _responseTimesMs = new Queue<int>(responseTimesMs);
    }

    public int Calls { get; private set; }

    public async ValueTask<List<string>> GetRecommendationsAsync(int customerId, CancellationToken token)
    {
        Calls++;
        int delay = _responseTimesMs.Count > 0 ? _responseTimesMs.Dequeue() : 100;

        await Task.Delay(delay, token);

        return new List<string> { "Бездротові навушники", "Чохол для ноутбука 15\"", "Ергономічна мишка" };
    }
}
