

using System.Diagnostics;
using System.Text;
using lab10v11;

Console.OutputEncoding = Encoding.UTF8;

List<Venue> venues = DataSeed.Venues();
List<Event> events = DataSeed.Events();
DateTime today = DateTime.Today;

Console.WriteLine($"Дані: {venues.Count} локацій, {events.Count} подій. Сьогодні: {today:dd.MM.yyyy}");
Console.WriteLine("Час: «перший запуск» — одне виконання (включно з JIT-компіляцією),");
Console.WriteLine("      «середній» — середнє з 10 000 повторів після прогріву.\n");

// ---------------------------------------------------------------------------
// Запит 1. 
// ---------------------------------------------------------------------------
Header("Запит 1. Події у 2024 році на локаціях з місткістю > 500");

var q1Query = Bench.Measure("синтаксис запитів", () =>
    (from e in events
     join v in venues on e.LocationId equals v.Id
     where e.Date.Year == 2024 && v.Capacity > 500
     orderby e.Date
     select new { e.Name, e.Date, Venue = v.Name, v.Capacity }).ToList());

var q1Method = Bench.Measure("синтаксис методів", () =>
    events.Join(venues, e => e.LocationId, v => v.Id,
                (e, v) => new { e.Name, e.Date, Venue = v.Name, v.Capacity })
          .Where(x => x.Date.Year == 2024 && x.Capacity > 500)
          .OrderBy(x => x.Date)
          .ToList());

foreach (var x in q1Method)
    Console.WriteLine($"  {x.Date:dd.MM.yyyy}  {x.Name,-20} {x.Venue} ({x.Capacity})");
Check(q1Query.SequenceEqual(q1Method));

// ---------------------------------------------------------------------------
// Запит 2. 
// ---------------------------------------------------------------------------
Header("Запит 2. Події та їхні локації (inner join)");

var q2Query = Bench.Measure("синтаксис запитів", () =>
    (from e in events
     join v in venues on e.LocationId equals v.Id
     select new { Event = e.Name, Venue = v.Name }).ToList());

var q2Method = Bench.Measure("синтаксис методів", () =>
    events.Join(venues, e => e.LocationId, v => v.Id,
                (e, v) => new { Event = e.Name, Venue = v.Name })
          .ToList());

foreach (var x in q2Method)
    Console.WriteLine($"  {x.Event,-20} -> {x.Venue}");
Check(q2Query.SequenceEqual(q2Method));

// ---------------------------------------------------------------------------
// Запит 3.
// ---------------------------------------------------------------------------
Header("Запит 3. Кількість подій для кожної локації (GroupJoin + Count)");

var q3Query = Bench.Measure("синтаксис запитів", () =>
    (from v in venues
     join e in events on v.Id equals e.LocationId into venueEvents
     let count = venueEvents.Count()
     orderby count descending, v.Name
     select new { Venue = v.Name, Count = count }).ToList());

var q3Method = Bench.Measure("синтаксис методів", () =>
    venues.GroupJoin(events, v => v.Id, e => e.LocationId,
                     (v, venueEvents) => new { Venue = v.Name, Count = venueEvents.Count() })
          .OrderByDescending(x => x.Count)
          .ThenBy(x => x.Venue)
          .ToList());

foreach (var x in q3Method)
    Console.WriteLine($"  {x.Venue,-20} {x.Count}");
Check(q3Query.SequenceEqual(q3Method));

// ---------------------------------------------------------------------------
// Запит 4.
// ---------------------------------------------------------------------------
Header("Запит 4. Подія з найближчою датою (від сьогодні)");

var q4Query = Bench.Measure("синтаксис запитів", () =>
    (from e in events
     where e.Date >= today
     orderby e.Date
     select e).FirstOrDefault());

// MinBy (.NET 6+) — один прохід O(n) без сортування.
var q4Method = Bench.Measure("синтаксис методів", () =>
    events.Where(e => e.Date >= today).MinBy(e => e.Date));

Console.WriteLine(q4Method is null
    ? "  Майбутніх подій немає."
    : $"  {q4Method} — через {(q4Method.Date - today).Days} дн., " +
      $"локація: {venues.First(v => v.Id == q4Method.LocationId).Name}");
Check(q4Query == q4Method);

Performance.Run();

static void Header(string title)
{
    Console.WriteLine();
    Console.WriteLine(new string('=', 70));
    Console.WriteLine(title);
    Console.WriteLine(new string('=', 70));
}

static void Check(bool same) =>
    Console.WriteLine(same ? "  [OK] обидва синтаксиси дали однаковий результат"
                           : "  [!!] результати синтаксисів відрізняються");

