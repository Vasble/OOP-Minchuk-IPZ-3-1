using System.Diagnostics;

namespace IndependentWork19;

public static class Log
{
    private static readonly Stopwatch Clock = Stopwatch.StartNew();
    private static readonly object Sync = new();

    public static void Restart() => Clock.Restart();

    public static void Info(string message) => Write("INFO ", message, ConsoleColor.Gray);

    public static void Success(string message) => Write("OK   ", message, ConsoleColor.Green);

    public static void Warn(string message) => Write("WARN ", message, ConsoleColor.Yellow);

    public static void Error(string message) => Write("ERROR", message, ConsoleColor.Red);

    public static void Polly(string message) => Write("POLLY", message, ConsoleColor.Cyan);

    private static void Write(string level, string message, ConsoleColor color)
    {
        lock (Sync)
        {
            bool useColor = !Console.IsOutputRedirected;

            if (useColor)
            {
                Console.ForegroundColor = color;
            }

            Console.WriteLine($"[+{Clock.Elapsed.TotalSeconds,6:F2}s] {level} {message}");

            if (useColor)
            {
                Console.ResetColor();
            }
        }
    }
}
