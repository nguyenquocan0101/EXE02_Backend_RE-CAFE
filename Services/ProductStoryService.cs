using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using EXE02_Backend_RE_CAFE.Data;
using EXE02_Backend_RE_CAFE.DTOs;
using EXE02_Backend_RE_CAFE.Exceptions;
using EXE02_Backend_RE_CAFE.Interfaces;
using EXE02_Backend_RE_CAFE.Models;

namespace EXE02_Backend_RE_CAFE.Services
{
    public sealed class ProductStoryService : IProductStoryService
    {
        private const int MaxStorySlugLength = 200;
        private const int MaxQrValueLength = 250;
        private readonly ApplicationDbContext _context;
        private readonly IStoryHtmlSanitizer _sanitizer;
        private readonly TraceabilitySettings _settings;
        private readonly IHostEnvironment _environment;

        public ProductStoryService(
            ApplicationDbContext context,
            IStoryHtmlSanitizer sanitizer,
            IOptions<TraceabilitySettings> settings,
            IHostEnvironment environment)
        {
            _context = context;
            _sanitizer = sanitizer;
            _settings = settings.Value;
            _environment = environment;
        }

        public async Task<ProductStoryPublicDto?> GetPublishedBySlugAsync(string slug)
        {
            var normalizedSlug = slug.Trim().ToLowerInvariant();
            var story = await _context.ProductStories
                .AsNoTracking()
                .Where(item => item.Slug == normalizedSlug
                    && item.IsPublished
                    && item.Product != null
                    && item.Product.IsActive
                    && item.CoffeeType != null
                    && item.CoffeeType.IsActive)
                .Select(item => new ProductStoryPublicDto
                {
                    Slug = item.Slug,
                    ProductName = item.Product!.Name,
                    ProductSlug = item.Product.Slug,
                    CoffeeTypeName = item.CoffeeType!.Name,
                    CoffeeTypeSlug = item.CoffeeType.Slug,
                    ContentHtmlVi = item.ContentHtmlVi,
                    ContentHtmlEn = item.ContentHtmlEn,
                    LandingPageUrl = item.QRCodes
                        .Where(qr => qr.IsShared && qr.IsActive)
                        .Select(qr => qr.LandingPageUrl)
                        .FirstOrDefault() ?? string.Empty,
                    UpdatedAt = item.UpdatedAt
                })
                .SingleOrDefaultAsync();

            if (story == null)
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(story.LandingPageUrl))
            {
                story.LandingPageUrl = BuildCanonicalUrl(story.Slug);
            }

            story.ContentHtmlVi = _sanitizer.SanitizeAndValidate(story.ContentHtmlVi, "ContentHtmlVi");
            story.ContentHtmlEn = _sanitizer.SanitizeAndValidate(story.ContentHtmlEn, "ContentHtmlEn");
            return story;
        }

        public async Task<ProductStoryAdminPageDto> GetAdminStoriesAsync(ProductStoryQueryParameters parameters)
        {
            var page = Math.Max(parameters.Page, 1);
            var pageSize = parameters.PageSize <= 0 ? 20 : Math.Min(parameters.PageSize, 100);
            var query = _context.ProductStories.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(parameters.Keyword))
            {
                var keyword = parameters.Keyword.Trim().ToLowerInvariant();
                query = query.Where(item => item.Slug.ToLower().Contains(keyword)
                    || item.Product!.Name.ToLower().Contains(keyword)
                    || item.Product!.Slug.ToLower().Contains(keyword)
                    || item.CoffeeType!.Name.ToLower().Contains(keyword));
            }

            if (parameters.ProductId.HasValue)
            {
                query = query.Where(item => item.ProductId == parameters.ProductId.Value);
            }

            if (parameters.CoffeeTypeId.HasValue)
            {
                query = query.Where(item => item.CoffeeTypeId == parameters.CoffeeTypeId.Value);
            }

            if (parameters.IsPublished.HasValue)
            {
                query = query.Where(item => item.IsPublished == parameters.IsPublished.Value);
            }

