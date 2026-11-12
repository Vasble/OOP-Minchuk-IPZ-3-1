using System.Collections.Concurrent;
using System.Diagnostics;

namespace IndependentWork12;

public static class SideEffectsExperiment
{
    public static void Run(List<int> data, int attempts)
    {
        Console.WriteLine("=== Експеримент 3: побічні ефекти в PLINQ ===");
        Console.WriteLine($"Завдання: порахувати прості числа серед {data.Count:N0} елементів і зібрати їх у список.");

        int expectedCount = data.Count(Workloads.IsPrime);
        Console.WriteLine($"Правильна відповідь (звичайний LINQ, один потік): {expectedCount:N0}");
        Console.WriteLine();

        Console.WriteLine("--- Проблема: спільні змінні змінюються з багатьох потоків без синхронізації ---");
        for (int attempt = 1; attempt <= attempts; attempt++)
        {
            int counter = 0;
            var primes = new List<int>();
            string listResult;

            try
            {
                data.AsParallel().Where(Workloads.IsPrime).ForAll(n =>
                {
                    counter++;
                    primes.Add(n);
                });

                listResult = $"{primes.Count:N0}";
            }
            catch (AggregateException ex)
            {
                listResult = $"виняток {ex.InnerExceptions[0].GetType().Name}";
            }

            Console.WriteLine($"Спроба {attempt}: counter++ = {counter,9:N0} | List.Add -> {listResult,-32} | " +
                              $"{(counter == expectedCount ? "збіглося випадково" : $"втрачено {expectedCount - counter:N0}")}");
        }

        Console.WriteLine();
        Console.WriteLine("--- Виправлення ---");

        Measure("1) lock для лічильника і списку", () =>
        {
            int counter = 0;
            var primes = new List<int>();
            var sync = new object();

            data.AsParallel().Where(Workloads.IsPrime).ForAll(n =>
            {
                lock (sync)
                {
                    counter++;
                    primes.Add(n);
                }
            });

            return (counter, primes.Count);
        }, expectedCount);

        Measure("2) Interlocked.Increment + ConcurrentBag<int>", () =>
        {
            int counter = 0;
            var primes = new ConcurrentBag<int>();

            data.AsParallel().Where(Workloads.IsPrime).ForAll(n =>
            {
                Interlocked.Increment(ref counter);
                primes.Add(n);
            });

            return (counter, primes.Count);
        }, expectedCount);

        Measure("3) без побічних ефектів: Count() і ToList()", () =>
        {
            List<int> primes = data.AsParallel().Where(Workloads.IsPrime).ToList();
            return (primes.Count, primes.Count);
        }, expectedCount);

        Console.WriteLine();
    }

    private static void Measure(string title, Func<(int Counter, int ListCount)> action, int expected)
    {
        var results = new List<(int Counter, int ListCount)>();
        var stopwatch = Stopwatch.StartNew();

        for (int i = 0; i < 3; i++)
        {
            results.Add(action());
        }

        stopwatch.Stop();

        bool allCorrect = results.All(r => r.Counter == expected && r.ListCount == expected);
        string values = string.Join(", ", results.Select(r => $"{r.Counter:N0}/{r.ListCount:N0}"));

        Console.WriteLine($"{title,-46} -> 3 запуски: {values} | {(allCorrect ? "завжди правильно" : "ПОМИЛКА")} | {stopwatch.Elapsed.TotalMilliseconds / 3,7:F1} мс");
    }
}
