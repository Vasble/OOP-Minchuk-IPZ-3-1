namespace lab17v11.Models;
public class Event
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<Participant> Participants { get; set; } = new();

    public override string ToString() => $"Подія #{Id}: {Name} (учасників: {Participants.Count})";
}
