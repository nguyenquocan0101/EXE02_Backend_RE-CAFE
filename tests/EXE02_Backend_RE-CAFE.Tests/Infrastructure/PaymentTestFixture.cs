using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using EXE02_Backend_RE_CAFE.Data;
using EXE02_Backend_RE_CAFE.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Npgsql;

namespace EXE02_Backend_RE_CAFE.Tests.Infrastructure;

public sealed class PaymentTestFixture : IAsyncLifetime
{
    private const string TestDatabasePrefix = "recafe_payment_tests_";
    private readonly SemaphoreSlim _performanceSeedLock = new(1, 1);
    private string _adminConnectionString = string.Empty;
    private bool _performanceDataSeeded;

    public Guid AdminUserId { get; } = Guid.NewGuid();
    public Guid StaffUserId { get; } = Guid.NewGuid();
    public Guid CustomerUserId { get; } = Guid.NewGuid();
    public Guid CustomerAddressId { get; } = Guid.NewGuid();
    public string DatabaseName { get; } = $"{TestDatabasePrefix}{Guid.NewGuid():N}";
    public string ConnectionString { get; private set; } = string.Empty;
    public PaymentApiFactory Factory { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        _adminConnectionString = ResolveAdminConnectionString();
        var testBuilder = new NpgsqlConnectionStringBuilder(_adminConnectionString)
        {
            Database = DatabaseName,
            Pooling = true,
            Timeout = 10,
            CommandTimeout = 120
        };
        ConnectionString = testBuilder.ConnectionString;

        await CreateDatabaseAsync();
        await using (var context = CreateDbContext())
        {
            await context.Database.MigrateAsync();
            await SeedIdentityDataAsync(context);
        }

        Factory = new PaymentApiFactory(ConnectionString);
    }

