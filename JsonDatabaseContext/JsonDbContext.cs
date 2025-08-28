namespace JsonDatabaseContext;

public class JsonDbContext : IJsonDbContext
{
    private readonly string _basePath;
    private static readonly ConcurrentDictionary<Type, SemaphoreSlim> _locks = new();

    public JsonDbContext(IOptions<DbContextOptions> options)
    {
        _basePath = Path.IsPathRooted(options.Value.BasePath)
            ? options.Value.BasePath
            : Path.Combine(AppContext.BaseDirectory, options.Value.BasePath);
        Directory.CreateDirectory(_basePath);
    }

    public Task<JsonDbSet<T>> SetAsync<T>() where T : class, IJsonEntity
    {
        return Task.FromResult(new JsonDbSet<T>(this));
    }

    internal async Task<List<T>> LoadEntityListAsync<T>() where T : class
    {
        Type type = typeof(T);
        SemaphoreSlim semaphore = _locks.GetOrAdd(type, _ => new SemaphoreSlim(1, 1));
        string fileName = Path.Combine(_basePath, $"{type.Name}.json");

        await semaphore.WaitAsync();
        try
        {
            if (!File.Exists(fileName))
            {
                return new List<T>();
            }

            string json = await File.ReadAllTextAsync(fileName);
            return JsonSerializer.Deserialize<List<T>>(json) ?? new List<T>();
        }
        finally
        {
            semaphore.Release();
        }
    }

    internal async Task SaveEntityListAsync<T>(List<T> items) where T : class
    {
        Type type = typeof(T);
        SemaphoreSlim semaphore = _locks.GetOrAdd(type, _ => new SemaphoreSlim(1, 1));
        string fileName = Path.Combine(_basePath, $"{type.Name}.json");

        await semaphore.WaitAsync();
        try
        {
            string json = JsonSerializer.Serialize(items, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(fileName, json);
        }
        finally
        {
            semaphore.Release();
        }
    }
}
