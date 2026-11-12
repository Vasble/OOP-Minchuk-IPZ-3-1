using System.Runtime.InteropServices;
using IndependentWork12;

Console.OutputEncoding = System.Text.Encoding.UTF8;

bool quick = args.Contains("--quick");
int[] sizes = quick ? new[] { 1_000_000 } : new[] { 1_000_000, 5_000_000, 10_000_000 };
int repeats = quick ? 1 : 3;

Console.WriteLine("=== Середовище ===");
Console.WriteLine($".NET: {RuntimeInformation.FrameworkDescription}");
Console.WriteLine($"ОС: {RuntimeInformation.OSDescription}");
Console.WriteLine($"Логічних процесорів (Environment.ProcessorCount): {Environment.ProcessorCount}");
Console.WriteLine($"Конфігурація: {(IsDebug() ? "Debug" : "Release")}");
Console.WriteLine();

PerformanceExperiment.Run(sizes, repeats);
SideEffectsExperiment.Run(Workloads.CreateData(1_000_000, seed: 7), attempts: 5);

static bool IsDebug()
{
#if DEBUG
    return true;
#else
    return false;
#endif
}
