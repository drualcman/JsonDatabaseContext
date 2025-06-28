# JsonDatabaseContext
## Introduction
JsonDatabaseContext is a lightweight implementation of a DbContext similar to Entity Framework, but using JSON files for persistent storage. It's ideal for small applications that need data persistence without requiring a full database system.

## Key Features
- JSON-based persistence: Stores data in human-readable JSON files
- CRUD operations: Supports basic Create, Read, Update, and Delete operations
- Thread-safe: Uses semaphores to ensure thread-safe access
- Easy integration: Simple to set up and use in .NET projects

## Setup
### 1. Add Configuration
In your appsettings.json, add the database configuration:
```json
json
{
  "DbContextOptions": {
    "BasePath": "Data/JsonDatabase"
  }
}
```
### 2. Register the Service
In your dependency configuration file (typically Program.cs):

```csharp
builder.Services.Configure<DbContextOptions>(builder.Configuration.GetSection(DbContextOptions.SectionKey));
builder.Services.AddSingleton<IJsonDbContext, JsonDbContext>();     //or AddScoped
```
## Basic Usage
### Getting a DbSet
To work with an entity, first get its JsonDbSet:

```csharp
var dbSet = await _dbContext.SetAsync<MyEntity>();
```
## CRUD Operations
Create an Entity
```csharp
var newEntity = new MyEntity { /* properties */ };
await dbSet.AddAsync(newEntity);
```
### Read Entities
```csharp
// All entities
var all = dbSet.GetAll();

// With filter
var filtered = dbSet.GetAll(e => e.Property == value);
Update an Entity
csharp
var entity = dbSet.GetAll().First();
entity.Property = newValue;
await dbSet.UpdateAsync(entity);
Delete an Entity
csharp
var entity = dbSet.GetAll().First();
await dbSet.RemoveAsync(entity);
```
### Entity Requirements
All entities must implement the IJsonEntity interface:

```csharp
public class MyEntity : IJsonEntity
{
    public Guid Id { get; set; }
    // other properties...
}
```
### File Structure
Data is stored in JSON files at the configured path (BasePath), with one file per entity type:

```text
Data/
  JsonDatabase/
    MyEntity.json
    AnotherEntity.json
```
## Considerations
- Performance: This approach is suitable for small datasets. For large-scale data, consider a traditional database.
- Concurrency: While thread-safe, heavy concurrent access may impact performance.
- Migrations: No migration system included. Entity changes may require manual JSON file manipulation.

## Complete Example
```csharp
// Entity definition
public class Product : IJsonEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
}

// Service usage
public class ProductService
{
    private readonly IJsonDbContext _dbContext;
    
    public ProductService(IJsonDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task AddProduct(string name, decimal price)
    {
        var products = await _dbContext.SetAsync<Product>();
        await products.AddAsync(new Product {
            Id = Guid.NewGuid(),
            Name = name,
            Price = price
        });
    }
    
    public async Task<List<Product>> GetExpensiveProducts(decimal minPrice)
    {
        var products = await _dbContext.SetAsync<Product>();
        return products.GetAll(p => p.Price >= minPrice);
    }
}
```
## License
[MIT License] - Free to use in your projects.