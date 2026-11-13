using System.Text;
using lab17v11.Data;
using lab17v11.Extensions;
using lab17v11.Models;

namespace lab17v11;

internal class Program
{
    private const int EventsPerPage = 2;

    private static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;

        List<Event> events = SeedData.GetEvents();

        ShowAllTickets(events);
        ShowEventsByPages(events);
        ShowRevenue(events);
    }

    private static void ShowAllTickets(List<Event> events)
    {
        PrintHeader("1. SelectMany — усі квитки всіх учасників усіх подій");

        List<Ticket> allTickets = events
            .SelectMany(e => e.Participants)
            .SelectMany(p => p.Tickets)
            .ToList();

        foreach (Ticket t in allTickets)
            Console.WriteLine($"  {t}");

        Console.WriteLine($"\n  Усього квитків: {allTickets.Count}");

        Console.WriteLine("\n  Квитки з прив'язкою до події та учасника (перші 5):");

        var detailed = events
            .SelectMany(e => e.Participants, (e, p) => new { Event = e, Participant = p })
            .SelectMany(x => x.Participant.Tickets, (x, t) => new
            {
                EventName = x.Event.Name,
                ParticipantName = x.Participant.Name,
                Ticket = t
            })
            .Take(5);

        foreach (var row in detailed)
            Console.WriteLine($"  {row.EventName,-30} | {row.ParticipantName,-20} | {row.Ticket.Type,-14} | {row.Ticket.Price,8:F2} грн");
    }

    private static void ShowEventsByPages(List<Event> events)
    {
        PrintHeader($"2. Пагінація — події по {EventsPerPage} на сторінку");

        int totalPages = events.PageCount(EventsPerPage);

        for (int page = 1; page <= totalPages; page++)
        {
            Console.WriteLine($"\n  --- Сторінка {page} з {totalPages} ---");

            foreach (Event ev in events.Page(page, EventsPerPage))
                Console.WriteLine($"  {ev}");
        }
    }

    private static void ShowRevenue(List<Event> events)
    {
        PrintHeader("3. Custom Extension — TotalTicketRevenue()");

        foreach (Event ev in events)
            Console.WriteLine($"  {ev.Name,-30} {ev.TicketRevenue(),10:F2} грн");

        Console.WriteLine(new string('-', 50));
        Console.WriteLine($"  {"Загальний дохід:",-30} {events.TotalTicketRevenue(),10:F2} грн");

        decimal topTwo = events
            .OrderByDescending(e => e.TicketRevenue())
            .Take(2)
            .TotalTicketRevenue();

        Console.WriteLine($"  {"Дохід двох найприбутковіших:",-30} {topTwo,10:F2} грн");
    }

    private static void PrintHeader(string title)
    {
        Console.WriteLine();
        Console.WriteLine(new string('=', 70));
        Console.WriteLine($" {title}");
        Console.WriteLine(new string('=', 70));
    }
}
