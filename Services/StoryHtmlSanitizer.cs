using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Ganss.Xss;
using EXE02_Backend_RE_CAFE.Exceptions;
using EXE02_Backend_RE_CAFE.Interfaces;

namespace EXE02_Backend_RE_CAFE.Services
{
    public sealed class StoryHtmlSanitizer : IStoryHtmlSanitizer
    {
        private const int MaxHtmlLength = 50_000;
        private static readonly ISet<string> AllowedTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "h1", "h2", "h3", "h4", "p", "strong", "em", "ul", "ol", "li", "blockquote", "a", "br", "hr"
        };

        public string SanitizeAndValidate(string rawHtml, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(rawHtml))
            {
                throw new BadRequestException($"{fieldName} must contain meaningful HTML content.");
            }

            if (rawHtml.Length > MaxHtmlLength)
            {
                throw new BadRequestException($"{fieldName} must be 50,000 characters or fewer.");
            }

            var sanitizer = new HtmlSanitizer();
            sanitizer.AllowedTags.Clear();
            foreach (var tag in AllowedTags)
            {
                sanitizer.AllowedTags.Add(tag);
            }

            sanitizer.AllowedAttributes.Clear();
            sanitizer.AllowedAttributes.Add("href");
            sanitizer.AllowedSchemes.Clear();
            sanitizer.AllowedSchemes.Add("http");
            sanitizer.AllowedSchemes.Add("https");
            sanitizer.AllowedCssProperties.Clear();

            var sanitized = sanitizer.Sanitize(rawHtml).Trim();
            sanitized = Regex.Replace(
                sanitized,
                "<a\\b([^>]*)>",
                "<a$1 target=\"_blank\" rel=\"noopener noreferrer\">",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

            var text = WebUtility.HtmlDecode(Regex.Replace(sanitized, "<[^>]+>", " "));
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new BadRequestException($"{fieldName} must contain meaningful text.");
            }

            return sanitized;
        }
    }
}
