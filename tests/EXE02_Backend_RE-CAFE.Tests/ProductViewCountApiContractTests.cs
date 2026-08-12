using System.Net;
using System.Text.Json;
using EXE02_Backend_RE_CAFE.Models;
using EXE02_Backend_RE_CAFE.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace EXE02_Backend_RE_CAFE.Tests;

[Collection(PaymentApiCollection.Name)]
public sealed class ProductViewCountApiContractTests
{
    private readonly PaymentTestFixture _fixture;

    public ProductViewCountApiContractTests(PaymentTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task ActiveProductViewIncrementsAndReturnsNewCount()
    {
        var product = await CreateProductAsync(viewCount: 5);
        using var client = _fixture.CreateAnonymousClient();

        var response = await client.PostAsync($"/api/products/{product.Id}/view", content: null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(6, document.RootElement.GetProperty("data").GetProperty("viewCount").GetInt32());

        await using var context = _fixture.CreateDbContext();
        var persisted = await context.Products.FindAsync(product.Id);
        Assert.Equal(6, persisted!.ViewCount);
    }

    [Fact]
    public async Task MissingOrInactiveProductViewReturnsNotFoundWithoutMutation()
    {
        var inactive = await CreateProductAsync(viewCount: 9, isActive: false);
        using var client = _fixture.CreateAnonymousClient();

        var inactiveResponse = await client.PostAsync($"/api/products/{inactive.Id}/view", content: null);
        var missingResponse = await client.PostAsync($"/api/products/{Guid.NewGuid()}/view", content: null);

        Assert.Equal(HttpStatusCode.NotFound, inactiveResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, missingResponse.StatusCode);

        await using var context = _fixture.CreateDbContext();
        var persisted = await context.Products.FindAsync(inactive.Id);
        Assert.Equal(9, persisted!.ViewCount);
    }

    [Fact]
    public async Task PublicListAndDetailExposeTheSameViewCount()
    {
        var product = await CreateProductAsync(viewCount: 17);
        using var client = _fixture.CreateAnonymousClient();

        var listResponse = await client.GetAsync("/api/products");
        var detailResponse = await client.GetAsync($"/api/products/slug/{product.Slug}");

        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, detailResponse.StatusCode);

        using var listDocument = JsonDocument.Parse(await listResponse.Content.ReadAsStringAsync());
        using var detailDocument = JsonDocument.Parse(await detailResponse.Content.ReadAsStringAsync());
        var listedProduct = listDocument.RootElement.GetProperty("data")
            .EnumerateArray()
            .Single(item => item.GetProperty("id").GetGuid() == product.Id);

        Assert.Equal(17, listedProduct.GetProperty("viewCount").GetInt32());
        Assert.Equal(17, detailDocument.RootElement.GetProperty("data").GetProperty("viewCount").GetInt32());
    }

    [Fact]
    public async Task ConcurrentViewsAreNotLost()
    {
        var product = await CreateProductAsync(viewCount: 20);
        using var client = _fixture.CreateAnonymousClient();

        var responses = await Task.WhenAll(
            Enumerable.Range(0, 12)
                .Select(_ => client.PostAsync($"/api/products/{product.Id}/view", content: null)));

        Assert.All(responses, response => Assert.Equal(HttpStatusCode.OK, response.StatusCode));

        await using var context = _fixture.CreateDbContext();
        var persisted = await context.Products.FindAsync(product.Id);
        Assert.Equal(32, persisted!.ViewCount);
    }

    [Fact]
    public async Task InitialCatalogProductsUseRequestedViewCounts()
    {
        await using var context = _fixture.CreateDbContext();

        var first = await context.Products.SingleAsync(product => product.SKU == "RE-0001");
        var second = await context.Products.SingleAsync(product => product.SKU == "RE-0002");

        Assert.Equal(125, first.ViewCount);
        Assert.Equal(178, second.ViewCount);
    }

    private async Task<Product> CreateProductAsync(int viewCount, bool isActive = true)
    {
        await using var context = _fixture.CreateDbContext();
        var suffix = Guid.NewGuid().ToString("N");
        var category = new Category
        {
            Name = $"View count category {suffix}",
            Slug = $"view-count-category-{suffix}"
        };
        var product = new Product
        {
            CategoryId = category.Id,
            Name = $"View count product {suffix}",
            Slug = $"view-count-product-{suffix}",
            SKU = $"VIEW-{suffix[..12]}",
            Price = 100_000m,
            ViewCount = viewCount,
            IsActive = isActive
        };

        context.Categories.Add(category);
        context.Products.Add(product);
        await context.SaveChangesAsync();
        return product;
    }
}
