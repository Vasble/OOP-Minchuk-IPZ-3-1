using System;
using System.Globalization;
using System.Text;

namespace Lab4v13
{
    internal static class Program
    {
        private static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture; 

            Console.WriteLine("=== 1. Створення об'єктів ===");
            var furniture = new Furniture("Метал", 10.0);
            var chair = new Chair("Дерево", 5.5, true);
            var table = new Table("Скло", 20.0, 4);

            Console.WriteLine($"Furniture: {furniture.Material}, {furniture.Weight:F1} кг");
            Console.WriteLine($"Chair: {chair.Material}, {chair.Weight:F1} кг, підлокітники: {(chair.HasArmrests ? "так" : "ні")}");
            Console.WriteLine($"Table: {table.Material}, {table.Weight:F1} кг, ніжок: {table.NumLegs}");

            Console.WriteLine();
            Console.WriteLine("=== 2. Поліморфізм (virtual / override) ===");
            Furniture[] items = { furniture, chair, table };
            foreach (Furniture item in items)
            {
                Console.WriteLine($"[{item.GetType().Name}]");
                item.Assemble(); 
            }

            Console.WriteLine();
            Console.WriteLine("=== 3. Власні методи похідних класів ===");
            chair.SitOn();
            table.PlaceItems();

            Console.WriteLine();
            Console.WriteLine("=== 4. override vs new ===");

            Furniture chairAsFurniture = chair; 
            Chair chairAsChair = chair;     

            Console.WriteLine("-- override (Assemble): результат НЕ залежить від типу посилання --");
            Console.WriteLine("Через Furniture-посилання:");
            chairAsFurniture.Assemble();
            Console.WriteLine("Через Chair-посилання:");
            chairAsChair.Assemble();

            Console.WriteLine("-- new (GetFurnitureType): результат ЗАЛЕЖИТЬ від типу посилання --");
            Console.WriteLine($"Furniture-посилання на Chair: {chairAsFurniture.GetFurnitureType()}");
            Console.WriteLine($"Chair-посилання на Chair:     {chairAsChair.GetFurnitureType()}");
            Console.WriteLine($"Table (new не використано):   {table.GetFurnitureType()}");
        }
    }
}
