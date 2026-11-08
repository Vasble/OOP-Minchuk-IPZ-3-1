namespace lab10v11;

public class Event
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public DateTime Date { get; set; }
    public int LocationId { get; set; }

    public override string ToString() => $"#{Id} {Name} ({Date:dd.MM.yyyy})";
}

public class Venue
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int Capacity { get; set; }

    public override string ToString() => $"#{Id} {Name} (місткість {Capacity})";
}
