using System;

namespace Lab4v13
{
    public class Table : Furniture
    {
        private int _numLegs;

        public int NumLegs
        {
            get => _numLegs;
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException(nameof(value), "Кількість ніжок має бути додатною.");
                _numLegs = value;
            }
        }

        public Table(string material, double weight, int numLegs)
            : base(material, weight)
        {
            NumLegs = numLegs;
        }

        public override void Assemble()
        {
            Console.WriteLine($"Стіл: кріпимо стільницю ({Material}) до ніжок, кількість ніжок: {NumLegs}.");
        }

        public void PlaceItems()
        {
            Console.WriteLine($"На стіл ({Material}) розміщено предмети. Кількість опор: {NumLegs}.");
        }
    }
}
