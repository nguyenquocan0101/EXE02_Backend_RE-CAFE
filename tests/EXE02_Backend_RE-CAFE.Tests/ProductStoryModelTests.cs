using EXE02_Backend_RE_CAFE.Data;
using EXE02_Backend_RE_CAFE.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace EXE02_Backend_RE_CAFE.Tests;

public sealed class ProductStoryModelTests
{
    [Fact]
    public void CoffeeType_ContainsStableCatalogFields()
    {
        var coffeeType = new CoffeeType();

        Assert.NotEqual(Guid.Empty, coffeeType.Id);
        Assert.Equal(string.Empty, coffeeType.Name);
        Assert.Equal(string.Empty, coffeeType.Slug);
        Assert.True(coffeeType.IsActive);
        Assert.Empty(coffeeType.ProductStories);
    }

    [Fact]
    public void CoffeeType_SeedsEightActiveCatalogOptions()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql("Host=localhost;Database=unused;Username=unused;Password=unused")
            .Options;

        using var context = new ApplicationDbContext(options);
        var seedData = context.GetService<IDesignTimeModel>().Model
            .FindEntityType(typeof(CoffeeType))!
            .GetSeedData();

        Assert.Equal(8, seedData.Count());
        Assert.All(seedData, row => Assert.True((bool)row[nameof(CoffeeType.IsActive)]!));
        Assert.Equal(
            new[] { "arabica", "robusta", "liberica", "excelsa", "culi", "moka", "catimor", "blend" },
            seedData.Select(row => (string)row[nameof(CoffeeType.Slug)]!).ToArray());
    }

    [Fact]
    public void ProductStory_ContainsBilingualPublishedPageFields()
    {
        var story = new ProductStory();

        Assert.Equal(Guid.Empty, story.ProductId);
        Assert.Equal(Guid.Empty, story.CoffeeTypeId);
        Assert.Equal(string.Empty, story.Slug);
        Assert.Equal(string.Empty, story.ContentHtmlVi);
        Assert.Equal(string.Empty, story.ContentHtmlEn);
        Assert.False(story.IsPublished);
        Assert.NotEqual(default, story.CreatedAt);
        Assert.Empty(story.QRCodes);
    }

    [Fact]
    public void ProductStory_UsesManyStoriesPerProductAndUniquePairIndex()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql("Host=localhost;Database=unused;Username=unused;Password=unused")
            .Options;

        using var context = new ApplicationDbContext(options);
        var productNavigation = context.Model.FindEntityType(typeof(Product))!
            .FindNavigation(nameof(Product.ProductStories));
        var pairIndex = context.Model.FindEntityType(typeof(ProductStory))!
            .GetIndexes()
            .Single(index => index.Properties.Select(property => property.Name)
                .SequenceEqual(new[] { nameof(ProductStory.ProductId), nameof(ProductStory.CoffeeTypeId) }));

        Assert.NotNull(productNavigation);
        Assert.True(productNavigation!.IsCollection);
        Assert.True(pairIndex.IsUnique);
    }

    [Fact]
    public void SharedQr_UsesNullableUnlimitedScanLimit()
    {
        var qr = new QRCode
        {
            IsShared = true,
            ScanLimit = null
        };

        Assert.True(qr.IsShared);
        Assert.Null(qr.ScanLimit);
    }
}
