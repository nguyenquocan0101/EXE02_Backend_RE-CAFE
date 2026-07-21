using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using EXE02_Backend_RE_CAFE.DTOs;
using EXE02_Backend_RE_CAFE.Models;
using EXE02_Backend_RE_CAFE.Services;
using EXE02_Backend_RE_CAFE.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace EXE02_Backend_RE_CAFE.Tests;

[Collection(PaymentApiCollection.Name)]
public sealed class SepayPaymentRegressionTests
{
    private static int _orderSequence = Random.Shared.Next(1000, 8000);
    private readonly PaymentTestFixture _fixture;

    public SepayPaymentRegressionTests(PaymentTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task SuccessfulWebhook_WhenReplayed_RemainsOneSettlementAndOnePaidSummaryAmount()
    {
        var orderCode = NewWebhookOrderCode();
        var payment = await _fixture.CreatePaymentAsync(orderCode, PaymentStatus.Unpaid, 275_000m);
        var request = CreateWebhook(orderCode, 275_000m, $"REF-{Guid.NewGuid():N}");
        using var client = _fixture.CreateAnonymousClient();
        client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Apikey {PaymentApiFactory.SepayApiKey}");

        using var firstResponse = await client.PostAsJsonAsync("/api/sepay-webhook", request);
        using var secondResponse = await client.PostAsJsonAsync("/api/sepay-webhook", request);

        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, secondResponse.StatusCode);
        Assert.True(await ReadSuccessAsync(firstResponse));
        Assert.True(await ReadSuccessAsync(secondResponse));

        await using var context = _fixture.CreateDbContext();
        var persistedPayments = await context.Payments.Where(item => item.OrderId == payment.OrderId).ToListAsync();
        var persistedPayment = Assert.Single(persistedPayments);
        Assert.Equal(PaymentStatus.Paid, persistedPayment.Status);
        Assert.Equal(275_000m, persistedPayment.Amount);
        Assert.Equal(request.ReferenceCode, persistedPayment.TransactionCode);
        Assert.Equal(PaymentStatus.Paid, await context.Orders.Where(order => order.Id == payment.OrderId).Select(order => order.PaymentStatus).SingleAsync());

        var summary = await new PaymentManagementService(context).GetPaymentSummaryAsync(new AdminPaymentQuery { Keyword = orderCode });
        Assert.Equal(1, summary.PaidCount);
        Assert.Equal(275_000m, summary.PaidAmount);
    }

    [Fact]
    public async Task SuccessfulWebhook_WhenDeliveredConcurrently_RemainsIdempotentWithoutServerErrors()
    {
        var orderCode = NewWebhookOrderCode();
        var payment = await _fixture.CreatePaymentAsync(orderCode, PaymentStatus.Unpaid, 425_000m);
        var request = CreateWebhook(orderCode, 425_000m, $"REF-{Guid.NewGuid():N}");
        using var client = _fixture.CreateAnonymousClient();
        client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Apikey {PaymentApiFactory.SepayApiKey}");

        var responses = await Task.WhenAll(Enumerable.Range(0, 8)
            .Select(_ => client.PostAsJsonAsync("/api/sepay-webhook", request)));

        try
        {
            Assert.All(responses, response => Assert.Equal(HttpStatusCode.OK, response.StatusCode));
            foreach (var response in responses)
            {
                Assert.True(await ReadSuccessAsync(response));
            }
        }
        finally
        {
            foreach (var response in responses)
            {
                response.Dispose();
            }
        }

        await using var context = _fixture.CreateDbContext();
        var persistedPayment = Assert.Single(await context.Payments.Where(item => item.OrderId == payment.OrderId).ToListAsync());
        Assert.Equal(PaymentStatus.Paid, persistedPayment.Status);
        Assert.Equal(request.ReferenceCode, persistedPayment.TransactionCode);
        Assert.Equal(425_000m, persistedPayment.Amount);
    }

