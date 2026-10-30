namespace lab8v11;
public static class CollectionHelper<T>
{
    public static int Count(IEnumerable<T> collection, Predicate<T> predicate)
    {
        ArgumentNullException.ThrowIfNull(collection);
        ArgumentNullException.ThrowIfNull(predicate);

        int count = 0;
        foreach (T item in collection)
        {
            if (predicate(item))
            {
                count++;
            }
        }

        return count;
    }
}