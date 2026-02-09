# Advanced Usage Examples

## 1. Multiple Entities

### 1.1 Define new entities

```csharp
using JsonDatabaseContext.Interfaces;

public class Product : IJsonEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public DateTime CreationDate { get; set; }
}

public class Order : IJsonEntity
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public List<Guid> ProductIds { get; set; } = new();
    public decimal Total { get; set; }
    public DateTime OrderDate { get; set; }
    public OrderStatus Status { get; set; }
}

public enum OrderStatus
{
    Pending,
    Confirmed,
    Shipped,
    Delivered,
    Cancelled
}
```

### 1.2 Business service managing multiple entities

```csharp
public class OrderService
{
    private readonly IRepository<Order> _orderRepository;
    private readonly IRepository<Product> _productRepository;
    private readonly IRepository<User> _userRepository;

    public OrderService(
        IRepository<Order> orderRepository,
        IRepository<Product> productRepository,
        IRepository<User> userRepository)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _userRepository = userRepository;
    }

    public async Task<Order?> CreateOrderAsync(Guid userId, List<Guid> productIds)
    {
        // Validate user
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null || !user.Active)
            throw new InvalidOperationException("Invalid user");

        // Get products and calculate total
        var products = await _productRepository.GetAllAsync();
        var selectedProducts = products
            .Where(p => productIds.Contains(p.Id))
            .ToList();

        if (!selectedProducts.Any())
            throw new InvalidOperationException("No valid products in the order");

        var total = selectedProducts.Sum(p => p.Price);

        // Create order
        var order = new Order
        {
            UserId = userId,
            ProductIds = productIds,
            Total = total,
            OrderDate = DateTime.UtcNow,
            Status = OrderStatus.Pending
        };

        await _orderRepository.AddAsync(order);
        return order;
    }

    public async Task ConfirmOrderAsync(Guid orderId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null)
            throw new InvalidOperationException("Order not found");

        if (order.Status != OrderStatus.Pending)
            throw new InvalidOperationException("Only pending orders can be confirmed");

        order.Status = OrderStatus.Confirmed;
        await _orderRepository.UpdateAsync(order);
    }

    public async Task<List<Order>> GetOrdersByUserAsync(Guid userId)
    {
        var orders = await _orderRepository.GetAllAsync();
        return orders.Where(o => o.UserId == userId).ToList();
    }

    public async Task<decimal> GetTotalSalesAsync()
    {
        var orders = await _orderRepository.GetAllAsync();
        return orders
            .Where(o => o.Status == OrderStatus.Delivered)
            .Sum(o => o.Total);
    }
}
```

## 2. Advanced Search and Filtering

```csharp
public class SearchService
{
    private readonly IRepository<User> _userRepository;

    public SearchService(IRepository<User> userRepository)
    {
        _userRepository = userRepository;
    }

    // Search by pattern
    public async Task<List<User>> SearchByNameAsync(string pattern)
    {
        var users = await _userRepository.GetAllAsync();
        return users
            .Where(u => u.Name.Contains(pattern, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    // Search with multiple criteria
    public async Task<List<User>> AdvancedSearchAsync(
        string? name = null,
        string? email = null,
        bool? active = null,
        DateTime? fromDate = null,
        DateTime? toDate = null)
    {
        var users = await _userRepository.GetAllAsync();

        var result = users.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(name))
            result = result.Where(u => u.Name.Contains(name, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(email))
            result = result.Where(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));

        if (active.HasValue)
            result = result.Where(u => u.Active == active);

        if (fromDate.HasValue)
            result = result.Where(u => u.CreationDate >= fromDate);

        if (toDate.HasValue)
            result = result.Where(u => u.CreationDate <= toDate);

        return result.ToList();
    }

    // Pagination
    public async Task<PaginatedResult<User>> GetPaginatedAsync(
        int page = 1,
        int pageSize = 10,
        string? sortBy = null)
    {
        var users = await _userRepository.GetAllAsync();

        // Sorting
        if (!string.IsNullOrWhiteSpace(sortBy))
        {
            users = sortBy.ToLower() switch
            {
                "name" => users.OrderBy(u => u.Name).ToList(),
                "name_desc" => users.OrderByDescending(u => u.Name).ToList(),
                "date" => users.OrderBy(u => u.CreationDate).ToList(),
                "date_desc" => users.OrderByDescending(u => u.CreationDate).ToList(),
                _ => users
            };
        }

        // Pagination
        var total = users.Count;
        var items = users
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PaginatedResult<User>
        {
            Items = items,
            Total = total,
            Page = page,
            PageSize = pageSize
        };
    }
}

public class PaginatedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (Total + PageSize - 1) / PageSize;
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;
}
```

## 3. Batch Operations and Transactions

