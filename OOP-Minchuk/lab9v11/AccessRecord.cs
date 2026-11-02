namespace Lab9v11;

public record AccessRecord(string Operation, string Key, string Result, DateTime Time)
{
    public override string ToString() =>
        $"[{Time:HH:mm:ss.fff}] {Operation,-6} {Key,-12} -> {Result}";
}
