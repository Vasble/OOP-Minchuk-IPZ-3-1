namespace IndependentWork19.Domain;

public record Reservation(string ProductSku, int Quantity, Guid Id);

public class Warehouse
{
    private readonly Dictionary<string, int> _stock = new(StringComparer.OrdinalIgnoreCase)
    {
        ["LAPTOP-15"] = 4,
        ["MOUSE-01"] = 120,
        ["MONITOR-27"] = 0
    };

    public int GetStock(string sku) => _stock.GetValueOrDefault(sku);

    public Reservation Reserve(string sku, int quantity)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sku);

        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), quantity, "Кількість має бути більшою за 0.");
        }

        if (!_stock.TryGetValue(sku, out int available))
        {
            throw new KeyNotFoundException($"Товар '{sku}' не знайдено в каталозі.");
        }

        if (available < quantity)
        {
            throw new InsufficientStockException(sku, quantity, available);
        }

        _stock[sku] = available - quantity;
        return new Reservation(sku, quantity, Guid.NewGuid());
    }

    public Result<Reservation> TryReserve(string sku, int quantity)
    {
        if (string.IsNullOrWhiteSpace(sku) || quantity <= 0)
        {
            return Result<Reservation>.Failure("Некоректний артикул або кількість.");
        }

        if (!_stock.TryGetValue(sku, out int available))
        {
            return Result<Reservation>.Failure($"Товар '{sku}' не знайдено.");
        }

        if (available < quantity)
        {
            return Result<Reservation>.Failure($"Доступно лише {available} шт. товару '{sku}'.");
        }

        _stock[sku] = available - quantity;
        return Result<Reservation>.Success(new Reservation(sku, quantity, Guid.NewGuid()));
    }

    public void SyncWithSupplier(string sku)
    {
        try
        {
            throw new IOException("Сервер постачальника розірвав з'єднання.");
        }
        catch (IOException ex)
        {
            throw new InsufficientStockException($"Не вдалося оновити залишки товару '{sku}' у постачальника.", ex);
        }
    }
}
