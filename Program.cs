using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
// using Microsoft.OpenApi.Models;
using System.Text;
using EXE02_Backend_RE_CAFE.Data;
using EXE02_Backend_RE_CAFE.Interfaces;
using EXE02_Backend_RE_CAFE.Services;
using EXE02_Backend_RE_CAFE.Middlewares;
using EXE02_Backend_RE_CAFE.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure DbContext with PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

builder.Services.AddAuthorization();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Register Custom Services
builder.Services.AddApplicationServices();

var app = builder.Build();

var applyMigrationsOnStartup = builder.Configuration.GetValue<bool>("ApplyMigrationsOnStartup", false);
if (applyMigrationsOnStartup)
{
    var maxMigrationRetries = builder.Configuration.GetValue<int>("MigrationRetryCount", 5);
    var migrationRetryDelaySeconds = builder.Configuration.GetValue<int>("MigrationRetryDelaySeconds", 5);
    var failStartupOnMigrationError = builder.Configuration.GetValue<bool>("FailStartupOnMigrationError", false);
    var migrationsApplied = false;

    for (var attempt = 1; attempt <= maxMigrationRetries; attempt++)
    {
        try
        {
            using var scope = app.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            dbContext.Database.Migrate();
            migrationsApplied = true;
            app.Logger.LogInformation("Database migrations applied successfully.");
            break;
        }
        catch (Exception ex)
        {
            app.Logger.LogError(ex, "Failed to apply database migrations on attempt {Attempt}/{MaxAttempts}.", attempt, maxMigrationRetries);

            if (attempt < maxMigrationRetries)
            {
                await Task.Delay(TimeSpan.FromSeconds(migrationRetryDelaySeconds));
            }
        }
    }

    if (!migrationsApplied)
    {
        if (failStartupOnMigrationError)
        {
            throw new InvalidOperationException("Failed to apply database migrations after all retry attempts.");
        }

        app.Logger.LogWarning("Continuing startup without applying database migrations.");
    }
}

app.UseExceptionHandler();

// Configure the HTTP request pipeline.
var enableSwagger = builder.Configuration.GetValue<bool>("EnableSwagger", app.Environment.IsDevelopment());
if (enableSwagger)
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ReCafe API v1");
    });
}

var disableHttpsRedirection = builder.Configuration.GetValue<bool>("DisableHttpsRedirection");
if (!disableHttpsRedirection)
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/healthz", () => Results.Ok(new { status = "ok" }))
    .WithName("HealthCheck");

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.MapControllers();
app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
