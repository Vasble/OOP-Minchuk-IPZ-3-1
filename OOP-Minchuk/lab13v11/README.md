# Лабораторна робота №13 — Власні винятки та патерн Result

**Дисципліна:** Об'єктно-орієнтоване програмування
**Варіант:** 11 — «Обробка платежів»
**Проєкт:** `lab19v11` (.NET 8, консольний застосунок)

## Мета

Навчитися створювати власні типи винятків для специфічних помилок, а також реалізовувати патерн Result. Він дає контрольовану обробку помилок без винятків у «нормальних» сценаріях невдачі.

## Умова варіанту 11

- **Власний виняток:** `PaymentGatewayException (string GatewayName, string ErrorCode)`.
- **Service:** `PaymentService`:
  - `void ProcessPayment(decimal amount)` — викидає `PaymentGatewayException`, якщо платіжний шлюз повернув помилку;
  - `Result<bool> RefundPayment(string transactionId)` — повертає `Result.Success` або `Result.Failure` (якщо транзакцію не знайдено).

## Структура проєкту

```
lab19v11/
├── lab19v11.csproj
├── Program.cs                          # Main: демонстрація обох підходів
├── Exceptions/
│   └── PaymentGatewayException.cs      # власний виняток
├── Common/
│   └── Result.cs                       # узагальнений Result<T>
├── Models/
│   └── Transaction.cs                  # транзакція (Id, Amount, CreatedAt, IsRefunded)
└── Services/
    └── PaymentService.cs               # ProcessPayment, RefundPayment, GetTransactions
```

## Реалізація

### PaymentGatewayException

Успадкований від `System.Exception`, має додаткові властивості `GatewayName` і `ErrorCode`. Реалізовано всі рекомендовані конструктори:

| Конструктор | Призначення |
|---|---|
| `()` | стандартне повідомлення |
| `(string message)` | власне повідомлення |
| `(string message, Exception innerException)` | обгортання іншого винятку |
| `(string gatewayName, string errorCode, string message[, Exception inner])` | конструктори з даними варіанту |
| `(SerializationInfo, StreamingContext)` + `GetObjectData` | серіалізація (у .NET 8 позначено `[Obsolete]`, як і в самому `Exception`) |

### Result&lt;T&gt;

```csharp
public class Result<T>
{
    public bool IsSuccess { get; }
    public T Value { get; }
    public string Error { get; }

    private Result(T value, bool isSuccess, string error) { ... }

    public static Result<T> Success(T value);
    public static Result<T> Failure(string error);
    public static Result<T> Failure(Exception ex);
}
```

Конструктор закритий, тому об'єкт можна створити лише через фабричні методи. Неможливо отримати «успіх з помилкою» чи «невдачу без опису».

### PaymentService

- **`ProcessPayment(decimal amount)`** імітує шлюз LiqPay. Він викидає `PaymentGatewayException` з кодом:
  - `LIMIT_EXCEEDED`, якщо сума перевищує ліміт 50 000 грн;
  - `GATEWAY_UNAVAILABLE`, якщо шлюз не відповідає (`IsGatewayAvailable = false`). Причину передано через `InnerException` (`TimeoutException`).

  На некоректну суму (≤ 0) метод викидає стандартний `ArgumentOutOfRangeException`, бо це помилка того, хто викликає метод, а не шлюзу. Успішний платіж створює транзакцію `TX-0001`, `TX-0002`, ...
- **`RefundPayment(string transactionId)`** повертає `Result<bool>.Failure`, якщо ідентифікатор порожній, транзакцію не знайдено або кошти вже повернено. В інших випадках повертає `Result<bool>.Success(true)`.
- **`GetTransactions()`** повертає список транзакцій лише для читання.

## Сценарії в Main

**1. ProcessPayment (try-catch):**

| Сума | Результат |
|---|---|
| 1 250.00 | успіх → `TX-0001` |
| 499.99 | успіх → `TX-0002` |
| 75 000.00 | `PaymentGatewayException`, код `LIMIT_EXCEEDED` |
| -100.00 | `ArgumentOutOfRangeException` |
| 300.00 (шлюз недоступний) | `PaymentGatewayException`, код `GATEWAY_UNAVAILABLE` + `InnerException` |
| 3 400.00 | успіх → `TX-0003` |

**2. RefundPayment (IsSuccess / Error):**

| Запит | Результат |
|---|---|
| `TX-0001` | `Success(True)` |
| `TX-0001` повторно | `Failure` — кошти вже повернено |
| `TX-9999` | `Failure` — транзакцію не знайдено |
| `""` | `Failure` — не вказано ідентифікатор |
| `TX-0003` | `Success(True)` |

Додатково показано `Result<T>.Failure(Exception)`.

**3. Підсумок:** 3 транзакції, сума без повернень 499.99 грн.

## Запуск

```bash
dotnet new console -o OOP-Minchuk/lab19v11   # створення проєкту (вже виконано)
cd OOP-Minchuk/lab19v11
dotnet run
```

## Висновок

Для `ProcessPayment` обрано **виняток**. Відмова зовнішнього платіжного шлюзу — виняткова ситуація, яку сервіс не може виправити сам. Її треба передати вище з деталями (`GatewayName`, `ErrorCode`, `InnerException`), щоб викликаючий код вирішив, що робити: повторити спробу, змінити шлюз чи повідомити користувача. Для `RefundPayment` обрано **Result&lt;T&gt;**. Неіснуюча або вже повернена транзакція — очікуваний бізнес-сценарій, тому його зручніше обробити звичайною перевіркою `IsSuccess`, без дорогого механізму винятків і без ризику забути `try-catch`.

## Відповіді на контрольні запитання

1. **Навіщо власні винятки?** Вони точно описують помилку предметної області (`PaymentGatewayException` замість загального `Exception`). Дозволяють перехоплювати саме цей тип у `catch`. Несуть додаткові дані (`GatewayName`, `ErrorCode`) і роблять код та логи зрозумілішими.
2. **Стандартні конструктори:** без параметрів, `(string message)`, `(string message, Exception innerException)`. Для старих версій .NET також захищений конструктор серіалізації `(SerializationInfo, StreamingContext)`.
3. **Патерн Result.** Метод повертає об'єкт, що містить або значення, або опис помилки. Доцільний для очікуваних невдач (валідація, «не знайдено», бізнес-правила): вони трапляються часто, їх треба обробити явно, а винятки для них дорогі й ускладнюють потік виконання.
4. **Винятки чи Result?** Винятки — для непередбачуваних і системних збоїв, які не можна обробити на місці: недоступний зовнішній сервіс, помилка IO, некоректний аргумент від програміста. Result — для передбачуваних результатів бізнес-логіки, які є нормальною частиною роботи програми: невалідні дані, відсутній запис, повторна операція.
