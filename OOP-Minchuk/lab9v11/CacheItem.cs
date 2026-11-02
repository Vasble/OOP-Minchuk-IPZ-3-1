namespace Lab9v11;

public class CacheItem
{
    public string Key { get; }
    public string Value { get; set; }
    public DateTime UpdatedAt { get; set; }

    public CacheItem(string key, string value)
    {
        Key = key;
        Value = value;
        UpdatedAt = DateTime.Now;
    }

    public override string ToString() => $"{Key} = \"{Value}\"";
}
