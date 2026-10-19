namespace lab7v11;

public interface IRepository<T> where T : IEntity
{
    /// <summary>Повертає елемент за Id або null, якщо не знайдено.</summary>
    T? GetById(int id);
    IEnumerable<T> GetAll();
    void Add(T item);
    /// <exception cref="KeyNotFoundException">Елемента з таким Id немає.</exception>
    void Update(T item);
    /// <exception cref="KeyNotFoundException">Елемента з таким Id немає.</exception>
    void Delete(int id);
}
