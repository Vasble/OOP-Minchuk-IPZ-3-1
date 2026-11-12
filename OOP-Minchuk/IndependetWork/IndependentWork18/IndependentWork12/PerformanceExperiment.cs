using System.Diagnostics;

namespace IndependentWork12;

public static class PerformanceExperiment
{
    public static void Run(int[] sizes, int repeats)
    {
        Console.WriteLine("=== Експеримент 1: LINQ vs PLINQ на обчислювально інтенсивній операції ===");
        Console.WriteLine("Операція: Where(IsPrime) — перевірка на просте число перебором дільників до √n,");
        Console.WriteLine("          Select(HeavyTransform) — 30 ітерацій Sqrt/Log для кожного простого числа.");
        Console.WriteLine($"Кожен вимір — найкращий час з {repeats} повторів (після прогріву).");
        Console.WriteLine();

        Warmup();

        Console.WriteLine($"{"Елементів",11} | {"LINQ, мс",10} | {"PLINQ, мс",10} | {"Прискорення",11} | {"Простих",9} | Результати однакові");
        Console.WriteLine(new string('-', 82));

        foreach (int size in sizes)
        {
            List<int> data = Workloads.CreateData(size);

            (double linqMs, (int Count, double Sum) linqResult) = Best(() => Workloads.HeavyLinq(data), repeats);
            (double plinqMs, (int Count, double Sum) plinqResult) = Best(() => Workloads.HeavyPlinq(data), repeats);

            bool same = linqResult.Count == plinqResult.Count &&
                        Math.Abs(linqResult.Sum - plinqResult.Sum) < 1e-6 * Math.Abs(linqResult.Sum);

            Console.WriteLine($"{size,11:N0} | {linqMs,10:F1} | {plinqMs,10:F1} | {linqMs / plinqMs,10:F2}x | {linqResult.Count,9:N0} | {(same ? "так" : "НІ")}");
        }

        Console.WriteLine();
        Console.WriteLine("=== Експеримент 2: LINQ vs PLINQ на дешевій операції (n % 3 == 0, n * 2, Sum) ===");
        Console.WriteLine($"{"Елементів",11} | {"LINQ, мс",10} | {"PLINQ, мс",10} | {"Прискорення",11} | Результати однакові");
        Console.WriteLine(new string('-', 70));

        foreach (int size in new[] { 10_000 }.Concat(sizes))
        {
            List<int> data = Workloads.CreateData(size);

            (double linqMs, long linqSum) = Best(() => Workloads.CheapLinq(data), repeats);
            (double plinqMs, long plinqSum) = Best(() => Workloads.CheapPlinq(data), repeats);

            Console.WriteLine($"{size,11:N0} | {linqMs,10:F3} | {plinqMs,10:F3} | {linqMs / plinqMs,10:F2}x | {(linqSum == plinqSum ? "так" : "НІ")}");
        }

        Console.WriteLine();
    }

    private static void Warmup()
    {
        List<int> small = Workloads.CreateData(20_000, seed: 1);
        Workloads.HeavyLinq(small);
        Workloads.HeavyPlinq(small);
        Workloads.CheapLinq(small);
        Workloads.CheapPlinq(small);
    }

    private static (double Milliseconds, T Result) Best<T>(Func<T> action, int repeats)
    {
        double best = double.MaxValue;
        T result = default!;

        for (int i = 0; i < repeats; i++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();

            var stopwatch = Stopwatch.StartNew();
            result = action();
            stopwatch.Stop();

            best = Math.Min(best, stopwatch.Elapsed.TotalMilliseconds);
        }

        return (best, result);
    }
}
