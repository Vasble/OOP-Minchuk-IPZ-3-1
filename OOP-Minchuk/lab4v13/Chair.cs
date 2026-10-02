using System;

namespace Lab4v13
{
    public class Chair : Furniture
    {
        public bool HasArmrests { get; set; }

        public Chair(string material, double weight, bool hasArmrests)
            : base(material, weight)
        {
            HasArmrests = hasArmrests;
        }
        public override void Assemble()
        {
            base.Assemble();
            string armrests = HasArmrests ? ", додаємо підлокітники" : string.Empty;
            Console.WriteLine($"Стілець: прикручуємо ніжки та спинку{armrests}.");
        }

        public new string GetFurnitureType()
        {
            return "Стілець";
        }

        public void SitOn()
        {
            string support = HasArmrests ? ", спираючись на підлокітники" : string.Empty;
            Console.WriteLine($"Ви сідаєте на стілець ({Material}){support}.");
        }
    }
}
