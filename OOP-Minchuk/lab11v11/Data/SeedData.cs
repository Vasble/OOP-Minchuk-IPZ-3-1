using lab17v11.Models;

namespace lab17v11.Data;

public static class SeedData
{
    private static int _ticketId = 1;
    private static int _participantId = 1;

    private static Ticket T(string type, decimal price) =>
        new() { Id = _ticketId++, Type = type, Price = price };

    private static Participant P(string name, params Ticket[] tickets) =>
        new() { Id = _participantId++, Name = name, Tickets = tickets.ToList() };

    public static List<Event> GetEvents()
    {
        _ticketId = 1;
        _participantId = 1;

        return new List<Event>
        {
            new()
            {
                Id = 1, Name = "Рок-фестиваль «Захід»",
                Participants =
                {
                    P("Олександр Коваль", T("VIP", 2500m), T("Паркінг", 200m)),
                    P("Марія Шевченко", T("Стандарт", 900m)),
                    P("Іван Бондар", T("Стандарт", 900m), T("Кемпінг", 600m)),
                }
            },
            new()
            {
                Id = 2, Name = "IT-конференція DevFest",
                Participants =
                {
                    P("Андрій Мельник", T("Повний доступ", 3200m)),
                    P("Олена Ткаченко", T("Студентський", 800m)),
                    P("Дмитро Кравчук", T("Повний доступ", 3200m), T("Воркшоп", 1500m)),
                }
            },
            new()
            {
                Id = 3, Name = "Джазовий вечір",
                Participants =
                {
                    P("Юлія Савчук", T("Партер", 650m)),
                    P("Тарас Литвин", T("Балкон", 450m)),
                    P("Наталія Поліщук", T("Партер", 650m), T("Партер", 650m)),
                }
            },
            new()
            {
                Id = 4, Name = "Марафон «Рівне-21»",
                Participants =
                {
                    P("Сергій Гончар", T("Учасник 21 км", 500m)),
                    P("Ірина Мороз", T("Учасник 10 км", 350m), T("Футболка", 300m)),
                }
            },
            new()
            {
                Id = 5, Name = "Виставка сучасного мистецтва",
                Participants =
                {
                    P("Віктор Руденко", T("Вхідний", 150m)),
                    P("Катерина Лисенко", T("Екскурсія", 300m)),
                    P("Богдан Олійник", T("Вхідний", 150m), T("Каталог", 250m)),
                }
            },
            new()
            {
                Id = 6, Name = "Кіберспортивний турнір",
                Participants =
                {
                    P("Максим Захарченко", T("Глядач", 400m), T("Мерч", 700m)),
                    P("Анна Павленко", T("Глядач", 400m)),
                    P("Роман Кузьмич", T("Фан-зона", 1200m)),
                }
            },
            new()
            {
                Id = 7, Name = "Стендап-шоу",
                Participants =
                {
                    P("Софія Демчук", T("1 ряд", 1100m)),
                    P("Євген Марченко", T("Стандарт", 550m), T("Стандарт", 550m)),
                    P("Оксана Білик", T("Стандарт", 550m)),
                }
            },
        };
    }
}
