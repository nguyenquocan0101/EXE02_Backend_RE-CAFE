using System.Diagnostics;
using EXE02_Backend_RE_CAFE.DTOs;
using EXE02_Backend_RE_CAFE.Models;
using EXE02_Backend_RE_CAFE.Services;
using EXE02_Backend_RE_CAFE.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace EXE02_Backend_RE_CAFE.Tests;

[Collection(PaymentApiCollection.Name)]
public sealed class ProductStoryPerformanceIntegrationTests
{
    private const int StoryCount = 10_000;
    private readonly PaymentTestFixture _fixture;

    public ProductStoryPerformanceIntegrationTests(PaymentTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task PublicSlugLookupWithTenThousandStoriesStaysBelowP95Budget()
    {
        await using var context = _fixture.CreateDbContext();
        var suffix = Guid.NewGuid().ToString("N");
        var category = new Category { Name = $"Performance {suffix}", Slug = $"performance-{suffix}" };
        var coffeeType = await context.CoffeeTypes.SingleAsync(item => item.Slug == "arabica");
        context.Categories.Add(category);

        var products = new List<Product>(StoryCount);
        var stories = new List<ProductStory>(StoryCount);
        for (var index = 0; index < StoryCount; index++)
        {
            var product = new Product
            {
                CategoryId = category.Id,
                Name = $"Performance lamp {suffix}-{index}",
                Slug = $"performance-lamp-{suffix}-{index}",
                SKU = $"PERF-{suffix[..8]}-{index:D5}",
                Price = 100_000m,
                IsActive = true
            };
            products.Add(product);
            stories.Add(new ProductStory
            {
                ProductId = product.Id,
                CoffeeTypeId = coffeeType.Id,
                Slug = $"performance-{suffix}-{index}",
                ContentHtmlVi = "<p>Performance story</p>",
                ContentHtmlEn = "<p>Performance story</p>",
                IsPublished = true
            });
        }

        context.ChangeTracker.AutoDetectChangesEnabled = false;
        context.Products.AddRange(products);
        context.ProductStories.AddRange(stories);
        await context.SaveChangesAsync();
        context.ChangeTracker.AutoDetectChangesEnabled = true;

        var service = new ProductStoryService(
            context,
            new StoryHtmlSanitizer(),
            Options.Create(new TraceabilitySettings { PublicBaseUrl = "https://www.recafe.site" }),
            new TestHostEnvironment());
        var targetSlug = $"performance-{suffix}-{StoryCount - 1}";
        _ = await service.GetPublishedBySlugAsync(targetSlug);

        var durations = new List<long>();
        for (var index = 0; index < 30; index++)
        {
            var stopwatch = Stopwatch.StartNew();
            var result = await service.GetPublishedBySlugAsync(targetSlug);
            stopwatch.Stop();
            Assert.NotNull(result);
            durations.Add(stopwatch.ElapsedMilliseconds);
        }

        var p95 = durations.OrderBy(value => value).ElementAt((int)Math.Ceiling(durations.Count * 0.95) - 1);
        Assert.True(p95 < 500, $"Expected p95 below 500ms, observed {p95}ms. Samples: {string.Join(",", durations)}");
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
