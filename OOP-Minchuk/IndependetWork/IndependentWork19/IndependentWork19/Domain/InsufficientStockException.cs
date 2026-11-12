namespace IndependentWork19.Domain;

public class InsufficientStockException : Exception
{
    public string ProductSku { get; } = string.Empty;
    public int RequestedQuantity { get; }
    public int AvailableQuantity { get; }
    public int Shortage => Math.Max(0, RequestedQuantity - AvailableQuantity);

    public InsufficientStockException()
        : base("Недостатньо товару на складі.")
    {
    }

    public InsufficientStockException(string message)
        : base(message)
    {
    }

    public InsufficientStockException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public InsufficientStockException(string productSku, int requestedQuantity, int availableQuantity)
        : base($"Недостатньо товару '{productSku}': запитано {requestedQuantity} шт., доступно {availableQuantity} шт.")
    {
        ProductSku = productSku;
        RequestedQuantity = requestedQuantity;
        AvailableQuantity = availableQuantity;
    }
}
