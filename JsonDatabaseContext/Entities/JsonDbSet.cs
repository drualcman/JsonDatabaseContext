namespace JsonDatabaseContext.Entities;

public class JsonDbSet<T> where T : class, IJsonEntity
{
    private readonly List<T> _items;
    private readonly Func<List<T>, Task> _saveFunc;

    public JsonDbSet(List<T> items, Func<List<T>, Task> saveFunc)
    {
        _items = items;
        _saveFunc = saveFunc;
    }

    public List<T> Items => _items;

    public async Task AddAsync(T entity)
    {
        if(entity.Id == Guid.Empty)
        {
            entity.Id = Guid.NewGuid();
        }
        _items.Add(entity);
        await _saveFunc(_items);
    }

    public async Task RemoveAsync(T entity)
    {
        _items.Remove(entity);
        await _saveFunc(_items);
    }

    public async Task UpdateAsync(T entity)
    {
        int index = _items.FindIndex(e => e.Id == entity.Id);
        if(index != -1)
        {
            _items[index] = entity;
            await _saveFunc(_items);
        }
    }

    public List<T> GetAll()
    {
        return new List<T>(_items);
    }

    public List<T> GetAll(Func<T, bool> predicate)
    {
        return new List<T>(_items.Where(predicate));
    }
}

