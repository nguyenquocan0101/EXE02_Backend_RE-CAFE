using EXE02_Backend_RE_CAFE.DTOs;
using EXE02_Backend_RE_CAFE.Exceptions;
using EXE02_Backend_RE_CAFE.Models;
using EXE02_Backend_RE_CAFE.Services;
using EXE02_Backend_RE_CAFE.Tests.Infrastructure;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;

namespace EXE02_Backend_RE_CAFE.Tests;

[Collection(PaymentApiCollection.Name)]
public sealed class ProductStoryServiceIntegrationTests
{
    private readonly PaymentTestFixture _fixture;

    public ProductStoryServiceIntegrationTests(PaymentTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Create_SanitizesContentPublishesStoryAndCreatesSharedQr()
    {
        var product = await CreateProductAsync();
        var coffeeTypeId = await GetCoffeeTypeIdAsync("arabica");
        await using var context = _fixture.CreateDbContext();
        var service = CreateService(context);

        var result = await service.CreateAsync(new CreateProductStoryRequest
        {
            ProductId = product.Id,
            CoffeeTypeId = coffeeTypeId,
            ContentHtmlVi = "<h2>Arabica</h2><p onclick=\"alert(1)\">Bền vững</p><script>alert(1)</script>",
            ContentHtmlEn = "<h2>Arabica</h2><p>Reusable lamp</p>"
        });

        Assert.Equal($"arabica-and-{product.Slug}", result.Slug);
        Assert.True(result.IsPublished);
        Assert.Equal("https://www.recafe.site/" + result.Slug, result.LandingPageUrl);
        Assert.Equal(1, result.SharedQrCount);
        Assert.DoesNotContain("script", result.ContentHtmlVi, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("onclick", result.ContentHtmlVi, StringComparison.OrdinalIgnoreCase);

        var persisted = await context.ProductStories
            .Include(item => item.QRCodes)
            .SingleAsync(item => item.Id == result.Id);
        var qr = Assert.Single(persisted!.QRCodes);
        Assert.True(qr.IsShared);
        Assert.Null(qr.ScanLimit);
        Assert.Equal(result.LandingPageUrl, qr.QRValue);
    }

    [Fact]
    public async Task Create_DuplicateProductAndCoffeeTypeReturnsConflict()
    {
        var product = await CreateProductAsync();
        var coffeeTypeId = await GetCoffeeTypeIdAsync("robusta");
        await using var firstContext = _fixture.CreateDbContext();
        var firstService = CreateService(firstContext);
        var request = new CreateProductStoryRequest
        {
            ProductId = product.Id,
            CoffeeTypeId = coffeeTypeId,
            ContentHtmlVi = "<p>VI</p>",
            ContentHtmlEn = "<p>EN</p>"
        };

        await firstService.CreateAsync(request);

        await using var secondContext = _fixture.CreateDbContext();
        var secondService = CreateService(secondContext);
        await Assert.ThrowsAsync<ConflictException>(() => secondService.CreateAsync(request));
    }

    [Fact]
    public async Task UpdatePreservesStableSlugAndPublicReadSanitizesPersistedHtml()
    {
        var product = await CreateProductAsync();
        var coffeeTypeId = await GetCoffeeTypeIdAsync("liberica");
        await using var context = _fixture.CreateDbContext();
        var service = CreateService(context);
        var created = await service.CreateAsync(new CreateProductStoryRequest
        {
            ProductId = product.Id,
            CoffeeTypeId = coffeeTypeId,
            ContentHtmlVi = "<p>Original VI</p>",
            ContentHtmlEn = "<p>Original EN</p>"
        });

        var updated = await service.UpdateAsync(created.Id, new UpdateProductStoryRequest
        {
            ContentHtmlVi = "<p>Updated VI</p>",
            ContentHtmlEn = "<p>Updated EN</p>"
        });
        Assert.Equal(created.Slug, updated.Slug);
        Assert.Equal(created.LandingPageUrl, updated.LandingPageUrl);

        var persisted = await context.ProductStories.FindAsync(created.Id);
        persisted!.ContentHtmlVi = "<p>safe</p><script>bad()</script>";
        await context.SaveChangesAsync();

        await using var publicContext = _fixture.CreateDbContext();
        var publicStory = await CreateService(publicContext).GetPublishedBySlugAsync(created.Slug);
        Assert.NotNull(publicStory);
        Assert.Contains("safe", publicStory!.ContentHtmlVi, StringComparison.Ordinal);
        Assert.DoesNotContain("script", publicStory.ContentHtmlVi, StringComparison.OrdinalIgnoreCase);
    }

    private async Task<Product> CreateProductAsync()
    {
        await using var context = _fixture.CreateDbContext();
        var suffix = Guid.NewGuid().ToString("N");
        var category = new Category
        {
            Name = $"Traceability category {suffix}",
            Slug = $"traceability-category-{suffix}"
        };
        var product = new Product
        {
            CategoryId = category.Id,
            Name = $"Lamp {suffix}",
            Slug = $"lamp-{suffix}",
            SKU = $"TRC-{suffix[..12]}",
            Price = 100_000m,
            IsActive = true
        };
        context.Categories.Add(category);
        context.Products.Add(product);
        await context.SaveChangesAsync();
        return product;
    }

    private async Task<Guid> GetCoffeeTypeIdAsync(string slug)
    {
        await using var context = _fixture.CreateDbContext();
        return (await context.CoffeeTypes.SingleAsync(item => item.Slug == slug)).Id;
    }

    private ProductStoryService CreateService(EXE02_Backend_RE_CAFE.Data.ApplicationDbContext context)
    {
        return new ProductStoryService(
            context,
            new StoryHtmlSanitizer(),
            Options.Create(new TraceabilitySettings { PublicBaseUrl = "https://www.recafe.site" }),
            new TestHostEnvironment());
    }

    private sealed class TestHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = "Testing";
        public string ApplicationName { get; set; } = "EXE02_Backend_RE_CAFE.Tests";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
        public string WebRootPath { get; set; } = AppContext.BaseDirectory;
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
    }
}
