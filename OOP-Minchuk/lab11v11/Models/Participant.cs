namespace lab17v11.Models;

public class Participant
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<Ticket> Tickets { get; set; } = new();

    public override string ToString() => $"{Name} (квитків: {Tickets.Count})";
}
