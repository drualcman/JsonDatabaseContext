namespace JsonDatabaseContext.Entities;

public class JsonDbSet<T> where T : class, IJsonEntity
{
    private readonly JsonDbContext _context;

    public JsonDbSet(JsonDbContext context)
    {
        _context = context;
    }

    public Task<List<T>> GetAllAsync()
    {
        return _context.LoadEntityListAsync<T>();
    }

    public async Task AddAsync(T entity)
    {
        if (entity.Id == Guid.Empty)
        {
            entity.Id = Guid.NewGuid();
        }
        List<T> items = await _context.LoadEntityListAsync<T>();
        items.Add(entity);
        await _context.SaveEntityListAsync(items);
    }

    public async Task AddRangeAsync(IEnumerable<T> entities)
    {
        List<T> items = await _context.LoadEntityListAsync<T>();
        foreach (T entity in entities)
        {
            if (entity.Id == Guid.Empty)
            {
                entity.Id = Guid.NewGuid();
            }
            items.Add(entity);
        }
        await _context.SaveEntityListAsync(items);
    }

    public Task RemoveAsync(T entity) => RemoveRangeAsync([entity]);

    public async Task RemoveRangeAsync(IEnumerable<T> entities)
    {
        List<T> items = await _context.LoadEntityListAsync<T>();
        HashSet<Guid> ids = entities.Select(e => e.Id).ToHashSet();
        items.RemoveAll(e => ids.Contains(e.Id));
        await _context.SaveEntityListAsync(items);
    }

    public async Task UpdateAsync(T entity)
    {
        List<T> items = await _context.LoadEntityListAsync<T>();
        int index = items.FindIndex(e => e.Id == entity.Id);
        if (index != -1)
        {
            items[index] = entity;
            await _context.SaveEntityListAsync(items);
        }
    }

    public async Task UpdateRangeAsync(IEnumerable<T> entities)
    {
        List<T> items = new(entities);
        await _context.SaveEntityListAsync(items);
    }
}
