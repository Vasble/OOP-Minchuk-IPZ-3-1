namespace lab7v11;

public class InMemoryRepository<T> : IRepository<T> where T : IEntity
{
    private readonly List<T> _items = new();
    private int _nextId = 1;

    public T? GetById(int id) => _items.FirstOrDefault(x => x.Id == id);

    public IEnumerable<T> GetAll() => _items.ToList();

    public void Add(T item)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (item.Id == 0)
        {
            item.Id = _nextId++;
        }
        else if (_items.Any(x => x.Id == item.Id))
        {
            throw new InvalidOperationException($"Елемент з Id={item.Id} уже існує.");
        }
        else if (item.Id >= _nextId)
        {
            _nextId = item.Id + 1;
        }

        _items.Add(item);
    }

    public void Update(T item)
    {
        ArgumentNullException.ThrowIfNull(item);

        int index = _items.FindIndex(x => x.Id == item.Id);
        if (index < 0)
            throw new KeyNotFoundException($"Елемент з Id={item.Id} не знайдено.");

        _items[index] = item;
    }

    public void Delete(int id)
    {
        int index = _items.FindIndex(x => x.Id == id);
        if (index < 0)
            throw new KeyNotFoundException($"Елемент з Id={id} не знайдено.");

        _items.RemoveAt(index);
    }
}
