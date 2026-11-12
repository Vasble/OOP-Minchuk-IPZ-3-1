namespace IndependentWork11;

public class SalesAnalytics
{
    private readonly SalesData _data;

    public SalesAnalytics(SalesData data)
    {
        _data = data;
    }

    public List<CategoryTopProducts> TopProductsByCategory(int topCount = 3) =>
        _data.Sales
            .GroupBy(sale => sale.ProductId)
            .Select(group => new
            {
                ProductId = group.Key,
                TotalQuantity = group.Sum(sale => sale.Quantity)
            })
            .Join(
                _data.Products,
                totals => totals.ProductId,
                product => product.Id,
                (totals, product) => new
                {
                    product.Name,
                    product.Category,
                    totals.TotalQuantity
                })
            .GroupBy(item => item.Category)
            .Select(categoryGroup => new CategoryTopProducts(
                categoryGroup.Key,
                categoryGroup
                    .OrderByDescending(item => item.TotalQuantity)
                    .ThenBy(item => item.Name)
                    .Take(topCount)
                    .Select(item => new ProductSales(item.Name, item.TotalQuantity))
                    .ToList()))
            .OrderBy(result => result.Category)
            .ToList();

    public List<CityRevenue> RevenueByCity() =>
        _data.Sales
            .Join(
                _data.Products,
                sale => sale.ProductId,
                product => product.Id,
                (sale, product) => new { sale.CustomerId, Revenue = sale.Quantity * product.UnitPrice })
            .Join(
                _data.Customers,
                item => item.CustomerId,
                customer => customer.Id,
                (item, customer) => new { customer.City, item.CustomerId, item.Revenue })
            .GroupBy(item => item.City)
            .Select(cityGroup => new CityRevenue(
                cityGroup.Key,
                cityGroup.Sum(item => item.Revenue),
                cityGroup.Count(),
                cityGroup.Select(item => item.CustomerId).Distinct().Count()))
            .OrderByDescending(result => result.Revenue)
            .ToList();

    public List<MonthlySales> SalesByMonth() =>
        _data.Sales
            .Join(
                _data.Products,
                sale => sale.ProductId,
                product => product.Id,
                (sale, product) => new
                {
                    sale.SaleDate.Year,
                    sale.SaleDate.Month,
                    sale.Quantity,
                    Revenue = sale.Quantity * product.UnitPrice
                })
            .GroupBy(item => new { item.Year, item.Month })
            .Select(group => new MonthlySales(
                group.Key.Year,
                group.Key.Month,
                group.Sum(item => item.Quantity),
                group.Sum(item => item.Revenue),
                group.Count()))
            .OrderBy(result => result.Year)
            .ThenBy(result => result.Month)
            .ToList();

    public List<CustomerInsight> TopCustomersWithFavoriteCategory(int topCount = 10)
    {
        var customerStats = _data.Sales
            .Join(
                _data.Products,
                sale => sale.ProductId,
                product => product.Id,
                (sale, product) => new
                {
                    sale.Id,
                    sale.CustomerId,
                    product.Category,
                    Revenue = sale.Quantity * product.UnitPrice
                })
            .GroupBy(item => item.CustomerId)
            .Select(group => new
            {
                CustomerId = group.Key,
                OrdersCount = group.Count(),
                TotalSpent = group.Sum(item => item.Revenue),
                FavoriteCategory = group
                    .GroupBy(item => item.Category)
                    .Select(categoryGroup => new { Category = categoryGroup.Key, Spent = categoryGroup.Sum(item => item.Revenue) })
                    .Aggregate((best, next) => next.Spent > best.Spent ? next : best)
                    .Category
            })
            .ToList();

        decimal averageSpent = customerStats.Average(stat => stat.TotalSpent);

        return customerStats
            .Where(stat => stat.TotalSpent > averageSpent && stat.OrdersCount >= 5)
            .Join(
                _data.Customers,
                stat => stat.CustomerId,
                customer => customer.Id,
                (stat, customer) => new CustomerInsight(
                    customer.Name,
                    customer.City,
                    stat.OrdersCount,
                    stat.TotalSpent,
                    Math.Round(stat.TotalSpent / stat.OrdersCount, 2),
                    stat.FavoriteCategory))
            .OrderByDescending(insight => insight.TotalSpent)
            .Take(topCount)
            .ToList();
    }

    public List<CityRevenue> RevenueByCityManual()
    {
        var prices = new Dictionary<int, decimal>();
        foreach (Product product in _data.Products)
        {
            prices[product.Id] = product.UnitPrice;
        }

        var cities = new Dictionary<int, string>();
        foreach (Customer customer in _data.Customers)
        {
            cities[customer.Id] = customer.City;
        }

        var revenue = new Dictionary<string, decimal>();
        var counts = new Dictionary<string, int>();
        var customersByCity = new Dictionary<string, HashSet<int>>();

        foreach (Sale sale in _data.Sales)
        {
            string city = cities[sale.CustomerId];
            revenue[city] = revenue.GetValueOrDefault(city) + sale.Quantity * prices[sale.ProductId];
            counts[city] = counts.GetValueOrDefault(city) + 1;

            if (!customersByCity.TryGetValue(city, out HashSet<int>? ids))
            {
                ids = new HashSet<int>();
                customersByCity[city] = ids;
            }

            ids.Add(sale.CustomerId);
        }

        var result = new List<CityRevenue>();
        foreach ((string city, decimal total) in revenue)
        {
            result.Add(new CityRevenue(city, total, counts[city], customersByCity[city].Count));
        }

        result.Sort((a, b) => b.Revenue.CompareTo(a.Revenue));
        return result;
    }
}
