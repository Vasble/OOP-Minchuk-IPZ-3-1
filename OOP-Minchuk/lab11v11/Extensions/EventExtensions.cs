using lab17v11.Models;

namespace lab17v11.Extensions;

public static class EventExtensions
{
    public static decimal TotalTicketRevenue(this IEnumerable<Event> events)
    {
        ArgumentNullException.ThrowIfNull(events);

        return events
            .SelectMany(e => e.Participants)
            .SelectMany(p => p.Tickets)
            .Sum(t => t.Price);
    }
    public static decimal TicketRevenue(this Event ev)
    {
        ArgumentNullException.ThrowIfNull(ev);
        return new[] { ev }.TotalTicketRevenue();
    }
}
