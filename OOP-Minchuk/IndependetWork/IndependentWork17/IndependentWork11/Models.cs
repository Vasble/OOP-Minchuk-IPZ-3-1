namespace IndependentWork11;

public record Product(int Id, string Name, string Category, decimal UnitPrice);

public record Customer(int Id, string Name, string City);

public record Sale(int Id, int ProductId, int Quantity, DateTime SaleDate, int CustomerId);

public record SalesData(List<Product> Products, List<Customer> Customers, List<Sale> Sales);

public record ProductSales(string ProductName, int TotalQuantity);

public record CategoryTopProducts(string Category, List<ProductSales> TopProducts);

public record CityRevenue(string City, decimal Revenue, int SalesCount, int CustomersCount);

public record MonthlySales(int Year, int Month, int TotalQuantity, decimal Revenue, int SalesCount);

public record CustomerInsight(
    string CustomerName,
    string City,
    int OrdersCount,
    decimal TotalSpent,
    decimal AverageOrder,
    string FavoriteCategory);
