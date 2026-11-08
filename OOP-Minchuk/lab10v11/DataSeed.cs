namespace lab10v11;

public static class DataSeed
{
    public static List<Venue> Venues() => new()
    {
        new() { Id = 1,  Name = "Palace of Sports",        Capacity = 7000 },
        new() { Id = 2,  Name = "Atlas Club",              Capacity = 1500 },
        new() { Id = 3,  Name = "Caribbean Club",          Capacity = 450  },
        new() { Id = 4,  Name = "Freedom Hall",            Capacity = 2000 },
        new() { Id = 5,  Name = "Coffee Lab Loft",         Capacity = 80   },
        new() { Id = 6,  Name = "National Opera",          Capacity = 1300 },
        new() { Id = 7,  Name = "IT Hub Rivne",            Capacity = 200  },
        new() { Id = 8,  Name = "City Library Hall",       Capacity = 120  },
        new() { Id = 9,  Name = "Olympic Stadium",         Capacity = 70000 },
        new() { Id = 10, Name = "Expo Center",             Capacity = 5000 },
        new() { Id = 11, Name = "Drama Theatre",           Capacity = 600  },
        new() { Id = 12, Name = "Student Coworking",       Capacity = 50   }, // без подій
    };

    public static List<Event> Events() => new()
    {
        new() { Id = 1,  Name = ".NET Conference",          Date = new(2024, 3, 15),  LocationId = 10 },
        new() { Id = 2,  Name = "Rock Fest",                Date = new(2024, 6, 22),  LocationId = 9  },
        new() { Id = 3,  Name = "Jazz Evening",             Date = new(2024, 2, 10),  LocationId = 3  },
        new() { Id = 4,  Name = "Startup Pitch Night",      Date = new(2024, 9, 5),   LocationId = 7  },
        new() { Id = 5,  Name = "Swan Lake",                Date = new(2024, 12, 28), LocationId = 6  },
        new() { Id = 6,  Name = "Basketball Final",         Date = new(2024, 5, 18),  LocationId = 1  },
        new() { Id = 7,  Name = "Poetry Reading",           Date = new(2024, 4, 2),   LocationId = 8  },
        new() { Id = 8,  Name = "Electronic Night",         Date = new(2023, 11, 11), LocationId = 2  },
        new() { Id = 9,  Name = "Book Fair",                Date = new(2023, 9, 9),   LocationId = 10 },
        new() { Id = 10, Name = "Hamlet",                   Date = new(2024, 10, 12), LocationId = 11 },
        new() { Id = 11, Name = "Indie Showcase",           Date = new(2024, 8, 30),  LocationId = 4  },
        new() { Id = 12, Name = "Barista Workshop",         Date = new(2024, 7, 7),   LocationId = 5  },
        new() { Id = 13, Name = "Football Cup",             Date = new(2025, 6, 1),   LocationId = 9  },
        new() { Id = 14, Name = "AI Meetup",                Date = new(2025, 11, 20), LocationId = 7  },
        new() { Id = 15, Name = "Winter Gala",              Date = new(2026, 12, 19), LocationId = 6  },
        new() { Id = 16, Name = "Hackathon 48h",            Date = new(2026, 11, 21), LocationId = 10 },
        new() { Id = 17, Name = "New Year Concert",         Date = new(2026, 12, 31), LocationId = 1  },
        new() { Id = 18, Name = "Spring Comic Con",         Date = new(2027, 4, 10),  LocationId = 4  },
        new() { Id = 19, Name = "Chamber Music",            Date = new(2026, 11, 14), LocationId = 11 },
        new() { Id = 20, Name = "Game Dev Day",             Date = new(2027, 2, 6),   LocationId = 2  },
    };

    public static (List<Venue> venues, List<Event> events) Large(int venueCount, int eventCount)
    {
        var rnd = new Random(42); 
        var venues = Enumerable.Range(1, venueCount)
            .Select(i => new Venue { Id = i, Name = $"Venue {i}", Capacity = rnd.Next(50, 10_000) })
            .ToList();
        var start = new DateTime(2022, 1, 1);
        var events = Enumerable.Range(1, eventCount)
            .Select(i => new Event
            {
                Id = i,
                Name = $"Event {i}",
                Date = start.AddDays(rnd.Next(0, 365 * 6)),
                LocationId = rnd.Next(1, venueCount + 1)
            })
            .ToList();
        return (venues, events);
    }
}
