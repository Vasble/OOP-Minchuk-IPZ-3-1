# Самостійна робота №13 — узагальнений кеш `Cache<TKey, TValue>`

**Дисципліна:** Об'єктно-орієнтоване програмування
**Тема:** Узагальнений кеш `Cache<TKey, TValue>` із тестами

## Мета

Розробити узагальнений клас кешу, який зберігає дані за ключем і має обмежену ємність.
Коли кеш заповнений, з нього видаляється найстаріший елемент (політика **FIFO**, First In, First Out).
Роботу класу перевіряють юніт-тести.

## Структура репозиторію

```
IndependentWork7/
├── IndependentWork7.sln
├── IndependentWork7/               # консольний проєкт
│   ├── Cache.cs                    # клас Cache<TKey, TValue>
│   └── Program.cs                  # демонстрація роботи в Main
└── IndependentWork7.Tests/         # проєкт юніт-тестів (xUnit)
    └── CacheTests.cs               # тести для Cache<TKey, TValue>
```

Тестовий проєкт містить посилання на основний проєкт (`ProjectReference`).

## Реалізація

### Приватні поля

| Поле | Тип | Призначення |
|------|-----|-------------|
| `_capacity` | `int` | Максимальна кількість елементів у кеші |
| `_cache` | `Dictionary<TKey, TValue>` | Зберігання пар «ключ — значення», швидкий пошук за O(1) |
| `_keysOrder` | `Queue<TKey>` | Порядок додавання ключів; на початку черги завжди найстаріший ключ |

### Публічні члени

| Член | Опис |
|------|------|
| `Cache(int capacity)` | Створює кеш заданої ємності. Якщо `capacity <= 0`, викидає `ArgumentOutOfRangeException` |
| `void Add(TKey key, TValue value)` | Додає елемент. Якщо кеш повний, спочатку робить `Dequeue()` і видаляє найстаріший ключ зі словника |
| `TValue Get(TKey key)` | Повертає значення через `TryGetValue`. Якщо ключа немає, викидає `KeyNotFoundException` |
| `bool ContainsKey(TKey key)` | Перевіряє, чи є ключ у кеші |
| `int Count { get; }` | Поточна кількість елементів |
| `int Capacity { get; }` | Максимальна ємність (додаткова властивість) |

### Як працює FIFO

1. Новий ключ записується в словник і ставиться в кінець черги.
2. Якщо під час додавання `Count == capacity`, з початку черги береться найстаріший ключ (`Dequeue()`) і видаляється зі словника.
3. Якщо додати вже наявний ключ, значення оновлюється, а позиція ключа в черзі не змінюється. Так кеш не дублює ключ і не порушує порядок FIFO.
4. Метод `Get` не змінює порядок витіснення. Саме цим FIFO відрізняється від LRU.

Приклад для ємності 3:

```
Add(a) Add(b) Add(c)  ->  [a, b, c]
Add(d)                ->  a витіснено  ->  [b, c, d]
Add(e)                ->  b витіснено  ->  [c, d, e]
```

## Юніт-тести

Тести написані на **xUnit** у файлі `IndependentWork7.Tests/CacheTests.cs`.

| Що перевіряється | Тести |
|------------------|-------|
| Конструктор | `Constructor_NewCache_IsEmpty`, `Constructor_NonPositiveCapacity_ThrowsArgumentOutOfRangeException` (0 і −5) |
| Додавання (`Add`) | `Add_SingleItem_ItemIsStored`, `Add_ExistingKey_UpdatesValueWithoutIncreasingCount` |
| Отримання (`Get`) | `Get_ExistingKey_ReturnsCorrectValue` |
| `KeyNotFoundException` | `Get_MissingKey_ThrowsKeyNotFoundException`, `Get_EmptyCache_ThrowsKeyNotFoundException` |
| `ContainsKey` | `ContainsKey_ExistingKey_ReturnsTrue`, `ContainsKey_MissingKey_ReturnsFalse` |
| `Count` | `Count_AfterAddingItems_EqualsNumberOfItems`, `Count_AfterOverflow_NeverExceedsCapacity` |
| Переповнення кешу (FIFO) | `Add_WhenFull_RemovesOldestItem`, `Add_SeveralOverflows_RemovesItemsInInsertionOrder`, `Add_WhenFull_GetOfEvictedKeyThrowsKeyNotFoundException`, `Get_DoesNotChangeEvictionOrder_FifoNotLru`, `Add_UpdatingExistingKey_DoesNotMoveItInQueue` |

