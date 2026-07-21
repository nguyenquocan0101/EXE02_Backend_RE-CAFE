using EXE02_Backend_RE_CAFE.Data;
using EXE02_Backend_RE_CAFE.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace EXE02_Backend_RE_CAFE.Tests.Infrastructure;

public sealed class PaymentApiFactory : WebApplicationFactory<Program>
{
    public const string JwtIssuer = "recafe-payment-tests";
    public const string JwtAudience = "recafe-payment-tests-client";
    public const string JwtKey = "recafe-payment-tests-signing-key-2026-at-least-32-bytes";
    public const string SepayApiKey = "recafe-payment-tests-sepay-key";

    private readonly string _connectionString;

    public PaymentApiFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = _connectionString,
                ["Jwt:Key"] = JwtKey,
                ["Jwt:Issuer"] = JwtIssuer,
                ["Jwt:Audience"] = JwtAudience,
                ["Sepay:ApiKey"] = SepayApiKey,
                ["Sepay:BankAccount"] = "123456789",
                ["Sepay:BankName"] = "MBBank",
                ["Sepay:QrPrefix"] = "RECAFE",
                ["ApplyMigrationsOnStartup"] = "false",
                ["DisableHttpsRedirection"] = "true",
                ["EnableSwagger"] = "false",
                ["Logging:LogLevel:Default"] = "Warning",
                ["Logging:LogLevel:Microsoft.EntityFrameworkCore.Database.Command"] = "Warning",
                ["Logging:LogLevel:Microsoft.Hosting.Lifetime"] = "Warning"
            });
        });

        builder.ConfigureServices(services =>
        {
            var renderWorker = services.FirstOrDefault(descriptor =>
                descriptor.ServiceType == typeof(IHostedService)
                && descriptor.ImplementationType == typeof(ProductCustomizationRenderWorker));
            if (renderWorker != null)
            {
                services.Remove(renderWorker);
            }

            var dbContextOptions = services
                .Where(descriptor => descriptor.ServiceType == typeof(ApplicationDbContext)
                    || descriptor.ServiceType == typeof(DbContextOptions<ApplicationDbContext>))
                .ToList();
            foreach (var descriptor in dbContextOptions)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(_connectionString));
            services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = JwtIssuer,
                    ValidAudience = JwtAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtKey))
                };
            });
        });
    }
}
