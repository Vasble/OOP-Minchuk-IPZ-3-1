

public class Product
{
    public string Name { get; }
    public double Price { get; }

    public Product(string name, double price)
    {
        Name = name;
        Price = price;
    }
}

public class CartItem
{
    public Product Product { get; }
    public int Quantity { get; }

    public CartItem(Product product, int quantity)
    {
        Product = product;
        Quantity = quantity;
    }

    public double LineTotal => Product.Price * Quantity;

    public double DiscountedLineTotal => Product.Price > 500 ? LineTotal * 0.9 : LineTotal;
}

public class Cart
{
    private readonly List<CartItem> _items = new();

    public void AddItem(Product product, int quantity)
    {
        _items.Add(new CartItem(product, quantity));
    }

    public double GetTotal()
    {
        double total = 0;
        foreach (var item in _items)
        {
            total += item.DiscountedLineTotal;
        }
        return total;
    }

    public void PrintReceipt()
    {
        foreach (var item in _items)
        {
            string discountMark = item.Product.Price > 500 ? " (знижка 10%)" : "";
            Console.WriteLine($"{item.Product.Name}: {item.DiscountedLineTotal:F2} грн{discountMark}");
        }
    }
}

public static class ObjectOrientedDemo
{
    public static void Run()
    {
        Console.WriteLine("=== Об'єктна версія ===");

        var cart = new Cart();
        cart.AddItem(new Product("Ноутбук", 25000), 1);
        cart.AddItem(new Product("Мишка", 350), 2);
        cart.AddItem(new Product("Клавіатура", 800), 1);
        cart.AddItem(new Product("Навушники", 600), 1);
        cart.AddItem(new Product("Флешка", 250), 3);

        cart.PrintReceipt();
        Console.WriteLine($"Підсумок кошика (об'єктна версія): {cart.GetTotal():F2} грн");
    }
}
