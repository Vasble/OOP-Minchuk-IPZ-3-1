using lab19v11.Common;
using lab19v11.Exceptions;
using lab19v11.Models;

namespace lab19v11.Services;

/// <summary>
/// Сервіс обробки платежів через імітований платіжний шлюз.
/// </summary>
public class PaymentService
{
    public const decimal SingleTransactionLimit = 50_000m;

    private readonly Dictionary<string, Transaction> _transactions = new();
    private int _nextId = 1;

    public string GatewayName { get; }

    /// <summary>Імітація стану шлюзу: false — шлюз не відповідає.</summary>
    public bool IsGatewayAvailable { get; set; } = true;

    public PaymentService(string gatewayName = "LiqPay")
    {
        GatewayName = gatewayName;
    }

    /// <summary>
    /// Проводить платіж через шлюз.
    /// Викидає <see cref="PaymentGatewayException"/>, якщо шлюз повернув помилку.
    /// </summary>
    public void ProcessPayment(decimal amount)
    {
        // Помилка програміста (некоректний аргумент) — стандартний виняток
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Сума платежу має бути більшою за 0.");

        // Помилки, які повертає зовнішній шлюз — власний виняток
        if (!IsGatewayAvailable)
        {
            throw new PaymentGatewayException(
                GatewayName,
                "GATEWAY_UNAVAILABLE",
                "Платіжний шлюз не відповідає. Спробуйте пізніше.",
                new TimeoutException("Час очікування відповіді від шлюзу вичерпано."));
        }

        if (amount > SingleTransactionLimit)
        {
            throw new PaymentGatewayException(
                GatewayName,
                "LIMIT_EXCEEDED",
                $"Сума {amount:F2} грн перевищує ліміт однієї транзакції ({SingleTransactionLimit:F2} грн).");
        }

        var transaction = new Transaction
        {
            Id = $"TX-{_nextId++:D4}",
            Amount = amount,
            CreatedAt = DateTime.Now
        };

        _transactions.Add(transaction.Id, transaction);
    }

    /// <summary>
    /// Повертає кошти за транзакцією.
    /// Для очікуваних невдач (транзакцію не знайдено, вже повернена) повертає Result.Failure.
    /// </summary>
    public Result<bool> RefundPayment(string transactionId)
    {
        if (string.IsNullOrWhiteSpace(transactionId))
            return Result<bool>.Failure("Не вказано ідентифікатор транзакції.");

        if (!_transactions.TryGetValue(transactionId, out Transaction? transaction))
            return Result<bool>.Failure($"Транзакцію \"{transactionId}\" не знайдено.");

        if (transaction.IsRefunded)
            return Result<bool>.Failure($"Кошти за транзакцією \"{transactionId}\" вже повернено.");

        transaction.IsRefunded = true;
        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Список усіх транзакцій (лише для читання).
    /// </summary>
    public IReadOnlyList<Transaction> GetTransactions() =>
        _transactions.Values.ToList();
}
