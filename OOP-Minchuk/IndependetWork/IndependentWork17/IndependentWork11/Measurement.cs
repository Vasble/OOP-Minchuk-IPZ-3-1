using System.Diagnostics;

namespace IndependentWork11;

public record Measurement<T>(T Result, double FirstRunMs, double AverageWarmMs);

public static class Measure
{
    public static Measurement<T> Run<T>(Func<T> pipeline, int warmRuns = 50)
    {
        var firstRun = Stopwatch.StartNew();
        T result = pipeline();
        firstRun.Stop();

        var warm = Stopwatch.StartNew();
        for (int i = 0; i < warmRuns; i++)
        {
            pipeline();
        }
        warm.Stop();

        return new Measurement<T>(result, firstRun.Elapsed.TotalMilliseconds, warm.Elapsed.TotalMilliseconds / warmRuns);
    }

    public static double AverageMs(Action pipeline, int runs)
    {
        pipeline();

        var stopwatch = Stopwatch.StartNew();
        for (int i = 0; i < runs; i++)
        {
            pipeline();
        }
        stopwatch.Stop();

        return stopwatch.Elapsed.TotalMilliseconds / runs;
    }
}
