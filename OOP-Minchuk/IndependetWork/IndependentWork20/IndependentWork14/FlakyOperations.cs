namespace IndependentWork14;

public class LockedReportFile
{
    private readonly int _lockedAttempts;

    public LockedReportFile(int lockedAttempts)
    {
        _lockedAttempts = lockedAttempts;
    }

    public int Attempts { get; private set; }

    public string ReadAllText()
    {
        Attempts++;

        if (Attempts <= _lockedAttempts)
        {
            throw new IOException("Файл 'report.xlsx' зайнятий іншим процесом (Excel ще зберігає його).");
        }

        return "Звіт за листопад: 128 замовлень, 412 360 грн.";
    }
}

public class CollegeScheduleClient
{
    public int Attempts { get; private set; }

    public string DownloadSchedule(string group)
    {
        Attempts++;
        throw new HttpRequestException($"503 Service Unavailable — сайт розкладу не відповідає (група {group}).");
    }
}

public class SensorReader
{
    public int Attempts { get; private set; }

    public double ReadTemperature(string rawValue)
    {
        Attempts++;
        return double.Parse(rawValue, System.Globalization.CultureInfo.InvariantCulture);
    }
}