    public async Task DisposeAsync()
    {
        if (Factory != null)
        {
            await Factory.DisposeAsync();
        }

        NpgsqlConnection.ClearAllPools();
        if (!DatabaseName.StartsWith(TestDatabasePrefix, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Refusing to drop a database outside the generated test prefix.");
        }

        await using var connection = new NpgsqlConnection(_adminConnectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand($"DROP DATABASE IF EXISTS \"{DatabaseName}\" WITH (FORCE);", connection);
        await command.ExecuteNonQueryAsync();
    }

    public ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;
        return new ApplicationDbContext(options);
    }

    public HttpClient CreateAnonymousClient()
    {
        return Factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    public HttpClient CreateAuthenticatedClient(UserRole role)
    {
        var userId = role switch
        {
            UserRole.Admin => AdminUserId,
            UserRole.Staff => StaffUserId,
            _ => CustomerUserId
        };
        var client = CreateAnonymousClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CreateToken(userId, role));
        return client;
    }

    public async Task<PaymentSeed> CreatePaymentAsync(
        string orderCode,
        PaymentStatus status,
        decimal amount,
        PaymentMethod method = PaymentMethod.BankTransfer,
        string? transactionCode = null,
        DateTime? createdAt = null)
    {
        await using var context = CreateDbContext();
        return await CreatePaymentAsync(
            context,
            CustomerUserId,
            CustomerAddressId,
            orderCode,
            status,
            amount,
            method,
            transactionCode,
            createdAt);
    }

    public async Task<PaymentSeed> CreatePaymentForCustomerAsync(
        string orderCode,
        string customerName,
        PaymentStatus status,
        decimal amount,
        string? transactionCode = null)
    {
        await using var context = CreateDbContext();
        var userId = Guid.NewGuid();
        var addressId = Guid.NewGuid();
        context.Users.Add(new User
        {
            Id = userId,
            Username = $"customer-{userId:N}",
            Email = $"{userId:N}@tests.local",
            FullName = customerName,
            PasswordHash = "test-only",
            Role = UserRole.Customer
        });
        context.Addresses.Add(new Address
        {
            Id = addressId,
            UserId = userId,
            ReceiverName = customerName,
            Phone = "0900000000",
            Province = "Ho Chi Minh",
            District = "District 1",
            Ward = "Ward 1",
            DetailAddress = "Test address"
        });
        await context.SaveChangesAsync();

        return await CreatePaymentAsync(
            context,
            userId,
            addressId,
            orderCode,
            status,
            amount,
            PaymentMethod.BankTransfer,
            transactionCode,
            DateTime.UtcNow);
    }

    public async Task<Guid> CreateCartItemAsync(decimal price)
    {
        await using var context = CreateDbContext();
        var suffix = Guid.NewGuid().ToString("N");
        var category = new Category
        {
            Name = $"Payment test category {suffix}",
            Slug = $"payment-test-category-{suffix}"
        };
        var product = new Product
        {
            CategoryId = category.Id,
            Name = $"Payment test product {suffix}",
            Slug = $"payment-test-product-{suffix}",
            SKU = $"PAY-{suffix[..12]}",
            Price = price,
            IsActive = true
        };
        var cart = await context.Carts.SingleOrDefaultAsync(item => item.UserId == CustomerUserId);
        if (cart == null)
        {
            cart = new Cart { UserId = CustomerUserId };
            context.Carts.Add(cart);
        }

        var cartItem = new CartItem
        {
            CartId = cart.Id,
            ProductId = product.Id,
            Quantity = 1
        };
        context.Categories.Add(category);
        context.Products.Add(product);
        context.CartItems.Add(cartItem);
        await context.SaveChangesAsync();
        return cartItem.Id;
    }

    public async Task EnsurePerformanceDataAsync(int count = 10_001)
    {
        if (_performanceDataSeeded)
        {
            return;
        }

        await _performanceSeedLock.WaitAsync();
        try
        {
            if (_performanceDataSeeded)
            {
                return;
            }

            await using var context = CreateDbContext();
            context.ChangeTracker.AutoDetectChangesEnabled = false;
            var createdAt = DateTime.UtcNow.AddDays(-1);
            var orders = new List<Order>(count);
            var payments = new List<Payment>(count);

            for (var index = 0; index < count; index++)
            {
                var orderId = Guid.NewGuid();
                var isPaid = index % 2 == 0;
                var amount = 100_000m + index;
                orders.Add(new Order
                {
                    Id = orderId,
                    UserId = CustomerUserId,
                    ShippingAddressId = CustomerAddressId,
                    OrderCode = $"PERF-20260721-{index:D5}",
                    Subtotal = amount,
                    ShippingFee = 0,
                    TotalAmount = amount,
                    Status = isPaid ? OrderStatus.Confirmed : OrderStatus.Pending,
                    PaymentStatus = isPaid ? PaymentStatus.Paid : PaymentStatus.Unpaid,
                    CreatedAt = createdAt.AddSeconds(index)
                });
                payments.Add(new Payment
                {
                    Id = Guid.NewGuid(),
                    OrderId = orderId,
                    Method = index % 3 == 0 ? PaymentMethod.COD : PaymentMethod.BankTransfer,
                    Status = isPaid ? PaymentStatus.Paid : PaymentStatus.Unpaid,
                    Amount = amount,
                    TransactionCode = isPaid ? $"PERF-TXN-{index:D5}" : null,
                    PaidAt = isPaid ? createdAt.AddSeconds(index) : null,
                    CreatedAt = createdAt.AddSeconds(index)
                });
            }

            context.Orders.AddRange(orders);
            context.Payments.AddRange(payments);
            await context.SaveChangesAsync();
            _performanceDataSeeded = true;
        }
        finally
        {
            _performanceSeedLock.Release();
        }
    }

    private async Task CreateDatabaseAsync()
    {
        await using var connection = new NpgsqlConnection(_adminConnectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand($"CREATE DATABASE \"{DatabaseName}\";", connection);
        await command.ExecuteNonQueryAsync();
    }

    private async Task SeedIdentityDataAsync(ApplicationDbContext context)
    {
        context.Users.AddRange(
            CreateUser(AdminUserId, "admin", UserRole.Admin),
            CreateUser(StaffUserId, "staff", UserRole.Staff),
            CreateUser(CustomerUserId, "customer", UserRole.Customer));
        context.Addresses.Add(new Address
        {
            Id = CustomerAddressId,
            UserId = CustomerUserId,
            ReceiverName = "Payment Test Customer",
            Phone = "0900000000",
            Province = "Ho Chi Minh",
            District = "District 1",
            Ward = "Ward 1",
            DetailAddress = "Test address",
            IsDefault = true
        });
        await context.SaveChangesAsync();
    }

    private async Task<PaymentSeed> CreatePaymentAsync(
        ApplicationDbContext context,
        Guid userId,
        Guid addressId,
        string orderCode,
        PaymentStatus status,
        decimal amount,
        PaymentMethod method,
        string? transactionCode,
        DateTime? createdAt)
    {
        var orderId = Guid.NewGuid();
        var paymentId = Guid.NewGuid();
        var timestamp = createdAt ?? DateTime.UtcNow;
        context.Orders.Add(new Order
        {
            Id = orderId,
            UserId = userId,
            ShippingAddressId = addressId,
            OrderCode = orderCode,
            Subtotal = amount,
            ShippingFee = 0,
            TotalAmount = amount,
            Status = status == PaymentStatus.Paid ? OrderStatus.Confirmed : OrderStatus.Pending,
            PaymentStatus = status,
            CreatedAt = timestamp
        });
        context.Payments.Add(new Payment
        {
            Id = paymentId,
            OrderId = orderId,
            Method = method,
            Status = status,
            Amount = amount,
            TransactionCode = transactionCode,
            PaidAt = status == PaymentStatus.Paid ? timestamp : null,
            CreatedAt = timestamp
        });
        await context.SaveChangesAsync();
        return new PaymentSeed(paymentId, orderId, orderCode, amount);
    }

    private string CreateToken(Guid userId, UserRole role)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, role.ToString()),
            new Claim(ClaimTypes.Role, role.ToString())
        };
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(PaymentApiFactory.JwtKey)),
            SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            PaymentApiFactory.JwtIssuer,
            PaymentApiFactory.JwtAudience,
            claims,
            expires: DateTime.UtcNow.AddMinutes(15),
            signingCredentials: credentials);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string ResolveAdminConnectionString()
    {
        var explicitConnection = Environment.GetEnvironmentVariable("PAYMENT_TEST_POSTGRES_ADMIN");
        if (!string.IsNullOrWhiteSpace(explicitConnection))
        {
            return BuildAdminConnectionString(explicitConnection, allowRemote: true);
        }

        var repositoryRoot = FindRepositoryRoot();
        using var document = JsonDocument.Parse(File.ReadAllText(Path.Combine(repositoryRoot, "appsettings.Development.json")));
        var connectionString = document.RootElement
            .GetProperty("ConnectionStrings")
            .GetProperty("DefaultConnection")
            .GetString()
            ?? throw new InvalidOperationException("Development database connection string is missing.");
        return BuildAdminConnectionString(connectionString, allowRemote: false);
    }

    private static string BuildAdminConnectionString(string connectionString, bool allowRemote)
    {
        var builder = new NpgsqlConnectionStringBuilder(connectionString);
        if (!allowRemote && builder.Host is not ("localhost" or "127.0.0.1" or "::1"))
        {
            throw new InvalidOperationException("Automatic test database creation is restricted to local PostgreSQL.");
        }

        builder.Database = "postgres";
        builder.Pooling = false;
        builder.Timeout = 10;
        builder.CommandTimeout = 120;
        return builder.ConnectionString;
    }

    private static string FindRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory != null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "EXE02_Backend_RE-CAFE.csproj")))
            {
                return directory.FullName;
            }
        }

        throw new DirectoryNotFoundException("Could not locate the backend repository root.");
    }

    private static User CreateUser(Guid id, string username, UserRole role)
    {
        return new User
        {
            Id = id,
            Username = $"payment-test-{username}",
            Email = $"payment-test-{username}@tests.local",
            FullName = $"Payment Test {role}",
            PasswordHash = "test-only",
            Role = role
        };
    }
}

public sealed record PaymentSeed(Guid PaymentId, Guid OrderId, string OrderCode, decimal Amount);

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class PaymentApiCollection : ICollectionFixture<PaymentTestFixture>
{
    public const string Name = "Payment API integration";
}