Разом 17 тестових випадків, усі проходять.

## Запуск

Потрібен .NET 8 SDK.

```bash
# запуск демонстрації
dotnet run --project IndependentWork7

# запуск усіх тестів
dotnet test
```

### Приклад виводу програми

```
=== Демонстрація Cache<string, string> (FIFO) ===
Створено кеш ємністю 3.
Додано 3 елементи. Count = 3
Get("ua") -> Україна
Get("pl") -> Польща

Додаємо "fr" — кеш повний, тому видаляється найстаріший ("ua").
  ContainsKey("ua"): немає
  ContainsKey("pl"): є -> Польща
  ContainsKey("de"): є -> Німеччина
  ContainsKey("fr"): є -> Франція
  Count = 3

Додаємо "it" — тепер видаляється "pl".
  ContainsKey("pl"): немає
  ContainsKey("de"): є -> Німеччина
  ContainsKey("fr"): є -> Франція
  ContainsKey("it"): є -> Італія
  Count = 3

Спроба отримати видалений ключ "ua":
KeyNotFoundException: Ключ 'ua' не знайдено в кеші.

=== Демонстрація Cache<int, Student> ===
Студент #1: немає в кеші
Студент #2: Андрій, середній бал 4.2
Студент #3: Марія, середній бал 4.5
Count = 2
```

## Відповіді на контрольні запитання

**1. Які переваги надає `Cache<TKey, TValue>` порівняно з окремими неузагальненими реалізаціями кешу?**
Один клас працює з будь-якими типами ключів і значень, тому код не треба дублювати для кожного типу.
Компілятор перевіряє типи, тож не потрібні приведення з `object`, а помилки з типами видно ще під час компіляції.
Для значущих типів (`int`, `double`, `struct`) немає упаковки й розпаковки (boxing/unboxing), тому кеш працює швидше.
Логіку FIFO достатньо протестувати й виправляти лише в одному місці.

**2. Чому для реалізації FIFO зручно поєднувати `Dictionary<TKey, TValue>` з `Queue<TKey>` або `LinkedList<TKey>`?**
`Dictionary` дає швидкий пошук значення за ключем (O(1)), але не зберігає порядок додавання.
`Queue` зберігає саме цей порядок: найстаріший ключ завжди на початку, і `Dequeue()` повертає його за O(1).
Разом вони дають швидкі й додавання, й пошук, і витіснення.
`LinkedList` потрібен для LRU: вузол можна перемістити в кінець списку за O(1), коли до елемента звертаються.

**3. Коли `Get` повинен викидати `KeyNotFoundException`, а коли краще підхід на кшталт `TryGetValue`?**
Виняток доречний, коли відсутність ключа означає помилку в логіці програми, тобто викликач упевнений, що ключ має бути в кеші.
У кеші ж відсутність ключа — звичайна ситуація (cache miss): елемент міг бути витіснений.
Тоді краще `bool TryGet(TKey key, out TValue value)`: він не створює виняток (а винятки «дорогі») і змушує викликача обробити обидва випадки.
Тому всередині `Get` використано `TryGetValue`, а виняток викидається явно, з власним повідомленням.

**4. Які тести критично важливі для перевірки політики витіснення?**
- Після переповнення видаляється саме найстаріший елемент, а новіші залишаються.
- Під час кількох переповнень поспіль елементи видаляються в порядку додавання.
- `Count` ніколи не перевищує ємність.
- `Get` для витісненого ключа викидає `KeyNotFoundException`.
- Граничний випадок із ємністю 1.
- Звернення через `Get` не змінює порядок витіснення (різниця між FIFO і LRU).
- Повторне додавання наявного ключа не дублює його в черзі й не порушує порядок.

## Висновок

Реалізовано узагальнений клас `Cache<TKey, TValue>` з обмеженою ємністю та політикою витіснення FIFO на основі `Dictionary<TKey, TValue>` і `Queue<TKey>`.
Програма демонструє роботу кешу з типами `Cache<string, string>` та `Cache<int, Student>`.
Функціональність покрито 17 юніт-тестами xUnit (додавання, отримання, `ContainsKey`, `Count`, переповнення, `KeyNotFoundException`), і всі вони проходять.
