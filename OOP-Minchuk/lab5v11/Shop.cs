public class Shop : Building
{
    public string ProductType { get; }

    public Shop(string address, string productType) : base(address)
    {
        ProductType = productType;
    }

    public override void GetPurpose()
    {
        Purpose = $"Магазин за адресою {Address}: торгівля, товари — {ProductType}";
        Console.WriteLine(Purpose);
    }
}