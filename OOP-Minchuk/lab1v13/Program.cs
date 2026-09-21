using System;
 
namespace Lab1V13
{
    class Library
    {
        private string name;
        private string address;
        private int booksCount;
 
        public string Name
        {
            get { return name; }
            set { name = value; }
        }
 
        public string Address
        {
            get { return address; }
            set { address = value; }
        }
 
        public int BooksCount
        {
            get { return booksCount; }
            private set { booksCount = value; }
        }
 
        public Library(string name, string address, int booksCount = 0)
        {
            this.name = name;
            this.address = address;
            this.booksCount = booksCount;
        }

        ~Library()
        {
            Console.WriteLine($"Об'єкт бібліотеки \"{name}\" знищено.");
        }

        public void AddBook(string title, int quantity = 1)
        {
            BooksCount += quantity;
            Console.WriteLine($"Бібліотека \"{name}\": додано книгу \"{title}\" " +
                               $"({quantity} прим.). Загальна кількість книг: {BooksCount}.");
        }
 
        public string GetInfo()
        {
            return $"Бібліотека: {name} | Адреса: {address} | Кількість книг: {BooksCount}";
        }
    }
 
    class Program
    {
        static void Main(string[] args)
        {
            Library library1 = new Library("Наукова бібліотека", "вул. Соборна, 12");
            Library library2 = new Library("Дитяча бібліотека", "вул. Шкільна, 5", 150);
            Library library3 = new Library("Обласна бібліотека", "просп. Миру, 44");
 
            Console.WriteLine("=== Демонстрація роботи класу Library ===\n");
 
            library1.AddBook("Кобзар", 3);
            library1.AddBook("Тіні забутих предків");
 
            library2.AddBook("Пригоди Незнайка", 10);
 
            library3.AddBook("Історія України", 5);
            library3.AddBook("Основи програмування", 2);
 
            Console.WriteLine();

            Console.WriteLine(library1.GetInfo());
            Console.WriteLine(library2.GetInfo());
            Console.WriteLine(library3.GetInfo());
        }
    }
}
