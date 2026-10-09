public class Building
{
    public string Address { get; }

    public string Purpose { get; protected set; } = "не визначено";

    public Building(string address)
    {
        Address = address;
    }

    public virtual void GetPurpose()
    {
        Purpose = $"Будівля за адресою {Address}: загальне призначення";
        Console.WriteLine(Purpose);
    }
}