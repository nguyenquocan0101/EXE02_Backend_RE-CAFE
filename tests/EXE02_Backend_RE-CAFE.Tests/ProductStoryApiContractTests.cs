using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using EXE02_Backend_RE_CAFE.Models;
using EXE02_Backend_RE_CAFE.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace EXE02_Backend_RE_CAFE.Tests;

[Collection(PaymentApiCollection.Name)]
public sealed class ProductStoryApiContractTests
{
    private readonly PaymentTestFixture _fixture;

    public ProductStoryApiContractTests(PaymentTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task AdminLifecycleAndAnonymousReadUseExpectedContracts()
    {
        var product = await CreateProductAsync();
        var coffeeTypeId = await GetCoffeeTypeIdAsync("arabica");
        using var anonymous = _fixture.CreateAnonymousClient();
        using var admin = _fixture.CreateAuthenticatedClient(UserRole.Admin);

        var unauthorized = await anonymous.GetAsync("/api/admin/product-stories");
        Assert.Equal(HttpStatusCode.Unauthorized, unauthorized.StatusCode);

        var create = await admin.PostAsJsonAsync("/api/admin/product-stories", new
        {
            productId = product.Id,
            coffeeTypeId,
            contentHtmlVi = "<h2>Arabica</h2><p>Đèn tái chế</p><script>bad()</script>",
            contentHtmlEn = "<h2>Arabica</h2><p>Recycled lamp</p>"
        });
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        using var createdDocument = JsonDocument.Parse(await create.Content.ReadAsStringAsync());
        var createdData = createdDocument.RootElement.GetProperty("data");
        var storyId = createdData.GetProperty("id").GetGuid();
        var slug = createdData.GetProperty("slug").GetString()!;
        Assert.True(createdDocument.RootElement.GetProperty("success").GetBoolean());
        Assert.Equal("https://www.recafe.site/" + slug, createdData.GetProperty("landingPageUrl").GetString());

        var publicResponse = await anonymous.GetAsync("/api/product-stories/" + slug);
        Assert.Equal(HttpStatusCode.OK, publicResponse.StatusCode);
        using var publicDocument = JsonDocument.Parse(await publicResponse.Content.ReadAsStringAsync());
        var publicData = publicDocument.RootElement.GetProperty("data");
        Assert.Equal("Arabica", publicData.GetProperty("coffeeTypeName").GetString());
        Assert.DoesNotContain("script", publicData.GetProperty("contentHtmlVi").GetString()!, StringComparison.OrdinalIgnoreCase);

        var unpublish = await admin.PatchAsJsonAsync($"/api/admin/product-stories/{storyId}/publication", new { isPublished = false });
        Assert.Equal(HttpStatusCode.OK, unpublish.StatusCode);
        var hidden = await anonymous.GetAsync("/api/product-stories/" + slug);
        Assert.Equal(HttpStatusCode.NotFound, hidden.StatusCode);

        var republish = await admin.PatchAsJsonAsync($"/api/admin/product-stories/{storyId}/publication", new { isPublished = true });
        Assert.Equal(HttpStatusCode.OK, republish.StatusCode);
        var visibleAgain = await anonymous.GetAsync("/api/product-stories/" + slug);
        Assert.Equal(HttpStatusCode.OK, visibleAgain.StatusCode);
    }

    [Fact]
    public async Task StaffAndCustomerCannotMutateAndDuplicateReturnsConflict()
    {
        var product = await CreateProductAsync();
        var coffeeTypeId = await GetCoffeeTypeIdAsync("robusta");
        using var staff = _fixture.CreateAuthenticatedClient(UserRole.Staff);
        using var customer = _fixture.CreateAuthenticatedClient(UserRole.Customer);
        using var admin = _fixture.CreateAuthenticatedClient(UserRole.Admin);
        var payload = new
        {
            productId = product.Id,
            coffeeTypeId,
            contentHtmlVi = "<p>VI</p>",
            contentHtmlEn = "<p>EN</p>"
        };

        Assert.Equal(HttpStatusCode.Forbidden, (await staff.PostAsJsonAsync("/api/admin/product-stories", payload)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await customer.PostAsJsonAsync("/api/admin/product-stories", payload)).StatusCode);
        Assert.Equal(HttpStatusCode.Created, (await admin.PostAsJsonAsync("/api/admin/product-stories", payload)).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await admin.PostAsJsonAsync("/api/admin/product-stories", payload)).StatusCode);
    }

    [Fact]
    public async Task AdminListBoundsPageSizeAndFiltersByPublication()
    {
        var product = await CreateProductAsync();
        var coffeeTypeId = await GetCoffeeTypeIdAsync("liberica");
        using var admin = _fixture.CreateAuthenticatedClient(UserRole.Admin);
        var create = await admin.PostAsJsonAsync("/api/admin/product-stories", new
        {
            productId = product.Id,
            coffeeTypeId,
            contentHtmlVi = "<p>VI</p>",
            contentHtmlEn = "<p>EN</p>"
        });
        create.EnsureSuccessStatusCode();

        var response = await admin.GetAsync("/api/admin/product-stories?page=1&pageSize=1000&isPublished=true");
        response.EnsureSuccessStatusCode();
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var data = document.RootElement.GetProperty("data");
        Assert.Equal(100, data.GetProperty("pageSize").GetInt32());
        Assert.All(data.GetProperty("stories").EnumerateArray(), story =>
            Assert.True(story.GetProperty("isPublished").GetBoolean()));
    }

    private async Task<Product> CreateProductAsync()
    {
        await using var context = _fixture.CreateDbContext();
        var suffix = Guid.NewGuid().ToString("N");
        var category = new Category
        {
            Name = $"API traceability category {suffix}",
            Slug = $"api-traceability-category-{suffix}"
        };
        var product = new Product
        {
            CategoryId = category.Id,
            Name = $"API Lamp {suffix}",
            Slug = $"api-lamp-{suffix}",
            SKU = $"API-{suffix[..12]}",
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
