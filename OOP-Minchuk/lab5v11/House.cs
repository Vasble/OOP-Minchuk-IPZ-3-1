public class House : Building
{
    public int NumRooms { get; }

    public House(string address, int numRooms) : base(address)
    {
        NumRooms = numRooms;
    }

    public override void GetPurpose()
    {
        Purpose = $"Будинок за адресою {Address}: житло, кімнат — {NumRooms}";
        Console.WriteLine(Purpose);
    }
}