    [Fact]
    public async Task SameBankReference_WhenDeliveredForTwoOrders_SettlesOnlyOneOrder()
    {
        var firstOrderCode = NewWebhookOrderCode();
        var secondOrderCode = NewWebhookOrderCode();
        var firstPayment = await _fixture.CreatePaymentAsync(firstOrderCode, PaymentStatus.Unpaid, 180_000m);
        var secondPayment = await _fixture.CreatePaymentAsync(secondOrderCode, PaymentStatus.Unpaid, 180_000m);
        var sharedReference = $"REF-{Guid.NewGuid():N}";
        using var firstClient = _fixture.CreateAnonymousClient();
        using var secondClient = _fixture.CreateAnonymousClient();
        firstClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Apikey {PaymentApiFactory.SepayApiKey}");
        secondClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Apikey {PaymentApiFactory.SepayApiKey}");

        var responses = await Task.WhenAll(
            firstClient.PostAsJsonAsync("/api/sepay-webhook", CreateWebhook(firstOrderCode, 180_000m, sharedReference)),
            secondClient.PostAsJsonAsync("/api/sepay-webhook", CreateWebhook(secondOrderCode, 180_000m, sharedReference)));

        try
        {
            Assert.All(responses, response => Assert.Equal(HttpStatusCode.OK, response.StatusCode));
            var results = await Task.WhenAll(responses.Select(ReadSuccessAsync));
            Assert.Single(results, success => success);
            Assert.Single(results, success => !success);
        }
        finally
        {
            foreach (var response in responses)
            {
                response.Dispose();
            }
        }

        await using var context = _fixture.CreateDbContext();
        var payments = await context.Payments
            .Where(item => item.Id == firstPayment.PaymentId || item.Id == secondPayment.PaymentId)
            .ToListAsync();
        Assert.Single(payments, item => item.Status == PaymentStatus.Paid && item.TransactionCode == sharedReference);
        Assert.Single(payments, item => item.Status == PaymentStatus.Unpaid && item.TransactionCode == null);
    }

    [Fact]
    public async Task Webhook_WithInsufficientAmount_LeavesOrderAndPaymentUnpaid()
    {
        var orderCode = NewWebhookOrderCode();
        var payment = await _fixture.CreatePaymentAsync(orderCode, PaymentStatus.Unpaid, 300_000m);
        var request = CreateWebhook(orderCode, 299_999m, $"REF-{Guid.NewGuid():N}");
        using var client = _fixture.CreateAnonymousClient();
        client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Apikey {PaymentApiFactory.SepayApiKey}");

        using var response = await client.PostAsJsonAsync("/api/sepay-webhook", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.False(await ReadSuccessAsync(response));
        await using var context = _fixture.CreateDbContext();
        Assert.Equal(PaymentStatus.Unpaid, await context.Payments.Where(item => item.Id == payment.PaymentId).Select(item => item.Status).SingleAsync());
        Assert.Equal(PaymentStatus.Unpaid, await context.Orders.Where(order => order.Id == payment.OrderId).Select(order => order.PaymentStatus).SingleAsync());
    }

    [Fact]
    public void QrGeneration_UsesOrderAndAmountWithoutEmbeddingWebhookCredential()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Sepay:BankAccount"] = "123456789",
            ["Sepay:BankName"] = "MBBank",
            ["Sepay:QrPrefix"] = "RECAFE",
            ["Sepay:ApiKey"] = "must-not-appear"
        }).Build();
        using var context = _fixture.CreateDbContext();
        var service = new PaymentService(context, configuration);

        var url = service.GetPaymentQrUrl("ORD-20260721-9999", 123_456m);

        Assert.Contains("amount=123456", url, StringComparison.Ordinal);
        Assert.Contains("RECAFE%20ORD-20260721-9999", url, StringComparison.Ordinal);
        Assert.DoesNotContain("must-not-appear", url, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Webhook_WithInvalidCredential_IsRejected()
    {
        using var client = _fixture.CreateAnonymousClient();
        client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Apikey invalid-test-key");

        using var response = await client.PostAsJsonAsync(
            "/api/sepay-webhook",
            CreateWebhook(NewWebhookOrderCode(), 100_000m, $"REF-{Guid.NewGuid():N}"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Webhook_WithOversizedReference_IsRejectedBeforePersistence()
    {
        using var client = _fixture.CreateAnonymousClient();
        client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Apikey {PaymentApiFactory.SepayApiKey}");
        var request = CreateWebhook(NewWebhookOrderCode(), 100_000m, new string('X', 101));

        using var response = await client.PostAsJsonAsync("/api/sepay-webhook", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private static SepayWebhookRequest CreateWebhook(string orderCode, decimal amount, string referenceCode)
    {
        return new SepayWebhookRequest
        {
            Id = Random.Shared.NextInt64(1, long.MaxValue),
            TransferType = "in",
            TransferAmount = amount,
            Code = orderCode,
            Content = $"Payment for {orderCode}",
            ReferenceCode = referenceCode,
            TransactionDate = "2026-07-21 12:00:00"
        };
    }

    private static string NewWebhookOrderCode()
    {
        var suffix = Interlocked.Increment(ref _orderSequence);
        return $"ORD-20260721-{suffix:D4}";
    }

    private static async Task<bool> ReadSuccessAsync(HttpResponseMessage response)
    {
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return document.RootElement.GetProperty("success").GetBoolean();
    }
}
