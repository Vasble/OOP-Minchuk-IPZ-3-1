namespace lab19v11.Common;

public class Result<T>
{
    public bool IsSuccess { get; }
    public T Value { get; }
    public string Error { get; }

    public bool IsFailure => !IsSuccess;

    private Result(T value, bool isSuccess, string error)
    {
        Value = value;
        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result<T> Success(T value) =>
        new(value, true, string.Empty);

    public static Result<T> Failure(string error)
    {
        if (string.IsNullOrWhiteSpace(error))
            throw new ArgumentException("Опис помилки не може бути порожнім.", nameof(error));

        return new(default!, false, error);
    }

    public static Result<T> Failure(Exception ex)
    {
        ArgumentNullException.ThrowIfNull(ex);
        return new(default!, false, $"{ex.GetType().Name}: {ex.Message}");
    }

    public override string ToString() =>
        IsSuccess ? $"Success({Value})" : $"Failure(\"{Error}\")";
}
