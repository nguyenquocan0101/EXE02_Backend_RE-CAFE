using System.Net;
using System.Text;
using System.Text.Json;
using EXE02_Backend_RE_CAFE.Models;
using EXE02_Backend_RE_CAFE.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace EXE02_Backend_RE_CAFE.Tests;

[Collection(PaymentApiCollection.Name)]
public sealed class PaymentApiContractTests
{
    private readonly PaymentTestFixture _fixture;

    public PaymentApiContractTests(PaymentTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task PaymentEndpoints_EnforceAnonymousCustomerStaffAndAdminRoleMatrix()
    {
        var tag = $"ROLE-{Guid.NewGuid():N}"[..17];
        var payment = await _fixture.CreatePaymentAsync(
            $"{tag}-ORDER",
            PaymentStatus.Paid,
            210_000m,
            transactionCode: $"{tag}-TXN");

        using var anonymous = _fixture.CreateAnonymousClient();
        using var customer = _fixture.CreateAuthenticatedClient(UserRole.Customer);
        using var staff = _fixture.CreateAuthenticatedClient(UserRole.Staff);
        using var admin = _fixture.CreateAuthenticatedClient(UserRole.Admin);

        Assert.Equal(HttpStatusCode.Unauthorized, (await anonymous.GetAsync("/api/admin/payments")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await customer.GetAsync("/api/admin/payments")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await customer.GetAsync("/api/admin/payments/summary")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await customer.GetAsync("/api/admin/payments/export")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await customer.GetAsync($"/api/admin/payments/{payment.PaymentId}")).StatusCode);

        Assert.Equal(HttpStatusCode.OK, (await staff.GetAsync($"/api/admin/payments?keyword={tag}")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await staff.GetAsync($"/api/admin/payments/{payment.PaymentId}")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await staff.GetAsync($"/api/admin/payments/summary?keyword={tag}")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await staff.GetAsync($"/api/admin/payments/export?keyword={tag}")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await admin.GetAsync($"/api/admin/payments?keyword={tag}")).StatusCode);
    }

    [Fact]
    public async Task ListDetailAndSummary_ReturnTheDocumentedFilteredContract()
    {
        var tag = $"CONTRACT-{Guid.NewGuid():N}"[..21];
        var payment = await _fixture.CreatePaymentAsync(
            $"{tag}-ORDER",
            PaymentStatus.Paid,
            325_000m,
            PaymentMethod.BankTransfer,
            $"{tag}-TXN");
        using var client = _fixture.CreateAuthenticatedClient(UserRole.Admin);

        using var listResponse = await client.GetAsync($"/api/admin/payments?page=1&pageSize=20&keyword={tag}&status=Paid&method=BankTransfer");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        using var listDocument = JsonDocument.Parse(await listResponse.Content.ReadAsStringAsync());
        var page = listDocument.RootElement.GetProperty("data");
        Assert.Equal(1, page.GetProperty("total").GetInt32());
        Assert.Equal(1, page.GetProperty("page").GetInt32());
        Assert.Equal(20, page.GetProperty("pageSize").GetInt32());
        Assert.Equal(1, page.GetProperty("totalPages").GetInt32());
        var row = page.GetProperty("items")[0];
        Assert.Equal(payment.PaymentId, row.GetProperty("id").GetGuid());
        Assert.Equal(payment.OrderId, row.GetProperty("orderId").GetGuid());
        Assert.Equal(payment.OrderCode, row.GetProperty("orderCode").GetString());
        Assert.Equal("BankTransfer", row.GetProperty("paymentMethod").GetString());
        Assert.Equal("Paid", row.GetProperty("paymentStatus").GetString());
        Assert.Equal(325_000m, row.GetProperty("amount").GetDecimal());
        Assert.Equal($"{tag}-TXN", row.GetProperty("transactionCode").GetString());

        using var detailResponse = await client.GetAsync($"/api/admin/payments/{payment.PaymentId}");
        Assert.Equal(HttpStatusCode.OK, detailResponse.StatusCode);
        using var detailDocument = JsonDocument.Parse(await detailResponse.Content.ReadAsStringAsync());
        var detail = detailDocument.RootElement.GetProperty("data");
        Assert.Equal(payment.PaymentId, detail.GetProperty("id").GetGuid());
        Assert.Equal(payment.OrderId, detail.GetProperty("orderId").GetGuid());
        Assert.True(detail.TryGetProperty("createdAt", out _));
        Assert.True(detail.TryGetProperty("paidAt", out _));

        using var summaryResponse = await client.GetAsync($"/api/admin/payments/summary?keyword={tag}");
        Assert.Equal(HttpStatusCode.OK, summaryResponse.StatusCode);
        using var summaryDocument = JsonDocument.Parse(await summaryResponse.Content.ReadAsStringAsync());
        var summary = summaryDocument.RootElement.GetProperty("data");
        Assert.Equal(1, summary.GetProperty("paidCount").GetInt32());
        Assert.Equal(0, summary.GetProperty("unpaidCount").GetInt32());
        Assert.Equal(325_000m, summary.GetProperty("paidAmount").GetDecimal());

        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/api/admin/payments/{Guid.NewGuid()}")).StatusCode);
    }

    [Fact]
    public async Task Export_ReturnsUtf8BomStableColumnsAndNeutralizesSpreadsheetFormulas()
    {
        var tag = $"CSV-{Guid.NewGuid():N}"[..16];
        await _fixture.CreatePaymentForCustomerAsync(
            $"{tag}-ORDER",
            "=2+3 Việt",
            PaymentStatus.Paid,
            450_000m,
            "@unsafe-transaction");
        using var client = _fixture.CreateAuthenticatedClient(UserRole.Admin);

        using var response = await client.GetAsync($"/api/admin/payments/export?keyword={tag}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/csv", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal("utf-8", response.Content.Headers.ContentType?.CharSet);
        Assert.Contains("payment-transactions-", response.Content.Headers.ContentDisposition?.FileNameStar ?? response.Content.Headers.ContentDisposition?.FileName);
        var bytes = await response.Content.ReadAsByteArrayAsync();
        Assert.True(bytes.Length > 3);
        Assert.Equal(new byte[] { 0xEF, 0xBB, 0xBF }, bytes[..3]);
        var csv = Encoding.UTF8.GetString(bytes);
        Assert.StartsWith("\uFEFF\"Payment ID\",\"Order ID\",\"Order Code\"", csv, StringComparison.Ordinal);
        Assert.Contains("\"'=2+3 Việt\"", csv, StringComparison.Ordinal);
        Assert.Contains("\"'@unsafe-transaction\"", csv, StringComparison.Ordinal);
        Assert.Contains("\"450000\"", csv, StringComparison.Ordinal);
        Assert.DoesNotContain("\"=2+3 Việt\"", csv, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ListDetailAndExport_WriteAuditEvidenceIncludingRejectedExport()
    {
        var tag = $"AUDIT-{Guid.NewGuid():N}"[..18];
        var payment = await _fixture.CreatePaymentAsync(
            $"{tag}-ORDER",
            PaymentStatus.Paid,
            180_000m,
            transactionCode: $"{tag}-TXN");
        using var client = _fixture.CreateAuthenticatedClient(UserRole.Admin);

        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync($"/api/admin/payments?keyword={tag}")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync($"/api/admin/payments/{payment.PaymentId}")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync($"/api/admin/payments/export?keyword={tag}")).StatusCode);

        await _fixture.EnsurePerformanceDataAsync();
        using var rejectedExport = await client.GetAsync("/api/admin/payments/export?keyword=PERF-20260721-");
        Assert.Equal(HttpStatusCode.BadRequest, rejectedExport.StatusCode);

        await using var context = _fixture.CreateDbContext();
        var auditLogs = await context.AuditLogs
            .Where(log => log.UserId == _fixture.AdminUserId && log.EntityName == "Payment")
            .ToListAsync();
        Assert.Contains(auditLogs, log => log.Action == "view_list" && log.NewValue!.Contains(tag, StringComparison.Ordinal));
        Assert.Contains(auditLogs, log => log.Action == "view_detail" && log.EntityId == payment.PaymentId);
        Assert.Contains(auditLogs, log => log.Action == "export" && log.NewValue!.Contains(tag, StringComparison.Ordinal));
        Assert.Contains(auditLogs, log => log.Action == "export"
            && log.NewValue!.Contains("\"Rejected\":true", StringComparison.Ordinal)
            && log.NewValue.Contains("\"Limit\":10000", StringComparison.Ordinal));
    }
}
