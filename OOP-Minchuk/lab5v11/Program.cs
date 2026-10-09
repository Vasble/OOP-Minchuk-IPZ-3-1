Console.OutputEncoding = System.Text.Encoding.UTF8;

List<Building> buildings = new()
{
    new House("вул. Соборна, 12", 5),
    new Office("просп. Миру, 40", 9),
    new Shop("вул. Київська, 7", "електроніка"),
    new House("вул. Шевченка, 3", 3)
};

Console.WriteLine("=== Поліморфні виклики GetPurpose() ===");
List<string> purposes = new();

foreach (Building building in buildings)
{
    building.GetPurpose();        
    purposes.Add(building.Purpose); 
}

Console.WriteLine();
Console.WriteLine("=== Агрегація: список призначень усіх будівель ===");
for (int i = 0; i < purposes.Count; i++)
{
    Console.WriteLine($"{i + 1}. {purposes[i]}");
}

Console.WriteLine($"Усього будівель: {purposes.Count}");