namespace lab19v11.Models;
public class Transaction
{
    public string Id { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public DateTime CreatedAt { get; init; }
    public bool IsRefunded { get; set; }

    public override string ToString() =>
        $"{Id} | {Amount,10:F2} грн | {CreatedAt:dd.MM.yyyy HH:mm:ss} | {(IsRefunded ? "повернено" : "оплачено")}";
}
