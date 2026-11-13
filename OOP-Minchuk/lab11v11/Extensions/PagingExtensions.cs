namespace lab17v11.Extensions;

public static class PagingExtensions
{
    public static IEnumerable<T> Page<T>(this IEnumerable<T> source, int pageNumber, int pageSize)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (pageNumber < 1) throw new ArgumentOutOfRangeException(nameof(pageNumber), "Номер сторінки має бути >= 1.");
        if (pageSize < 1) throw new ArgumentOutOfRangeException(nameof(pageSize), "Розмір сторінки має бути >= 1.");

        return source
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);
    }

    public static int PageCount<T>(this IEnumerable<T> source, int pageSize)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (pageSize < 1) throw new ArgumentOutOfRangeException(nameof(pageSize));

        return (int)Math.Ceiling(source.Count() / (double)pageSize);
    }
}
