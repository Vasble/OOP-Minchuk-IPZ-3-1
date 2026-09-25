using System;

namespace IndependentWork2
{

    public class Product
    {
        private readonly int _id;
        private readonly string _name;
        private readonly decimal _price;
        private readonly string _category;
        private readonly int _stockCount;

        public int Id => _id;
        public string Name => _name;
        public decimal Price => _price;
        public string Category => _category;
        public int StockCount => _stockCount;

        public Product(int id, string name, decimal price, string category, int stockCount)
        {
            _id = id;
            _name = name;
            _price = price;
            _category = category;
            _stockCount = stockCount;
        }

        public Product(int id, string name, decimal price)
            : this(id, name, price, "Uncategorized", 0)
        {
        }

        public Product(Product other)
            : this(other.Id, other.Name, other.Price, other.Category, other.StockCount)
        {
        }

        public override string ToString()
        {
            return $"ID: {Id}, Name: {Name}, Price: {Price:C}, Category: {Category}, Stock: {StockCount}";
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Створення товарів\n");

            var product1 = new Product(101, "Laptop", 35000m, "Electronics", 15);
            Console.WriteLine("Товар 1 (основний конструктор):");
            Console.WriteLine(product1);
            Console.WriteLine();

            var product2 = new Product(102, "Mouse", 800m);
            Console.WriteLine("Товар 2 (скорочений конструктор):");
            Console.WriteLine(product2);
            Console.WriteLine();

            var product3 = new Product(product1);
            Console.WriteLine("Товар 3 (конструктор копіювання, копія Товару 1):");
            Console.WriteLine(product3);
        }
    }
}