using EXE02_Backend_RE_CAFE.DTOs;
using EXE02_Backend_RE_CAFE.Models;
using EXE02_Backend_RE_CAFE.Services;
using EXE02_Backend_RE_CAFE.Tests.Infrastructure;

namespace EXE02_Backend_RE_CAFE.Tests;

[Collection(PaymentApiCollection.Name)]
public sealed class PaymentManagementIntegrationTests
{
    private readonly PaymentTestFixture _fixture;

    public PaymentManagementIntegrationTests(PaymentTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Summary_WhenFiltered_ExcludesEveryNonPaidAmount()
    {
        var tag = $"SUMMARY-{Guid.NewGuid():N}"[..20];
        await _fixture.CreatePaymentAsync($"{tag}-PAID-1", PaymentStatus.Paid, 120_000m, transactionCode: $"{tag}-TXN-1");
        await _fixture.CreatePaymentAsync($"{tag}-PAID-2", PaymentStatus.Paid, 80_000m, transactionCode: $"{tag}-TXN-2");
        await _fixture.CreatePaymentAsync($"{tag}-UNPAID", PaymentStatus.Unpaid, 900_000m);
        await _fixture.CreatePaymentAsync($"{tag}-PENDING", PaymentStatus.Pending, 700_000m);
        await _fixture.CreatePaymentAsync($"{tag}-FAILED", PaymentStatus.Failed, 600_000m);
        await _fixture.CreatePaymentAsync($"{tag}-REFUNDED", PaymentStatus.Refunded, 500_000m);

        await using var context = _fixture.CreateDbContext();
        var service = new PaymentManagementService(context);

        var summary = await service.GetPaymentSummaryAsync(new AdminPaymentQuery { Keyword = tag });

        Assert.Equal(2, summary.PaidCount);
        Assert.Equal(1, summary.UnpaidCount);
        Assert.Equal(200_000m, summary.PaidAmount);

        var unpaidSummary = await service.GetPaymentSummaryAsync(new AdminPaymentQuery
        {
            Keyword = tag,
            Status = PaymentStatus.Unpaid
        });
        Assert.Equal(0, unpaidSummary.PaidCount);
        Assert.Equal(1, unpaidSummary.UnpaidCount);
        Assert.Equal(0m, unpaidSummary.PaidAmount);
    }

    [Fact]
    public async Task List_WhenFiltersAreCombined_ReturnsOnlyMatchingRowsAndServerPagination()
    {
        var tag = $"FILTER-{Guid.NewGuid():N}"[..19];
        var insideRange = DateTime.UtcNow.AddHours(-2);
        await _fixture.CreatePaymentAsync(
            $"{tag}-MATCH",
            PaymentStatus.Paid,
            150_000m,
            PaymentMethod.BankTransfer,
            $"{tag}-TXN",
            insideRange);
        await _fixture.CreatePaymentAsync(
            $"{tag}-WRONG-STATUS",
            PaymentStatus.Unpaid,
            150_000m,
            PaymentMethod.BankTransfer,
            createdAt: insideRange);
        await _fixture.CreatePaymentAsync(
            $"{tag}-WRONG-METHOD",
            PaymentStatus.Paid,
            150_000m,
            PaymentMethod.COD,
            $"{tag}-COD",
            insideRange);
        await _fixture.CreatePaymentAsync(
            $"{tag}-OUTSIDE-DATE",
            PaymentStatus.Paid,
            150_000m,
            PaymentMethod.BankTransfer,
            $"{tag}-OLD",
            DateTime.UtcNow.AddDays(-10));

        await using var context = _fixture.CreateDbContext();
        var service = new PaymentManagementService(context);
        var result = await service.GetPaymentsAsync(new AdminPaymentQuery
        {
            Keyword = tag,
            Status = PaymentStatus.Paid,
            Method = PaymentMethod.BankTransfer,
            From = DateTime.UtcNow.AddDays(-1),
            To = DateTime.UtcNow.AddDays(1),
            Page = 1,
            PageSize = 1000
        });

        var payment = Assert.Single(result.Items);
        Assert.Equal($"{tag}-MATCH", payment.OrderCode);
        Assert.Equal(1, result.Total);
        Assert.Equal(100, result.PageSize);
        Assert.Equal(1, result.TotalPages);
    }

    [Fact]
    public async Task Export_WhenFilteredRowsExceedLimit_IsRejectedWithoutMaterializingTheFile()
    {
        await _fixture.EnsurePerformanceDataAsync();
        await using var context = _fixture.CreateDbContext();
        var service = new PaymentManagementService(context);

        var result = await service.GetPaymentExportAsync(new AdminPaymentQuery
        {
            Keyword = "PERF-20260721-"
        });

        Assert.True(result.IsLimitExceeded);
        Assert.True(result.Total > AdminPaymentExportResult.MaximumRows);
        Assert.Empty(result.Items);
    }
}
