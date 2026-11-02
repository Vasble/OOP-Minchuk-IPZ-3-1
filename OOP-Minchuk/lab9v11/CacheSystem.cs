namespace Lab9v11;

public class CacheSystem
{
    private readonly int _capacity;
    private readonly Dictionary<string, CacheItem> _storage = new();

    private readonly HashSet<string> _keys = new();
    private Queue<string> _evictionQueue = new();
    private readonly Stack<AccessRecord> _history = new();

    public int Capacity => _capacity;
    public int Count => _keys.Count;
    public int HistoryCount => _history.Count;

    public CacheSystem(int capacity)
    {
        if (capacity <= 0)
            throw new ArgumentOutOfRangeException(nameof(capacity), "Місткість має бути більшою за 0.");
        _capacity = capacity;
    }

    public bool ContainsKey(string key) => _keys.Contains(key);

    public HashSet<string> GetKeys() => new(_keys);

    public HashSet<string> CommonKeys(IEnumerable<string> other)
    {
        var result = new HashSet<string>(_keys);
        result.IntersectWith(other);
        return result;
    }
    public HashSet<string> MissingKeys(IEnumerable<string> requested)
    {
        var result = new HashSet<string>(requested);
        result.ExceptWith(_keys);
        return result;
    }
    public void Put(string key, string value)
    {
        if (_keys.Contains(key))
        {
            _storage[key].Value = value;
            _storage[key].UpdatedAt = DateTime.Now;
            MarkAsRecentlyUsed(key);
            Log("PUT", key, "оновлено");
            Console.WriteLine($"  PUT {key}: ключ уже існує — значення оновлено.");
            return;
        }

        if (_keys.Count >= _capacity)
        {
            Console.WriteLine($"  Кеш заповнений ({_keys.Count}/{_capacity}) — потрібне видалення.");
            EvictLeastRecentlyUsed();
        }

        bool added = _keys.Add(key);          
        _storage[key] = new CacheItem(key, value);
        _evictionQueue.Enqueue(key);     
        Log("PUT", key, "додано");
        Console.WriteLine($"  PUT {key}: додано (HashSet.Add = {added}).");
    }

    public string? Get(string key)
    {
        if (!_keys.Contains(key))
        {
            Log("GET", key, "MISS");
            Console.WriteLine($"  GET {key}: MISS (ключа немає в кеші).");
            return null;
        }

        MarkAsRecentlyUsed(key);
        Log("GET", key, "HIT");
        string value = _storage[key].Value;
        Console.WriteLine($"  GET {key}: HIT -> \"{value}\"");
        return value;
    }

    public bool Remove(string key)
    {
        bool removed = _keys.Remove(key);    
        if (removed)
        {
            _storage.Remove(key);
            RemoveFromQueue(key);
            Log("DEL", key, "видалено");
        }
        else
        {
            Log("DEL", key, "не знайдено");
        }
        Console.WriteLine($"  DEL {key}: {(removed ? "видалено" : "ключа не було")}.");
        return removed;
    }

    public string? PeekNextToEvict() =>
        _evictionQueue.Count > 0 ? _evictionQueue.Peek() : null;

    public string? EvictLeastRecentlyUsed()
    {
        if (_evictionQueue.Count == 0)
            return null;

        string victim = _evictionQueue.Dequeue(); 
        _keys.Remove(victim);
        _storage.Remove(victim);
        Log("EVICT", victim, "витіснено (LRU)");
        Console.WriteLine($"  EVICT {victim}: видалено з початку черги (найдавніше використаний).");
        return victim;
    }

    public IEnumerable<string> EvictionOrder => _evictionQueue;

    private void MarkAsRecentlyUsed(string key)
    {
        RemoveFromQueue(key);
        _evictionQueue.Enqueue(key);
    }

    private void RemoveFromQueue(string key)
    {
        var rebuilt = new Queue<string>(_evictionQueue.Count);
        foreach (var k in _evictionQueue)
        {
            if (k != key)
                rebuilt.Enqueue(k);
        }
        _evictionQueue = rebuilt;
    }


    private void Log(string operation, string key, string result) =>
        _history.Push(new AccessRecord(operation, key, result, DateTime.Now));

    public AccessRecord? PeekLastAccess() =>
        _history.Count > 0 ? _history.Peek() : null;

    public AccessRecord? PopLastAccess() =>
        _history.Count > 0 ? _history.Pop() : null;

    public IEnumerable<AccessRecord> History => _history;

    public void PrintState()
    {
        Console.WriteLine($"  Кеш ({Count}/{Capacity}):");
        foreach (var key in _evictionQueue)
            Console.WriteLine($"    {_storage[key]}");
        Console.WriteLine($"  HashSet ключів: {{ {string.Join(", ", _keys)} }}");
        Console.WriteLine($"  Queue (LRU, початок -> кінець): {string.Join(" -> ", _evictionQueue)}");
        Console.WriteLine($"  Наступний на видалення (Peek): {PeekNextToEvict() ?? "—"}");
    }
}