            var total = await query.CountAsync();
            var stories = await ProjectAdmin(query
                    .OrderByDescending(item => item.UpdatedAt ?? item.CreatedAt)
                    .ThenByDescending(item => item.Id)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize))
                .ToListAsync();

            foreach (var story in stories)
            {
                if (string.IsNullOrWhiteSpace(story.LandingPageUrl))
                {
                    story.LandingPageUrl = BuildCanonicalUrl(story.Slug);
                }

                SanitizeAdminContent(story);
            }

            return new ProductStoryAdminPageDto
            {
                Page = page,
                PageSize = pageSize,
                TotalStories = total,
                TotalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)pageSize),
                Stories = stories
            };
        }

        public async Task<ProductStoryAdminDto?> GetAdminByIdAsync(Guid id)
        {
            var story = await ProjectAdmin(_context.ProductStories
                    .AsNoTracking()
                    .Where(item => item.Id == id))
                .SingleOrDefaultAsync();

            if (story != null && string.IsNullOrWhiteSpace(story.LandingPageUrl))
            {
                story.LandingPageUrl = BuildCanonicalUrl(story.Slug);
            }

            if (story != null)
            {
                SanitizeAdminContent(story);
            }

            return story;
        }

        public async Task<ProductStoryAdminDto> CreateAsync(CreateProductStoryRequest request)
        {
            var contentVi = _sanitizer.SanitizeAndValidate(request.ContentHtmlVi, "ContentHtmlVi");
            var contentEn = _sanitizer.SanitizeAndValidate(request.ContentHtmlEn, "ContentHtmlEn");

            await using var transaction = await _context.Database.BeginTransactionAsync();
            var product = await _context.Products
                .SingleOrDefaultAsync(item => item.Id == request.ProductId && item.IsActive);
            if (product == null)
            {
                throw new NotFoundException("Active product not found.");
            }

            var coffeeType = await _context.CoffeeTypes
                .SingleOrDefaultAsync(item => item.Id == request.CoffeeTypeId && item.IsActive);
            if (coffeeType == null)
            {
                throw new NotFoundException("Active coffee type not found.");
            }

            var slug = CreateStorySlug(coffeeType.Slug, product.Slug);
            var canonicalUrl = BuildCanonicalUrl(slug);
            if (await _context.ProductStories.AnyAsync(item => item.ProductId == product.Id && item.CoffeeTypeId == coffeeType.Id))
            {
                throw new ConflictException("A traceability page already exists for this product and coffee type.");
            }

            if (await _context.ProductStories.AnyAsync(item => item.Slug == slug))
            {
                throw new ConflictException("The generated traceability URL is already in use.");
            }

            var story = new ProductStory
            {
                ProductId = product.Id,
                CoffeeTypeId = coffeeType.Id,
                Slug = slug,
                ContentHtmlVi = contentVi,
                ContentHtmlEn = contentEn,
                IsPublished = true,
                CreatedAt = DateTime.UtcNow
            };

            story.QRCodes.Add(new QRCode
            {
                ProductId = product.Id,
                QRValue = canonicalUrl,
                LandingPageUrl = canonicalUrl,
                IsShared = true,
                ScanLimit = null,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });

            _context.ProductStories.Add(story);
            try
            {
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (DbUpdateException)
            {
                throw new ConflictException("The product and coffee type combination is already in use.");
            }

            return (await GetAdminByIdAsync(story.Id))!;
        }

        public async Task<ProductStoryAdminDto> UpdateAsync(Guid id, UpdateProductStoryRequest request)
        {
            var story = await _context.ProductStories.SingleOrDefaultAsync(item => item.Id == id);
            if (story == null)
            {
                throw new NotFoundException("Traceability page not found.");
            }

            story.ContentHtmlVi = _sanitizer.SanitizeAndValidate(request.ContentHtmlVi, "ContentHtmlVi");
            story.ContentHtmlEn = _sanitizer.SanitizeAndValidate(request.ContentHtmlEn, "ContentHtmlEn");
            story.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                throw new ConflictException("The traceability page could not be updated.");
            }

            return (await GetAdminByIdAsync(id))!;
        }

        public async Task<ProductStoryAdminDto> SetPublicationAsync(Guid id, bool isPublished)
        {
            var story = await _context.ProductStories.SingleOrDefaultAsync(item => item.Id == id);
            if (story == null)
            {
                throw new NotFoundException("Traceability page not found.");
            }

            if (isPublished)
            {
                story.ContentHtmlVi = _sanitizer.SanitizeAndValidate(story.ContentHtmlVi, "ContentHtmlVi");
                story.ContentHtmlEn = _sanitizer.SanitizeAndValidate(story.ContentHtmlEn, "ContentHtmlEn");
            }

            story.IsPublished = isPublished;
            story.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return (await GetAdminByIdAsync(id))!;
        }

        public async Task<IReadOnlyList<CoffeeTypeDto>> GetActiveCoffeeTypesAsync()
        {
            return await _context.CoffeeTypes
                .AsNoTracking()
                .Where(item => item.IsActive)
                .OrderBy(item => item.DisplayOrder)
                .ThenBy(item => item.Name)
                .Select(item => new CoffeeTypeDto
                {
                    Id = item.Id,
                    Name = item.Name,
                    Slug = item.Slug,
                    IsActive = item.IsActive,
                    DisplayOrder = item.DisplayOrder
                })
                .ToListAsync();
        }

        private IQueryable<ProductStoryAdminDto> ProjectAdmin(IQueryable<ProductStory> query)
        {
            return query.Select(item => new ProductStoryAdminDto
            {
                Id = item.Id,
                ProductId = item.ProductId,
                CoffeeTypeId = item.CoffeeTypeId,
                Slug = item.Slug,
                ProductName = item.Product!.Name,
                ProductSlug = item.Product.Slug,
                CoffeeTypeName = item.CoffeeType!.Name,
                CoffeeTypeSlug = item.CoffeeType.Slug,
                ContentHtmlVi = item.ContentHtmlVi,
                ContentHtmlEn = item.ContentHtmlEn,
                IsPublished = item.IsPublished,
                CreatedAt = item.CreatedAt,
                UpdatedAt = item.UpdatedAt,
                LandingPageUrl = item.QRCodes
                    .Where(qr => qr.IsShared && qr.IsActive)
                    .Select(qr => qr.LandingPageUrl)
                    .FirstOrDefault() ?? string.Empty,
                SharedQrCount = item.QRCodes.Count(qr => qr.IsShared && qr.IsActive)
            });
        }

        private string CreateStorySlug(string coffeeSlug, string productSlug)
        {
            var slug = $"{NormalizeSlugSegment(coffeeSlug)}-and-{NormalizeSlugSegment(productSlug)}";
            if (slug.Length > MaxStorySlugLength)
            {
                throw new BadRequestException("The generated traceability slug must be 200 characters or fewer.");
            }

            return slug;
        }

        private string BuildCanonicalUrl(string slug)
        {
            var baseUrl = (_settings.PublicBaseUrl ?? string.Empty).Trim().TrimEnd('/');
            if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var uri)
                || (uri.Scheme != Uri.UriSchemeHttps && !_environment.IsDevelopment()))
            {
                throw new BadRequestException("Traceability:PublicBaseUrl must be an absolute HTTPS URL outside Development.");
            }

            var canonicalUrl = $"{baseUrl}/{slug}";
            if (canonicalUrl.Length > MaxQrValueLength)
            {
                throw new BadRequestException("The generated traceability URL must be 250 characters or fewer.");
            }

            return canonicalUrl;
        }

        private static string NormalizeSlugSegment(string value)
        {
            var normalized = value.Trim().ToLowerInvariant();
            while (normalized.Contains("--", StringComparison.Ordinal))
            {
                normalized = normalized.Replace("--", "-", StringComparison.Ordinal);
            }

            return normalized.Trim('-');
        }

        private void SanitizeAdminContent(ProductStoryAdminDto story)
        {
            story.ContentHtmlVi = _sanitizer.SanitizeAndValidate(story.ContentHtmlVi, "ContentHtmlVi");
            story.ContentHtmlEn = _sanitizer.SanitizeAndValidate(story.ContentHtmlEn, "ContentHtmlEn");
        }
    }
}
