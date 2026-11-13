namespace lab17v11.Models;

public class Ticket
{
    public int Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public decimal Price { get; set; }

    public override string ToString() => $"Квиток #{Id} [{Type}] — {Price:F2} грн";
}