static class Bench
{
    public static T Measure<T>(string label, Func<T> query, int iterations = 10_000)
    {
        var sw = Stopwatch.StartNew();
        T result = query();
        sw.Stop();
        double coldMs = sw.Elapsed.TotalMilliseconds;

        sw.Restart();
        for (int i = 0; i < iterations; i++)
            query();
        sw.Stop();
        double avgUs = sw.Elapsed.TotalMilliseconds * 1000 / iterations;

        Console.WriteLine($"  [{label}] перший запуск: {coldMs,8:F3} мс | середній: {avgUs,8:F3} мкс");
        return result;
    }
    public static T Once<T>(string label, Func<T> query, int runs = 5)
    {
        T result = query(); 
        GC.Collect();       
        GC.WaitForPendingFinalizers();
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < runs; i++)
            result = query();
        sw.Stop();
        Console.WriteLine($"  {label,-52} {sw.Elapsed.TotalMilliseconds / runs,10:F3} мс");
        return result;
    }
}

static class Performance
{
    public static void Run()
    {
        var (venues, events) = DataSeed.Large(venueCount: 2_000, eventCount: 200_000);

        Console.WriteLine();
        Console.WriteLine(new string('=', 70));
        Console.WriteLine($"АНАЛІЗ ПРОДУКТИВНОСТІ: {events.Count:N0} подій, {venues.Count:N0} локацій");
        Console.WriteLine(new string('=', 70));

        Console.WriteLine("\n1) Відкладене виконання:");
        GC.Collect();
        var sw = Stopwatch.StartNew();
        var deferred = events.Where(e => e.Date.Year == 2024);
        sw.Stop();
        Console.WriteLine($"  {"оголошення запиту Where (без виконання)",-52} {sw.Elapsed.TotalMilliseconds,10:F3} мс");
        Bench.Once("виконання того ж запиту через ToList()", () => deferred.ToList());

        Console.WriteLine("\n2) Фільтрація (Where):");
        Bench.Once("Where(Year == 2024).ToList()", () => events.Where(e => e.Date.Year == 2024).ToList());

        Console.WriteLine("\n3) Об'єднання:");
        Bench.Once("Join (хеш-таблиця, O(n + m))", () =>
            events.Join(venues, e => e.LocationId, v => v.Id, (e, v) => v.Name).ToList());
        Bench.Once("Dictionary + Select (ручний хеш-join)", () =>
        {
            var byId = venues.ToDictionary(v => v.Id);
            return events.Select(e => byId[e.LocationId].Name).ToList();
        });
        Bench.Once("Select + First для кожної події (O(n * m))", () =>
            events.Select(e => venues.First(v => v.Id == e.LocationId).Name).ToList(), runs: 1);

        Console.WriteLine("\n4) Запит 1 на великих даних:");
        Bench.Once("Join, потім Where (як у запиті 1)", () =>
            events.Join(venues, e => e.LocationId, v => v.Id, (e, v) => new { e, v })
                  .Where(x => x.e.Date.Year == 2024 && x.v.Capacity > 500).Count());
        Bench.Once("Спершу Where, потім HashSet великих локацій", () =>
        {
            var bigVenueIds = venues.Where(v => v.Capacity > 500).Select(v => v.Id).ToHashSet();
            return events.Count(e => e.Date.Year == 2024 && bigVenueIds.Contains(e.LocationId));
        });

        Console.WriteLine("\n5) Агрегація (кількість подій на локацію):");
        Bench.Once("GroupJoin + Count", () =>
            venues.GroupJoin(events, v => v.Id, e => e.LocationId, (v, g) => g.Count()).ToList());
        Bench.Once("GroupBy + Count", () =>
            events.GroupBy(e => e.LocationId).Select(g => g.Count()).ToList());
        Bench.Once("Count(predicate) для кожної локації (O(n * m))", () =>
            venues.Take(200).Select(v => events.Count(e => e.LocationId == v.Id)).ToList(), runs: 1);
        Console.WriteLine("  (останній рядок — лише для 200 локацій із 2 000, інакше занадто довго)");

        Console.WriteLine("\n6) Пошук найближчої дати:");
        var from = new DateTime(2025, 1, 1);
        Bench.Once("OrderBy(Date).First() — сортування O(n log n)", () =>
            events.Where(e => e.Date >= from).OrderBy(e => e.Date).First());
        Bench.Once("MinBy(Date) — один прохід O(n)", () =>
            events.Where(e => e.Date >= from).MinBy(e => e.Date));

        Console.WriteLine("\n7) Перевірка наявності:");
        Bench.Once("Where(...).Count() > 0", () => events.Where(e => e.Date.Year == 2024).Count() > 0);
        Bench.Once("Any(...)", () => events.Any(e => e.Date.Year == 2024));
    }
}
