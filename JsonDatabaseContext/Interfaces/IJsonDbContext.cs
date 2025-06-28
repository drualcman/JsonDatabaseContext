namespace JsonDatabaseContext.Interfaces;

public interface IJsonDbContext
{
    Task<JsonDbSet<T>> SetAsync<T>() where T : class, IJsonEntity;
}
