using EXE02_Backend_RE_CAFE.Exceptions;
using EXE02_Backend_RE_CAFE.Services;

namespace EXE02_Backend_RE_CAFE.Tests;

public sealed class StoryHtmlSanitizerTests
{
    private readonly StoryHtmlSanitizer _sanitizer = new();

    [Fact]
    public void RemovesScriptsEventsFormsIframesAndUnsafeLinks()
    {
        var result = _sanitizer.SanitizeAndValidate(
            "<h2>Arabica</h2><p onclick=\"alert(1)\">Good</p><script>alert(1)</script><form><input /></form><iframe src=\"https://evil.test\"></iframe><a href=\"javascript:alert(1)\">read</a>",
            "ContentHtmlVi");

        Assert.Contains("<h2>Arabica</h2>", result, StringComparison.Ordinal);
        Assert.DoesNotContain("script", result, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("onclick", result, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("form", result, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("iframe", result, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("javascript:", result, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("target=\"_blank\"", result, StringComparison.Ordinal);
        Assert.Contains("rel=\"noopener noreferrer\"", result, StringComparison.Ordinal);
    }

    [Fact]
    public void RejectsBlankAndOversizedContent()
    {
        Assert.Throws<BadRequestException>(() => _sanitizer.SanitizeAndValidate("<p> </p>", "ContentHtmlEn"));
        Assert.Throws<BadRequestException>(() => _sanitizer.SanitizeAndValidate(new string('x', 50_001), "ContentHtmlEn"));
    }

    [Fact]
    public void KeepsSemanticMarkupAndStripsInlineCss()
    {
        var result = _sanitizer.SanitizeAndValidate(
            "<p style=\"color:red\"><strong>Bold</strong><em> coffee</em></p><ul><li>Clean</li></ul>",
            "ContentHtmlVi");

        Assert.Contains("<strong>Bold</strong>", result, StringComparison.Ordinal);
        Assert.Contains("<em> coffee</em>", result, StringComparison.Ordinal);
        Assert.Contains("<ul>", result, StringComparison.Ordinal);
        Assert.DoesNotContain("style=", result, StringComparison.OrdinalIgnoreCase);
    }
}
