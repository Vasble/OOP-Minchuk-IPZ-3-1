# Лабораторна робота №12 — Обробка IO/мережевих помилок та патерн Retry

**Дисципліна:** Об'єктно-орієнтоване програмування
**Варіант:** 11 — «Отримання списку продуктів»
**Проєкт:** `lab18v11` (.NET 8, консольний застосунок)

## Мета

Навчитися обробляти типові помилки вводу/виводу та мережеві помилки за допомогою блоків `try-catch-finally`, а також реалізувати патерн Retry з експоненційною затримкою для підвищення відмовостійкості застосунку.

## Умова варіанту 11

| Клас | Метод | Імітована помилка |
|---|---|---|
| `FileProcessor` | `List<string> LoadProductNames(string path)` | `FileNotFoundException` перші **2** рази, потім успіх |
| `NetworkClient` | `List<string> GetProductsFromApi(string url)` | `HttpRequestException` перші **3** рази, потім успіх |

**shouldRetry:** повторювати лише для `FileNotFoundException` та `HttpRequestException`.

## Структура проєкту

```
lab18v11/
├── lab18v11.csproj
├── Program.cs                 # Main: 4 демонстраційні сценарії
├── Helpers/
│   └── RetryHelper.cs         # узагальнений ExecuteWithRetry<T>
└── Services/
    ├── FileProcessor.cs       # LoadProductNames(path)
    └── NetworkClient.cs       # GetProductsFromApi(url)
```

## Реалізація

### RetryHelper

```csharp
public static T ExecuteWithRetry<T>(
    Func<T> operation,
    int retryCount = 3,
    TimeSpan initialDelay = default,
    Func<Exception, bool>? shouldRetry = null)
```

- `operation` — операція, яку потрібно виконати;
- `retryCount` — максимальна кількість **повторних** спроб (усього спроб = `retryCount + 1`);
- `initialDelay` — початкова затримка (якщо не задано, 300 мс);
- `shouldRetry` — делегат, що вирішує, чи повторювати спробу для конкретного винятку (якщо `null`, то повторюються всі винятки).

Алгоритм:
1. Виконати `operation()`. Якщо успішно, повернути результат.
2. Якщо виник виняток, записати в лог номер спроби, тип і текст помилки.
3. Якщо `shouldRetry(ex) == false`, одразу передати виняток далі (`throw;`).
4. Якщо повтори вичерпано, теж передати виняток далі.
5. Інакше почекати `initialDelay * 2^retryAttempt` і повторити.

Кожна спроба логується з часовою міткою, тож у консолі видно, як зростає затримка.

### FileProcessor

Перші 2 виклики `LoadProductNames` викидають `FileNotFoundException`. Третій виклик **справді читає** файл `products.txt` через `File.ReadAllLines`. Якщо файлу немає, .NET сам викидає `FileNotFoundException`. Через конструктор можна підмінити тип помилки (використано в сценарії 3).

### NetworkClient

Перші 3 виклики `GetProductsFromApi` викидають `HttpRequestException` зі статусом `503 Service Unavailable`. Четвертий виклик повертає список із 5 продуктів.

## Сценарії в Main

| № | Сценарій | Очікуваний результат |
|---|---|---|
| 1 | `FileProcessor`: 2 × `FileNotFoundException` | 3 спроби, затримки 300 → 600 мс, список із 6 продуктів з файлу |
| 2 | `NetworkClient`: 3 × `HttpRequestException` | 4 спроби, затримки 300 → 600 → 1200 мс, список із 5 продуктів з API |
| 3 | `IOException` «файл заблоковано» | `shouldRetry` повертає `false`, тож повтору немає: 1 спроба, помилка передається одразу |
| 4 | Файлу `missing_products.txt` не існує | `FileNotFoundException` повторюється, але після 3 спроб (`retryCount: 2`) помилка передається далі |

Блок `finally` у кожному сценарії виводить кількість фактичних викликів методу. Блок `finally` у `Main` видаляє тимчасовий файл `products.txt` за будь-якого результату.

Фрагмент виводу (сценарій 2):

```
  [14:20:01.105] [Retry] Спроба 1/4...
  [14:20:01.207] [Retry] Спроба 1: невдача — HttpRequestException: Сервер ... тимчасово недоступний (імітація, запит №1).
  [14:20:01.208] [Retry] Очікування 300 мс перед наступною спробою (300 * 2^0).
  [14:20:01.509] [Retry] Спроба 2/4...
  ...
  [14:20:03.035] [Retry] Спроба 4: успіх.
```

## Висновок

Експоненційна затримка подвоює час очікування після кожної невдалої спроби за формулою `initialDelay * 2^retryAttempt`. У моєму варіанті при `initialDelay = 300 мс` це 300, 600 і 1200 мс. Так сервіс отримує дедалі більше часу на відновлення, а клієнт не перевантажує його частими запитами. Делегат `shouldRetry` обмежує повтори лише тимчасовими помилками (`FileNotFoundException`, `HttpRequestException`), тому постійні помилки, як-от заблокований файл, не повторюються марно.

## Відповіді на контрольні запитання

1. **Типові винятки.** Для файлів: `FileNotFoundException`, `DirectoryNotFoundException`, `IOException`, `UnauthorizedAccessException`. Для мережі: `HttpRequestException`, `SocketException`, `TaskCanceledException` (таймаут).
2. **Патерн Retry.** Операцію, що завершилась тимчасовою помилкою, автоматично повторюють обмежену кількість разів із паузою між спробами. Доцільний для збоїв, які зникають самі: короткочасна недоступність сервера, мережевий збій, тимчасово заблокований ресурс. Недоцільний для постійних помилок: неправильні дані, відсутність прав, 404.
3. **Експоненційна затримка.** Обчислюється як `delay = initialDelay * 2^retryAttempt`, де `retryAttempt` — номер повтору, починаючи з 0: `TimeSpan.FromMilliseconds(initialDelay.TotalMilliseconds * Math.Pow(2, retryAttempt))`, після чого `Thread.Sleep(delay)` (або `await Task.Delay(delay)` в асинхронному коді).
4. **Делегат shouldRetry.** Дозволяє ззовні визначити, які винятки вважати тимчасовими. Так `RetryHelper` залишається універсальним, а кожен виклик задає свою політику. Без нього повторювалися б і помилки, які повтор не виправить.
