using IndependentWork19.Domain;
using IndependentWork19.Scenarios;

Console.OutputEncoding = System.Text.Encoding.UTF8;

DemoCustomException();
DemoResultPattern();

await RetryScenario.RunAsync();
await CircuitBreakerScenario.RunAsync();
await TimeoutFallbackScenario.RunAsync();

static void DemoCustomException()
{
    Console.WriteLine("=== Власний виняток InsufficientStockException ===");

    var warehouse = new Warehouse();
    var orders = new (string Sku, int Quantity)[]
    {
        ("LAPTOP-15", 2),
        ("LAPTOP-15", 5),
        ("MONITOR-27", 1),
        ("PHONE-X", 1),
        ("MOUSE-01", 0)
    };

    foreach ((string sku, int quantity) in orders)
    {
        try
        {
            Reservation reservation = warehouse.Reserve(sku, quantity);
            Console.WriteLine($"✔ Зарезервовано {reservation.Quantity} шт. '{reservation.ProductSku}'. Залишок: {warehouse.GetStock(sku)}");
        }
        catch (InsufficientStockException ex)
        {
            Console.WriteLine($"✘ {ex.GetType().Name}: {ex.Message}");
            Console.WriteLine($"    Sku={ex.ProductSku}, Requested={ex.RequestedQuantity}, Available={ex.AvailableQuantity}, Shortage={ex.Shortage}");
        }
        catch (KeyNotFoundException ex)
        {
            Console.WriteLine($"✘ {ex.GetType().Name}: {ex.Message}");
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"✘ {ex.GetType().Name}: {ex.Message}");
        }
        finally
        {
            Console.WriteLine($"    finally: запит ({sku}, {quantity}) оброблено, з'єднання зі складом закрито.");
        }
    }

    try
    {
        warehouse.SyncWithSupplier("LAPTOP-15");
    }
    catch (InsufficientStockException ex)
    {
        Console.WriteLine($"✘ {ex.Message}");
        Console.WriteLine($"    InnerException: {ex.InnerException?.GetType().Name} — {ex.InnerException?.Message}");
    }

    var empty = new InsufficientStockException();
    Console.WriteLine($"Конструктор без параметрів: \"{empty.Message}\"");
    Console.WriteLine();
}

static void DemoResultPattern()
{
    Console.WriteLine("=== Альтернатива: Result<T> для очікуваних неуспіхів ===");

    var warehouse = new Warehouse();

    foreach ((string sku, int quantity) in new[] { ("MOUSE-01", 3), ("LAPTOP-15", 10), ("PHONE-X", 1) })
    {
        Result<Reservation> result = warehouse.TryReserve(sku, quantity);

        Console.WriteLine(result.IsSuccess
            ? $"✔ TryReserve({sku}, {quantity}): успіх, резерв {result.Value!.Id.ToString()[..8]}"
            : $"✘ TryReserve({sku}, {quantity}): {result.Error}");
    }

    Console.WriteLine();
}
