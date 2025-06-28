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

    public async Task<JsonDbSet<T>> SetAsync<T>() where T : class, IJsonEntity
    {
        Type type = typeof(T);
        SemaphoreSlim semaphore = _locks.GetOrAdd(type, _ => new SemaphoreSlim(1, 1));
        string fileName = Path.Combine(_basePath, $"{type.Name}.json");

        await semaphore.WaitAsync();
        try
        {
            JsonDbSet<T> request = new JsonDbSet<T>(new List<T>(), async (items) => await SaveEntityListAsync(items, semaphore, fileName));
            if(File.Exists(fileName))
            {
                string json = await File.ReadAllTextAsync(fileName);
                List<T>? data = JsonSerializer.Deserialize<List<T>>(json);
                if(data != null)
                {
                    request = new JsonDbSet<T>(data, async (items) => await SaveEntityListAsync(items, semaphore, fileName));
                }
            }
            return request;
        }
        finally
        {
            semaphore.Release();
        }
    }

    private async Task SaveEntityListAsync<T>(List<T> items, SemaphoreSlim semaphore, string fileName) where T : class
    {
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
