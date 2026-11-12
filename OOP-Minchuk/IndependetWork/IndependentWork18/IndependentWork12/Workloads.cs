namespace IndependentWork12;

public static class Workloads
{
    public static bool IsPrime(int n)
    {
        if (n < 2)
        {
            return false;
        }

        if (n % 2 == 0)
        {
            return n == 2;
        }

        for (int divisor = 3; divisor * divisor <= n; divisor += 2)
        {
            if (n % divisor == 0)
            {
                return false;
            }
        }

        return true;
    }

    public static double HeavyTransform(int n)
    {
        double value = n;

        for (int i = 1; i <= 30; i++)
        {
            value = Math.Sqrt(value + i) * Math.Log(value + 2) / Math.Log(i + 2);
        }

        return value;
    }

    public static List<int> CreateData(int count, int seed = 12)
    {
        var random = new Random(seed);
        var data = new List<int>(count);

        for (int i = 0; i < count; i++)
        {
            data.Add(random.Next(1, 10_000_000));
        }

        return data;
    }

    public static (int Count, double Sum) HeavyLinq(List<int> data)
    {
        List<double> result = data
            .Where(IsPrime)
            .Select(HeavyTransform)
            .ToList();

        return (result.Count, result.Sum());
    }

    public static (int Count, double Sum) HeavyPlinq(List<int> data)
    {
        List<double> result = data
            .AsParallel()
            .Where(IsPrime)
            .Select(HeavyTransform)
            .ToList();

        return (result.Count, result.Sum());
    }

    public static long CheapLinq(List<int> data) =>
        data.Where(n => n % 3 == 0).Select(n => (long)n * 2).Sum();

    public static long CheapPlinq(List<int> data) =>
        data.AsParallel().Where(n => n % 3 == 0).Select(n => (long)n * 2).Sum();
}
