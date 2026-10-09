using System;

namespace Lab4v13
{
    public class Furniture
    {
        private string _material = string.Empty;
        private double _weight;

        public string Material
        {
            get => _material;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Матеріал не може бути порожнім.", nameof(value));
                _material = value;
            }
        }

        public double Weight
        {
            get => _weight;
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException(nameof(value), "Вага має бути додатною.");
                _weight = value;
            }
        }

        public Furniture(string material, double weight)
        {
            Material = material;
            Weight = weight;
        }

        public virtual void Assemble()
        {
            Console.WriteLine($"Меблі ({Material}, {Weight:F1} кг) збираються за загальною інструкцією.");
        }

        public string GetFurnitureType()
        {
            return "Меблі";
        }
    }
}
