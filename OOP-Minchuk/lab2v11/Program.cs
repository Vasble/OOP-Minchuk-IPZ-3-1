using System;

namespace OOPLab2
{
    public class Library
    {
        private string _name;
        private string _address;
        private int _booksCount;

        public string Name
        {
            get => _name;
            set => _name = string.IsNullOrWhiteSpace(value) ? "Unnamed Library" : value;
        }

        public string Address
        {
            get => _address;
            set => _address = string.IsNullOrWhiteSpace(value) ? "N/A" : value;
        }

        public int BooksCount => _booksCount;

        public Library(string name, string address, int initialBooksCount)
        {
            Name = name;
            Address = address;
            _booksCount = initialBooksCount < 0 ? 0 : initialBooksCount;

            Console.WriteLine($"[Constructor] Створено бібліотеку \"{Name}\" з {_booksCount} книгами.");
        }

        public Library(string name, string address) : this(name, address, 0)
        {
        }

        public void AddBook(string bookTitle)
        {
            _booksCount++;
            Console.WriteLine($"Книгу \"{bookTitle}\" додано до бібліотеки \"{Name}\". " +
                               $"Тепер книг: {_booksCount}.");
        }

        ~Library()
        {
            Console.WriteLine($"[Destructor] Об'єкт Library \"{_name}\" знищується збирачем сміття.");
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("--- Creating objects ---");

            Library lib1 = new Library("Центральна бібліотека", "вул. Соборна, 10", 500);

            Library lib2 = new Library("Дитяча бібліотека", "вул. Шкільна, 5");

            Library lib3 = new Library("Наукова бібліотека", "просп. Миру, 22", 1200);

            Console.WriteLine("--- Objects created ---\n");

            lib1.AddBook("Кобзар");
            lib1.AddBook("Тіні забутих предків");

            lib2.AddBook("Азбука");

            lib3.AddBook("Основи програмування на C#");

            Console.WriteLine();
            Console.WriteLine($"{lib1.Name} ({lib1.Address}): {lib1.BooksCount} книг.");
            Console.WriteLine($"{lib2.Name} ({lib2.Address}): {lib2.BooksCount} книг.");
            Console.WriteLine($"{lib3.Name} ({lib3.Address}): {lib3.BooksCount} книг.");

            Console.WriteLine("\n--- End of Main, preparing for GC ---");

            lib1 = null;
            lib2 = null;
            lib3 = null;

            GC.Collect();
            GC.WaitForPendingFinalizers();

            Console.WriteLine("--- GC finished ---");
        }
    }
}