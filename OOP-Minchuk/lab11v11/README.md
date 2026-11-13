# Лабораторна робота №11 — LINQ: SelectMany, пагінація й custom extensions

**Дисципліна:** Об'єктно-орієнтоване програмування
**Варіант:** 11 — «Події та їхні учасники з квитками»
**Проєкт:** `lab17v11` (.NET 8, консольний застосунок)

## Мета

Навчитися використовувати оператор `SelectMany` для роботи з вкладеними колекціями, реалізовувати пагінацію даних за допомогою LINQ (`Skip` / `Take`) та створювати власні методи розширення для `IEnumerable<T>`.

## Предметна область

| Сутність | Поля | Опис |
|---|---|---|
| `Event` | `Id`, `Name`, `List<Participant> Participants` | Подія (фестиваль, конференція, концерт тощо) |
| `Participant` | `Id`, `Name`, `List<Ticket> Tickets` | Учасник події, може мати кілька квитків |
| `Ticket` | `Id`, `Type`, `Price` | Квиток певного типу з ціною |

Структура вкладеності: **Event → Participant → Ticket**.

Тестові дані (`Data/SeedData.cs`): 7 подій, 20 учасників, 28 квитків.

## Структура проєкту

```
lab17v11/
├── lab17v11.csproj
├── Program.cs                     # Main: демонстрація всіх запитів
├── Models/
│   ├── Event.cs
│   ├── Participant.cs
│   └── Ticket.cs
├── Data/
│   └── SeedData.cs                # тестові дані
└── Extensions/
    ├── EventExtensions.cs         # TotalTicketRevenue(), TicketRevenue()
    └── PagingExtensions.cs        # Page<T>(), PageCount<T>()
```

## Реалізовані запити

### 1. SelectMany — усі квитки всіх учасників усіх подій

Два послідовні `SelectMany` «розгортають» дворівневу вкладеність в одну плоску послідовність:

```csharp
List<Ticket> allTickets = events
    .SelectMany(e => e.Participants)
    .SelectMany(p => p.Tickets)
    .ToList();
```

Додатково показано перевантаження `SelectMany` з `resultSelector`, яке зберігає зв'язок квитка з подією та учасником (назва події, ім'я учасника, тип і ціна квитка).

### 2. Пагінація — події по 2 на сторінку

Пагінацію винесено в узагальнений метод розширення, тому його можна застосувати до будь-якої колекції:

```csharp
public static IEnumerable<T> Page<T>(this IEnumerable<T> source, int pageNumber, int pageSize)
    => source.Skip((pageNumber - 1) * pageSize).Take(pageSize);
```

Для 7 подій і розміру сторінки 2 виходить 4 сторінки (остання містить одну подію). Кількість сторінок обчислює `PageCount<T>()`.

### 3. Custom Extension — `TotalTicketRevenue()`

Метод розширення для `IEnumerable<Event>`, що обчислює загальний дохід від усіх квитків:

```csharp
public static decimal TotalTicketRevenue(this IEnumerable<Event> events)
    => events
        .SelectMany(e => e.Participants)
        .SelectMany(p => p.Tickets)
        .Sum(t => t.Price);
```

Оскільки метод розширює `IEnumerable<Event>`, його можна викликати на будь-якій вибірці, наприклад на двох найприбутковіших подіях:

```csharp
events.OrderByDescending(e => e.TicketRevenue()).Take(2).TotalTicketRevenue();
```

## Запуск

```bash
dotnet new console -o OOP-Minchuk/lab17v11   # створення проєкту (вже виконано)
cd OOP-Minchuk/lab17v11
dotnet run
```

## Очікуваний результат

- **Запит 1:** список із 28 квитків і таблиця перших 5 квитків з назвою події та учасником.
- **Запит 2:** 4 сторінки подій (2 + 2 + 2 + 1).
- **Запит 3:** дохід по кожній події та підсумок:

| Подія | Дохід, грн |
|---|---:|
| Рок-фестиваль «Захід» | 5 100.00 |
| IT-конференція DevFest | 8 700.00 |
| Джазовий вечір | 2 400.00 |
| Марафон «Рівне-21» | 1 150.00 |
| Виставка сучасного мистецтва | 850.00 |
| Кіберспортивний турнір | 2 700.00 |
| Стендап-шоу | 2 750.00 |
| **Разом** | **23 650.00** |

Дохід двох найприбутковіших подій: **11 450.00 грн**.

## Висновок

У ході роботи використано оператор `SelectMany` для перетворення вкладених колекцій «подія → учасники → квитки» в одну плоску послідовність, зокрема з `resultSelector` для збереження контексту. Пагінацію реалізовано через `Skip` і `Take` в узагальненому методі розширення `Page<T>()`. Створено власний метод розширення `TotalTicketRevenue()` для `IEnumerable<Event>`, який інкапсулює бізнес-логіку підрахунку доходу та працює з будь-якою вибіркою подій.
