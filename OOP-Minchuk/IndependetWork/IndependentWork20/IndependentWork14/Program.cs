using System.Diagnostics;
using IndependentWork14;

Console.OutputEncoding = System.Text.Encoding.UTF8;

var policy = new RetryPolicy(maxRetries: 4, initialDelay: TimeSpan.FromMilliseconds(250), jitterFactor: 0.25);

static bool IsTransient(Exception ex) => ex is IOException or HttpRequestException or TimeoutException;

Console.WriteLine($"RetryPolicy: maxRetries = {policy.MaxRetries}, initialDelay = {policy.InitialDelay.TotalMilliseconds} мс, jitterFactor = {policy.JitterFactor}");
Console.WriteLine("Формула: delay = initialDelay * 2^retryAttempt * (1 + random * jitterFactor * 2 - jitterFactor)");
Console.WriteLine();

Console.WriteLine("=== Сценарій 1: файл звіту тимчасово заблокований — успіх після кількох спроб ===");
var file = new LockedReportFile(lockedAttempts: 3);
var stopwatch = Stopwatch.StartNew();
string report = policy.Execute(file.ReadAllText, IsTransient);
Console.WriteLine($"Результат: \"{report}\" (спроб: {file.Attempts}, час: {stopwatch.ElapsedMilliseconds} мс)");
Console.WriteLine();

Console.WriteLine("=== Сценарій 2: сайт розкладу недоступний — остаточна невдача після всіх спроб ===");
var schedule = new CollegeScheduleClient();
stopwatch.Restart();
try
{
    policy.Execute(() => schedule.DownloadSchedule("ІПЗ-21"), IsTransient);
}
catch (HttpRequestException ex)
{
    Console.WriteLine($"Main перехопив остаточний виняток: {ex.GetType().Name} — {ex.Message}");
    Console.WriteLine($"Спроб: {schedule.Attempts}, загальний час очікування: {stopwatch.ElapsedMilliseconds} мс. Показуємо збережену копію розкладу.");
}
Console.WriteLine();

Console.WriteLine("=== Сценарій 3: помилка формату даних — shouldRetry забороняє повтор ===");
var sensor = new SensorReader();
try
{
    policy.Execute(() => sensor.ReadTemperature("23,5°C"), IsTransient);
}
catch (FormatException ex)
{
    Console.WriteLine($"Main перехопив: {ex.GetType().Name} — {ex.Message} (спроб: {sensor.Attempts})");
}
Console.WriteLine();

Console.WriteLine("=== Сценарій 4: без shouldRetry повторюється будь-який виняток (Execute для Action) ===");
int calls = 0;
policy.Execute(() =>
{
    calls++;
    if (calls < 2)
    {
        throw new InvalidOperationException("Черга повідомлень перевантажена.");
    }
    Console.WriteLine("  Повідомлення надіслано в чергу.");
});
Console.WriteLine();

Console.WriteLine("=== Перевірка формули: 1000 обчислень затримки для кожного retryAttempt ===");
var statsPolicy = new RetryPolicy(4, TimeSpan.FromMilliseconds(250), 0.25, new Random(42), _ => { });
Console.WriteLine($"{"retryAttempt",12} | {"База, мс",9} | {"Очікуваний діапазон, мс",24} | {"Мін, мс",8} | {"Сер, мс",8} | {"Макс, мс",8}");
Console.WriteLine(new string('-', 84));
for (int retryAttempt = 0; retryAttempt < 4; retryAttempt++)
{
    double baseMs = 250 * Math.Pow(2, retryAttempt);
    double[] samples = Enumerable.Range(0, 1000)
        .Select(_ => statsPolicy.CalculateDelay(retryAttempt).TotalMilliseconds)
        .ToArray();

    string range = $"{baseMs * 0.75:F0} – {baseMs * 1.25:F0}";
    Console.WriteLine($"{retryAttempt,12} | {baseMs,9:F0} | {range,24} | {samples.Min(),8:F1} | {samples.Average(),8:F1} | {samples.Max(),8:F1}");
}
