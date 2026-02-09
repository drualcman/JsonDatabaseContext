# Configuration Guide - JsonDatabaseContext

## 1. Initial Setup

### 1.1 Add appsettings.json

```json
{
  "DbContextOptions": {
    "BasePath": "data"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  }
}
```

### 1.2 Environment-Specific Configuration

For different environments, create specific configuration files:

- `appsettings.Development.json`
- `appsettings.Production.json`

Example `appsettings.Production.json`:

```json
{
  "DbContextOptions": {
    "BasePath": "/var/data/myapp"
  }
}
```

## 2. Configuration in Program.cs

### Option 1: Basic Configuration

```csharp
using JsonDatabaseContext;
using JsonDatabaseContext.Options;
using JsonDatabaseContext.Examples;

var builder = WebApplicationBuilder.CreateBuilder(args);

// Add services
builder.Services.AddControllers();

// Configure DbContextOptions
builder.Services.Configure<DbContextOptions>(
    builder.Configuration.GetSection(DbContextOptions.SectionKey));

// Register the context
builder.Services.AddScoped<IJsonDbContext, JsonDbContext>();

// Register generic repository
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// Register specific services
builder.Services.AddScoped<UserService>();

var app = builder.Build();

app.UseRouting();
app.MapControllers();

app.Run();
```

### Option 2: Advanced Configuration with Validation

```csharp
using JsonDatabaseContext;
using JsonDatabaseContext.Options;
using JsonDatabaseContext.Examples;

var builder = WebApplicationBuilder.CreateBuilder(args);

builder.Services.AddControllers();

// Configure and validate DbContextOptions
builder.Services.Configure<DbContextOptions>(options =>
{
    var configOptions = builder.Configuration.GetSection(DbContextOptions.SectionKey);
    configOptions.Bind(options);

    // Validate that BasePath is configured
    if (string.IsNullOrWhiteSpace(options.BasePath))
    {
        throw new InvalidOperationException(
            "DbContextOptions.BasePath is not configured in appsettings.json");
    }
});

builder.Services.AddScoped<IJsonDbContext, JsonDbContext>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<UserService>();

// Add logging
builder.Services.AddLogging(configure =>
{
    configure.AddConsole();
    configure.AddDebug();
});

var app = builder.Build();

// Middleware
app.UseRouting();
app.MapControllers();

app.Run();
```

## 3. Recommended Folder Structure

```
MyProject/
??? JsonDatabaseContext/          # Context library
?   ??? Interfaces/
?   ??? Entities/
?   ??? Options/
?   ??? Examples/                 # Usage examples
??? MyApp/                         # Your application
?   ??? Controllers/
?   ??? Services/
?   ??? Models/
?   ??? Program.cs
?   ??? appsettings.json
??? data/                          # Data folder (generated)
?   ??? User.json
?   ??? Product.json
??? README.md
```

## 4. Folder Permissions Configuration

Ensure that the folder specified in `BasePath` has correct permissions:

### On Windows:

```powershell
# Create folder
New-Item -Path ".\data" -ItemType Directory -Force

# Set permissions (replace "YourUsername")
$acl = Get-Acl ".\data"
$rule = New-Object System.Security.AccessControl.FileSystemAccessRule(
    "YourUsername",
    "FullControl",
    "ContainerInherit,ObjectInherit",
    "None",
    "Allow"
)
$acl.SetAccessRule($rule)
Set-Acl -Path ".\data" -AclObject $acl
```

### On Linux/Mac:

```bash
# Create folder
mkdir -p ./data

# Set permissions
chmod 755 ./data
```

## 5. Dependency Injection - Common Patterns

### Pattern 1: Generic Service

```csharp
public class GenericDataService<T> where T : class, IJsonEntity
{
    private readonly IRepository<T> _repository;

    public GenericDataService(IRepository<T> repository)
    {
        _repository = repository;
    }

    public async Task<List<T>> GetAllAsync() => 
        await _repository.GetAllAsync();
}

// In Program.cs
builder.Services.AddScoped(typeof(GenericDataService<>));
```

### Pattern 2: Specific Service with Multiple Repositories

```csharp
public class IntegralManagementService
{
    private readonly IRepository<User> _userRepo;
    private readonly IRepository<Product> _productRepo;

    public IntegralManagementService(
        IRepository<User> userRepo,
        IRepository<Product> productRepo)
    {
        _userRepo = userRepo;
        _productRepo = productRepo;
    }

    public async Task CreateUserWithProductsAsync(User user, List<Product> products)
    {
        await _userRepo.AddAsync(user);
        await _productRepo.AddRangeAsync(products);
    }
}

// In Program.cs
builder.Services.AddScoped<IntegralManagementService>();
```

## 6. Error Handling

### Create an exception handling service

```csharp
public class ErrorHandlingService
{
    private readonly ILogger<ErrorHandlingService> _logger;

    public ErrorHandlingService(ILogger<ErrorHandlingService> logger)
    {
        _logger = logger;
    }

    public async Task<T> ExecuteAsync<T>(Func<Task<T>> operation, string operationName)
    {
        try
        {
            return await operation();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Operation failed: {Operation}. Error: {Message}", operationName, ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError("Unexpected error in: {Operation}. Error: {Message}", operationName, ex.Message);
            throw;
        }
    }
}
```

## 7. Testing

### Create a mock context for unit tests

```csharp
using Moq;
using Xunit;

public class UserServiceTests
{
    [Fact]
    public async Task CreateUser_ShouldGenerateId_Automatically()
    {
        // Arrange
        var mockRepository = new Mock<IRepository<User>>();
        var service = new UserService(mockRepository.Object);

        // Act
        var user = await service.CreateUserAsync("John", "john@test.com");

        // Assert
        Assert.NotEqual(Guid.Empty, user.Id);
        mockRepository.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Once);
    }
}
```

## 8. Performance Considerations

### For applications with large amounts of data:

1. **Implement in-memory cache**
   ```csharp
   public class CachedRepository<T> : IRepository<T> where T : class, IJsonEntity
   {
       private readonly IRepository<T> _baseRepository;
       private readonly IMemoryCache _cache;

       // Cache implementation...
   }
   ```

2. **Implement pagination**
   ```csharp
   public class PaginatedResult<T>
   {
       public List<T> Items { get; set; }
       public int Total { get; set; }
       public int Page { get; set; }
       public int PageSize { get; set; }
   }
   ```

3. **Load data asynchronously**
   - Avoid loading all entities into memory
   - Implement filters in the repository

## 9. Migration from Other Databases

```csharp
// Migrate data from SQL Server to JSON
public class MigrationService
{
    private readonly IRepository<User> _jsonRepository;
    private readonly SqlDbContext _sqlContext;

    public async Task MigrateAsync()
    {
        var users = _sqlContext.Users.ToList();
        await _jsonRepository.AddRangeAsync(users);
    }
}
```

## 10. Data Backup

```csharp
public class BackupService
{
    private readonly string _dataPath;

    public async Task CreateBackupAsync()
    {
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var backupDir = Path.Combine(_dataPath, $"backup_{timestamp}");
        Directory.CreateDirectory(backupDir);

        foreach (var file in Directory.GetFiles(_dataPath, "*.json"))
        {
            var backupFile = Path.Combine(backupDir, Path.GetFileName(file));
            File.Copy(file, backupFile);
        }
    }
}
```
