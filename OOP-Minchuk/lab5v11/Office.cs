public class Office : Building
{
    public int NumFloors { get; }

    public Office(string address, int numFloors) : base(address)
    {
        NumFloors = numFloors;
    }

    public override void GetPurpose()
    {
        Purpose = $"Офіс за адресою {Address}: робота компаній, поверхів — {NumFloors}";
        Console.WriteLine(Purpose);
    }
}