using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using EXE02_Backend_RE_CAFE.Models;
using EXE02_Backend_RE_CAFE.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace EXE02_Backend_RE_CAFE.Tests;

[Collection(PaymentApiCollection.Name)]
public sealed class ProductStoryPhaseSixIntegrationTests
{
    private readonly PaymentTestFixture _fixture;

    public ProductStoryPhaseSixIntegrationTests(PaymentTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task PageOpenIncrementsSharedQrAtomicallyAndUnavailableStoryReturnsNotFound()
    {
        var product = await CreateProductAsync();
        var coffeeTypeId = await GetCoffeeTypeIdAsync("excelsa");
        using var admin = _fixture.CreateAuthenticatedClient(UserRole.Admin);
        using var anonymous = _fixture.CreateAnonymousClient();
        var create = await admin.PostAsJsonAsync("/api/admin/product-stories", new
        {
            productId = product.Id,
            coffeeTypeId,
            contentHtmlVi = "<p>VI</p>",
            contentHtmlEn = "<p>EN</p>"
        });
        create.EnsureSuccessStatusCode();
        using var document = JsonDocument.Parse(await create.Content.ReadAsStringAsync());
        var slug = document.RootElement.GetProperty("data").GetProperty("slug").GetString()!;
        var storyId = document.RootElement.GetProperty("data").GetProperty("id").GetGuid();

        Assert.Equal(HttpStatusCode.NoContent, (await anonymous.PostAsync($"/api/product-stories/{slug}/open", null)).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await anonymous.PostAsync($"/api/product-stories/{slug}/open", null)).StatusCode);

        await using (var context = _fixture.CreateDbContext())
        {
            var qr = await context.QRCodes.SingleAsync(item => item.ProductStoryId == storyId && item.IsShared);
            Assert.Equal(2, qr.ScanCount);
        }

        Assert.Equal(HttpStatusCode.OK, (await admin.PatchAsJsonAsync($"/api/admin/product-stories/{storyId}/publication", new { isPublished = false })).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await anonymous.PostAsync($"/api/product-stories/{slug}/open", null)).StatusCode);
    }

    [Fact]
    public async Task CoffeeTypeCrudWorksAndPublishedReferencesCannotBeDeactivated()
    {
        using var admin = _fixture.CreateAuthenticatedClient(UserRole.Admin);
        var suffix = Guid.NewGuid().ToString("N");
        var create = await admin.PostAsJsonAsync("/api/admin/coffee-types", new
        {
            name = $"Seasonal {suffix}",
            slug = $"seasonal-{suffix}",
            displayOrder = 99
        });
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        using var createdDocument = JsonDocument.Parse(await create.Content.ReadAsStringAsync());
        var createdId = createdDocument.RootElement.GetProperty("data").GetProperty("id").GetGuid();

        var update = await admin.PutAsJsonAsync($"/api/admin/coffee-types/{createdId}", new
        {
            name = $"Seasonal Updated {suffix}",
            slug = $"seasonal-updated-{suffix}",
            displayOrder = 100
        });
        Assert.Equal(HttpStatusCode.OK, update.StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await admin.PatchAsJsonAsync($"/api/admin/coffee-types/{createdId}/active", new { isActive = false })).StatusCode);

        var product = await CreateProductAsync();
        var arabicaId = await GetCoffeeTypeIdAsync("arabica");
        var storyCreate = await admin.PostAsJsonAsync("/api/admin/product-stories", new
        {
            productId = product.Id,
            coffeeTypeId = arabicaId,
            contentHtmlVi = "<p>VI</p>",
            contentHtmlEn = "<p>EN</p>"
        });
        storyCreate.EnsureSuccessStatusCode();

        var deactivateReferenced = await admin.PatchAsJsonAsync($"/api/admin/coffee-types/{arabicaId}/active", new { isActive = false });
        Assert.Equal(HttpStatusCode.Conflict, deactivateReferenced.StatusCode);
    }

    private async Task<Product> CreateProductAsync()
    {
        await using var context = _fixture.CreateDbContext();
        var suffix = Guid.NewGuid().ToString("N");
        var category = new Category
        {
            Name = $"Phase six category {suffix}",
            Slug = $"phase-six-category-{suffix}"
        };
        var product = new Product
        {
            CategoryId = category.Id,
            Name = $"Phase six lamp {suffix}",
            Slug = $"phase-six-lamp-{suffix}",
            SKU = $"P6-{suffix[..12]}",
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
}