```csharp
public class BatchOperationService
{
    private readonly IRepository<User> _userRepository;

    public BatchOperationService(IRepository<User> userRepository)
    {
        _userRepository = userRepository;
    }

    // Import multiple users
    public async Task ImportUsersAsync(List<CreateUserDto> users)
    {
        var usersToAdd = users
            .Select(u => new User
            {
                Name = u.Name,
                Email = u.Email,
                CreationDate = DateTime.UtcNow,
                Active = true
            })
            .ToList();

        await _userRepository.AddRangeAsync(usersToAdd);
    }

    // Batch update
    public async Task UpdateStatusBulkAsync(List<Guid> userIds, bool active)
    {
        var users = await _userRepository.GetAllAsync();
        
        var usersToUpdate = users
            .Where(u => userIds.Contains(u.Id))
            .ToList();

        foreach (var user in usersToUpdate)
        {
            user.Active = active;
        }

        await _userRepository.UpdateRangeAsync(usersToUpdate);
    }

    // Batch delete
    public async Task DeleteBulkAsync(List<Guid> userIds)
    {
        var users = await _userRepository.GetAllAsync();
        
        var usersToDelete = users
            .Where(u => userIds.Contains(u.Id))
            .ToList();

        await _userRepository.RemoveRangeAsync(usersToDelete);
    }
}

public record CreateUserDto(string Name, string Email);
```

## 4. Export and Reports

```csharp
using System.Globalization;
using System.Text;

public class ReportService
{
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<Order> _orderRepository;

    public ReportService(
        IRepository<User> userRepository,
        IRepository<Order> orderRepository)
    {
        _userRepository = userRepository;
        _orderRepository = orderRepository;
    }

    // Export to CSV
    public async Task<string> ExportUsersToCsvAsync()
    {
        var users = await _userRepository.GetAllAsync();
        var sb = new StringBuilder();

        // Headers
        sb.AppendLine("Id,Name,Email,CreationDate,Active");

        // Data
        foreach (var user in users)
        {
            sb.AppendLine($"\"{user.Id}\",\"{user.Name}\",\"{user.Email}\",\"{user.CreationDate:yyyy-MM-dd}\",{user.Active}");
        }

        return sb.ToString();
    }

    // Generate sales report
    public async Task<SalesReport> GenerateSalesReportAsync(DateTime from, DateTime to)
    {
        var orders = await _orderRepository.GetAllAsync();
        var ordersInRange = orders.Where(o => o.OrderDate >= from && o.OrderDate <= to);

        return new SalesReport
        {
            StartDate = from,
            EndDate = to,
            TotalOrders = ordersInRange.Count(),
            TotalSales = ordersInRange.Sum(o => o.Total),
            AverageSale = ordersInRange.Any() ? ordersInRange.Average(o => o.Total) : 0,
            MaximumSale = ordersInRange.Any() ? ordersInRange.Max(o => o.Total) : 0,
            MinimumSale = ordersInRange.Any() ? ordersInRange.Min(o => o.Total) : 0
        };
    }
}

public class SalesReport
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalOrders { get; set; }
    public decimal TotalSales { get; set; }
    public decimal AverageSale { get; set; }
    public decimal MaximumSale { get; set; }
    public decimal MinimumSale { get; set; }
}
```

## 5. Audit and History

```csharp
public class AuditEntry : IJsonEntity
{
    public Guid Id { get; set; }
    public string Table { get; set; } = string.Empty;
    public string Operation { get; set; } = string.Empty;
    public string DataBefore { get; set; } = string.Empty;
    public string DataAfter { get; set; } = string.Empty;
    public DateTime OperationDate { get; set; }
    public string User { get; set; } = string.Empty;
}

public class AuditService
{
    private readonly IRepository<AuditEntry> _auditRepository;

    public AuditService(IRepository<AuditEntry> auditRepository)
    {
        _auditRepository = auditRepository;
    }

    public async Task RecordAsync(
        string table,
        string operation,
        string dataBefore,
        string dataAfter,
        string user)
    {
        var entry = new AuditEntry
        {
            Table = table,
            Operation = operation,
            DataBefore = dataBefore,
            DataAfter = dataAfter,
            OperationDate = DateTime.UtcNow,
            User = user
        };

        await _auditRepository.AddAsync(entry);
    }

    public async Task<List<AuditEntry>> GetHistoryAsync(string table)
    {
        var audits = await _auditRepository.GetAllAsync();
        return audits
            .Where(a => a.Table == table)
            .OrderByDescending(a => a.OperationDate)
            .ToList();
    }
}
```

## 6. Custom Validation

```csharp
public class ValidationService
{
    public ValidationResult ValidateUser(User user)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(user.Name))
            errors.Add("Name is required");

        if (user.Name.Length < 3)
            errors.Add("Name must be at least 3 characters");

        if (string.IsNullOrWhiteSpace(user.Email))
            errors.Add("Email is required");

        if (!IsValidEmail(user.Email))
            errors.Add("Email format is invalid");

        return new ValidationResult
        {
            IsValid = !errors.Any(),
            Errors = errors
        };
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}

public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
}
```

All these examples show how to extend and customize `JsonDatabaseContext` according to your specific needs.
