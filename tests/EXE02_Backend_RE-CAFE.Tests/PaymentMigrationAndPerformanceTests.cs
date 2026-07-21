using System.Diagnostics;
using EXE02_Backend_RE_CAFE.DTOs;
using EXE02_Backend_RE_CAFE.Services;
using EXE02_Backend_RE_CAFE.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Xunit.Abstractions;

namespace EXE02_Backend_RE_CAFE.Tests;

[Collection(PaymentApiCollection.Name)]
public sealed class PaymentMigrationAndPerformanceTests
{
    private readonly PaymentTestFixture _fixture;
    private readonly ITestOutputHelper _output;

    public PaymentMigrationAndPerformanceTests(PaymentTestFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
    }

    [Fact]
    public async Task Migration_AppliesPaymentManagementColumnAndIndexesOnPostgreSql()
    {
        await using var context = _fixture.CreateDbContext();
        var appliedMigrations = await context.Database.GetAppliedMigrationsAsync();
        Assert.Contains("20260721053802_AddPaymentTransactionManagement", appliedMigrations);

        await using var connection = new NpgsqlConnection(_fixture.ConnectionString);
        await connection.OpenAsync();
        await using var columnCommand = new NpgsqlCommand(
            """
            SELECT COUNT(*)
            FROM information_schema.columns
            WHERE table_schema = 'public'
              AND table_name = 'Payments'
              AND column_name = 'CreatedAt'
              AND is_nullable = 'NO';
            """,
            connection);
        Assert.Equal(1L, Convert.ToInt64(await columnCommand.ExecuteScalarAsync()));

        await using var indexCommand = new NpgsqlCommand(
            "SELECT indexdef FROM pg_indexes WHERE schemaname = 'public' AND tablename = 'Payments';",
            connection);
        await using var reader = await indexCommand.ExecuteReaderAsync();
        var definitions = new List<string>();
        while (await reader.ReadAsync())
        {
            definitions.Add(reader.GetString(0));
        }

        Assert.Contains(definitions, definition => definition.Contains("\"Status\"", StringComparison.Ordinal));
        Assert.Contains(definitions, definition => definition.Contains("\"PaidAt\"", StringComparison.Ordinal));
        Assert.Contains(definitions, definition => definition.Contains("\"TransactionCode\"", StringComparison.Ordinal));
        Assert.Contains(definitions, definition => definition.Contains("\"CreatedAt\"", StringComparison.Ordinal));
    }

    [Fact]
    public async Task List_WithTenThousandRowsAndPageSizeOneHundred_HasP95BelowFiveHundredMilliseconds()
    {
        await _fixture.EnsurePerformanceDataAsync();
        await using var context = _fixture.CreateDbContext();
        var service = new PaymentManagementService(context);
        var query = new AdminPaymentQuery
        {
            Keyword = "PERF-20260721-",
            Page = 1,
            PageSize = 100
        };

        for (var warmup = 0; warmup < 3; warmup++)
        {
            await service.GetPaymentsAsync(query);
        }

        var durations = new List<double>();
        for (var iteration = 0; iteration < 20; iteration++)
        {
            var stopwatch = Stopwatch.StartNew();
            var result = await service.GetPaymentsAsync(query);
            stopwatch.Stop();
            Assert.Equal(100, result.Items.Count);
            Assert.True(result.Total >= 10_001);
            durations.Add(stopwatch.Elapsed.TotalMilliseconds);
        }

        durations.Sort();
        var percentileIndex = (int)Math.Ceiling(durations.Count * 0.95) - 1;
        var p95 = durations[percentileIndex];
        _output.WriteLine("Payment list latency ms: min={0:F2}, p95={1:F2}, max={2:F2}", durations[0], p95, durations[^1]);
        Assert.True(p95 < 500, $"Expected p95 < 500 ms but measured {p95:F2} ms.");
    }
}
