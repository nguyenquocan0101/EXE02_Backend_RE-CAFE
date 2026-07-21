using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using EXE02_Backend_RE_CAFE.Models;
using EXE02_Backend_RE_CAFE.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace EXE02_Backend_RE_CAFE.Tests;

[Collection(PaymentApiCollection.Name)]
public sealed class CheckoutPaymentRegressionTests
{
    private readonly PaymentTestFixture _fixture;

    public CheckoutPaymentRegressionTests(PaymentTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Checkout_WithCod_PersistsUnpaidPaymentWithoutQrCode()
    {
        var cartItemId = await _fixture.CreateCartItemAsync(225_000m);
        using var client = _fixture.CreateAuthenticatedClient(UserRole.Customer);

        using var response = await client.PostAsJsonAsync("/api/orders/checkout", CreateCheckoutRequest(PaymentMethod.COD, cartItemId));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var order = document.RootElement.GetProperty("data");
        var orderId = order.GetProperty("id").GetGuid();
        Assert.Equal("COD", order.GetProperty("paymentMethod").GetString());
        Assert.Equal("Unpaid", order.GetProperty("paymentStatus").GetString());
        Assert.Equal(JsonValueKind.Null, order.GetProperty("paymentQrUrl").ValueKind);

        await using var context = _fixture.CreateDbContext();
        var payment = await context.Payments.SingleAsync(item => item.OrderId == orderId);
        Assert.Equal(PaymentMethod.COD, payment.Method);
        Assert.Equal(PaymentStatus.Unpaid, payment.Status);
        Assert.True(payment.CreatedAt > DateTime.UtcNow.AddMinutes(-1));
        Assert.False(await context.CartItems.AnyAsync(item => item.Id == cartItemId));
    }

    [Fact]
    public async Task Checkout_WithBankTransfer_ProfileOrderRemainsPayableWithQrCode()
    {
        var cartItemId = await _fixture.CreateCartItemAsync(310_000m);
        using var client = _fixture.CreateAuthenticatedClient(UserRole.Customer);

        using var checkoutResponse = await client.PostAsJsonAsync(
            "/api/orders/checkout",
            CreateCheckoutRequest(PaymentMethod.BankTransfer, cartItemId));

        Assert.Equal(HttpStatusCode.Created, checkoutResponse.StatusCode);
        using var checkoutDocument = JsonDocument.Parse(await checkoutResponse.Content.ReadAsStringAsync());
        var checkoutOrder = checkoutDocument.RootElement.GetProperty("data");
        var orderId = checkoutOrder.GetProperty("id").GetGuid();
        var orderCode = checkoutOrder.GetProperty("orderCode").GetString();
        Assert.Equal("BankTransfer", checkoutOrder.GetProperty("paymentMethod").GetString());
        Assert.Equal("Unpaid", checkoutOrder.GetProperty("paymentStatus").GetString());
        Assert.Contains("amount=310000", checkoutOrder.GetProperty("paymentQrUrl").GetString(), StringComparison.Ordinal);

        using var profileResponse = await client.GetAsync($"/api/orders/{orderId}");

        Assert.Equal(HttpStatusCode.OK, profileResponse.StatusCode);
        using var profileDocument = JsonDocument.Parse(await profileResponse.Content.ReadAsStringAsync());
        var profileOrder = profileDocument.RootElement.GetProperty("data");
        Assert.Equal("Unpaid", profileOrder.GetProperty("paymentStatus").GetString());
        var qrUrl = profileOrder.GetProperty("paymentQrUrl").GetString();
        Assert.Contains(Uri.EscapeDataString($"RECAFE {orderCode}"), qrUrl, StringComparison.Ordinal);
        Assert.DoesNotContain(PaymentApiFactory.SepayApiKey, qrUrl, StringComparison.Ordinal);
    }

    private static object CreateCheckoutRequest(PaymentMethod method, Guid cartItemId)
    {
        return new
        {
            shippingAddress = new
            {
                receiverName = "Payment Regression Customer",
                phone = "0900000000",
                province = "Ho Chi Minh",
                district = "District 1",
                ward = "Ward 1",
                detailAddress = "Payment regression address",
                isDefault = false
            },
            paymentMethod = method,
            cartItemIds = new[] { cartItemId }
        };
    }
}
