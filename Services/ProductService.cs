using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EXE02_Backend_RE_CAFE.Data;
using EXE02_Backend_RE_CAFE.DTOs;
using EXE02_Backend_RE_CAFE.Interfaces;
using EXE02_Backend_RE_CAFE.Exceptions;
using EXE02_Backend_RE_CAFE.Models;

namespace EXE02_Backend_RE_CAFE.Services
{
    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICloudinaryService _cloudinaryService;

        public ProductService(ApplicationDbContext context, ICloudinaryService cloudinaryService)
        {
            _context = context;
            _cloudinaryService = cloudinaryService;
        }

        public async Task<IEnumerable<ProductListDto>> GetActiveProductsAsync()
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .Where(p => p.IsActive)
                .Select(p => new ProductListDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Slug = p.Slug,
                    SKU = p.SKU,
                    IsActive = p.IsActive,
                    Price = p.Price,
                    SalePrice = p.SalePrice,
                    ShortDescription = p.ShortDescription,
                    Size = p.Size,
                    Material = p.Material,
                    ThumbnailUrl = p.ProductImages
                        .Where(img => img.IsThumbnail)
                        .OrderBy(img => img.SortOrder)
                        .Select(img => img.ImageUrl)
                        .FirstOrDefault() ?? p.ProductImages
                        .OrderBy(img => img.SortOrder)
                        .Select(img => img.ImageUrl)
                        .FirstOrDefault(),
                    CategoryName = p.Category != null ? p.Category.Name : string.Empty
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<ProductListDto>> GetProductsForAdminAsync(bool? isActive = null)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .AsQueryable();

            if (isActive.HasValue)
            {
                query = query.Where(p => p.IsActive == isActive.Value);
            }

            return await query
                .Select(p => new ProductListDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Slug = p.Slug,
                    SKU = p.SKU,
                    IsActive = p.IsActive,
                    Price = p.Price,
                    SalePrice = p.SalePrice,
                    ShortDescription = p.ShortDescription,
                    Size = p.Size,
                    Material = p.Material,
                    ThumbnailUrl = p.ProductImages
                        .Where(img => img.IsThumbnail)
                        .OrderBy(img => img.SortOrder)
                        .Select(img => img.ImageUrl)
                        .FirstOrDefault() ?? p.ProductImages
                        .OrderBy(img => img.SortOrder)
                        .Select(img => img.ImageUrl)
                        .FirstOrDefault(),
                    CategoryName = p.Category != null ? p.Category.Name : string.Empty
                })
                .ToListAsync();
        }

        public async Task<ProductDetailDto?> GetProductByIdAsync(Guid id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .Include(p => p.ProductVariants)
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

            if (product == null)
            {
                return null;
            }

            return MapToDetailDto(product);
        }

        public async Task<ProductDetailDto?> CreateProductAsync(CreateProductRequest request)
        {
            var categoryExists = await _context.Categories.AnyAsync(c => c.Id == request.CategoryId);
            if (!categoryExists)
            {
                throw new NotFoundException($"Category with ID {request.CategoryId} not found.");
            }

            if (await _context.Products.AnyAsync(p => p.SKU == request.SKU))
            {
                throw new BadRequestException($"Product with SKU '{request.SKU}' already exists.");
            }

            if (await _context.Products.AnyAsync(p => p.Slug == request.Slug))
            {
                throw new BadRequestException($"Product with Slug '{request.Slug}' already exists.");
            }

            var imageFolder = string.IsNullOrWhiteSpace(request.ImageFolder) ? "recafe/products" : request.ImageFolder.Trim();
            var uploadedImageUrls = await UploadImagesAsync(request.Images, imageFolder);

            var product = new Product
            {
                CategoryId = request.CategoryId,
                Name = request.Name,
                Slug = request.Slug,
                SKU = request.SKU,
                Price = request.Price,
                SalePrice = request.SalePrice,
                ShortDescription = request.ShortDescription,
                Description = request.Description,
                Material = request.Material,
                Size = request.Size,
                UsageNote = request.UsageNote,
                IsPersonalizable = request.IsPersonalizable,
                IsActive = request.IsActive,
                RewardPoints = request.RewardPoints,
                CreatedAt = DateTime.UtcNow
            };

            if (uploadedImageUrls.Count > 0)
            {
                for (var i = 0; i < uploadedImageUrls.Count; i++)
                {
                    product.ProductImages.Add(new ProductImage
                    {
                        ImageUrl = uploadedImageUrls[i],
                        IsThumbnail = i == 0,
                        SortOrder = i + 1
                    });
                }
            }

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // Load properties for returning DTO
            var createdProduct = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .Include(p => p.ProductVariants)
                .FirstOrDefaultAsync(p => p.Id == product.Id);

            return MapToDetailDto(createdProduct!);
        }

        public async Task<ProductDetailDto?> UpdateProductAsync(Guid id, UpdateProductRequest request)
        {
            var product = await _context.Products
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (product == null)
            {
                throw new NotFoundException($"Product with ID {id} not found.");
            }

            var categoryExists = await _context.Categories.AnyAsync(c => c.Id == request.CategoryId);
            if (!categoryExists)
            {
                throw new NotFoundException($"Category with ID {request.CategoryId} not found.");
            }

            if (await _context.Products.AnyAsync(p => p.SKU == request.SKU && p.Id != id))
            {
                throw new BadRequestException($"Product with SKU '{request.SKU}' already exists on another product.");
            }

            if (await _context.Products.AnyAsync(p => p.Slug == request.Slug && p.Id != id))
            {
                throw new BadRequestException($"Product with Slug '{request.Slug}' already exists on another product.");
            }

            var imageFolder = string.IsNullOrWhiteSpace(request.ImageFolder) ? "recafe/products" : request.ImageFolder.Trim();
            var uploadedImageUrls = await UploadImagesAsync(request.Images, imageFolder);

            product.CategoryId = request.CategoryId;
            product.Name = request.Name;
            product.Slug = request.Slug;
            product.SKU = request.SKU;
            product.Price = request.Price;
            product.SalePrice = request.SalePrice;
            product.ShortDescription = request.ShortDescription;
            product.Description = request.Description;
            product.Material = request.Material;
            product.Size = request.Size;
            product.UsageNote = request.UsageNote;
            product.IsPersonalizable = request.IsPersonalizable;
            product.IsActive = request.IsActive;
            product.RewardPoints = request.RewardPoints;
            product.UpdatedAt = DateTime.UtcNow;

            var hasThumbnail = product.ProductImages.Any(i => i.IsThumbnail);
            var nextSortOrder = product.ProductImages.Count == 0
                ? 1
                : product.ProductImages.Max(i => i.SortOrder) + 1;

            if (request.ReplaceImages)
            {
                var existingImages = product.ProductImages.ToList();
                _context.ProductImages.RemoveRange(existingImages);
                hasThumbnail = false;
                nextSortOrder = 1;
            }

            if (uploadedImageUrls.Count > 0)
            {
                var newImages = uploadedImageUrls
                    .Select((url, index) => new ProductImage
                    {
                        ProductId = product.Id,
                        ImageUrl = url,
                        SortOrder = nextSortOrder + index,
                        IsThumbnail = !hasThumbnail && index == 0
                    })
                    .ToList();

                await _context.ProductImages.AddRangeAsync(newImages);
            }

            await _context.SaveChangesAsync();

            var updatedProduct = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .Include(p => p.ProductVariants)
                .FirstOrDefaultAsync(p => p.Id == id);

            return MapToDetailDto(updatedProduct!);
        }

        private ProductDetailDto MapToDetailDto(Product product)
        {
            return new ProductDetailDto
            {
                Id = product.Id,
                CategoryId = product.CategoryId,
                Name = product.Name,
                Slug = product.Slug,
                SKU = product.SKU,
                Price = product.Price,
                SalePrice = product.SalePrice,
                ShortDescription = product.ShortDescription,
                Description = product.Description,
                Material = product.Material,
                Size = product.Size,
                UsageNote = product.UsageNote,
                IsPersonalizable = product.IsPersonalizable,
                RewardPoints = product.RewardPoints,
                Category = product.Category != null ? new CategoryDto
                {
                    Id = product.Category.Id,
                    Name = product.Category.Name,
                    Slug = product.Category.Slug,
                    Description = product.Category.Description
                } : null,
                Images = product.ProductImages
                    .OrderBy(img => img.SortOrder)
                    .Select(img => new ProductImageDto
                    {
                        Id = img.Id,
                        ImageUrl = img.ImageUrl,
                        IsThumbnail = img.IsThumbnail,
                        SortOrder = img.SortOrder
                    }).ToList(),
                Variants = product.ProductVariants
                    .Where(v => v.IsActive)
                    .Select(v => new ProductVariantDto
                    {
                        Id = v.Id,
                        VariantName = v.VariantName,
                        Color = v.Color,
                        Size = v.Size,
                        Price = v.Price,
                        StockQuantity = v.StockQuantity,
                        SKU = v.SKU
                    }).ToList()
            };
        }

        public async Task<ProductDetailDto?> GetProductBySlugAsync(string slug)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .Include(p => p.ProductVariants)
                .FirstOrDefaultAsync(p => p.Slug == slug && p.IsActive);

            if (product == null)
            {
                return null;
            }

            return MapToDetailDto(product);
        }

        public async Task<bool> SoftDeleteProductAsync(Guid id)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (product == null)
            {
                throw new NotFoundException($"Product with ID {id} not found.");
            }

            product.IsActive = false;
            product.UpdatedAt = DateTime.UtcNow;

            _context.Products.Update(product);
            await _context.SaveChangesAsync();
            return true;
        }

        private async Task<List<string>> UploadImagesAsync(IReadOnlyCollection<Microsoft.AspNetCore.Http.IFormFile>? images, string folder)
        {
            var imageUrls = new List<string>();
            if (images == null || images.Count == 0)
            {
                return imageUrls;
            }

            foreach (var image in images)
            {
                var (url, _) = await _cloudinaryService.UploadImageAsync(image, folder);
                imageUrls.Add(url);
            }

            return imageUrls;
        }
    }
}
