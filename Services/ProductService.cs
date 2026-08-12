using System;
using System.Collections.Generic;
using System.IO;
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
        private const int FeaturedProductsLimit = 8;
        private const int MaxProductImages = 5;
        private const long MaxModel3DFileSizeBytes = 25 * 1024 * 1024; // 25MB
        private static readonly HashSet<string> AllowedModel3DExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".glb",
            ".gltf",
            ".stl",
            ".obj",
            ".3mf"
        };

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
                    Model3DUrl = p.Model3DUrl,
                    CategoryName = p.Category != null ? p.Category.Name : string.Empty,
                    ViewCount = p.ViewCount
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<ProductListDto>> GetFeaturedProductsAsync()
        {
            var ranking = await _context.OrderItems
                .Where(oi => oi.Order != null
                    && oi.Order.Status == OrderStatus.Completed
                    && oi.Product != null
                    && oi.Product.IsActive)
                .GroupBy(oi => new
                {
                    oi.ProductId,
                    ProductCreatedAt = oi.Product!.CreatedAt
                })
                .Select(g => new
                {
                    g.Key.ProductId,
                    g.Key.ProductCreatedAt,
                    SoldQuantity = g.Sum(x => x.Quantity),
                    Revenue = g.Sum(x => x.TotalPrice)
                })
                .OrderByDescending(x => x.SoldQuantity)
                .ThenByDescending(x => x.Revenue)
                .ThenByDescending(x => x.ProductCreatedAt)
                .Take(FeaturedProductsLimit)
                .ToListAsync();

            if (!ranking.Any())
            {
                return Enumerable.Empty<ProductListDto>();
            }

            var rankedProductIds = ranking.Select(x => x.ProductId).ToList();

            var products = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .Where(p => p.IsActive && rankedProductIds.Contains(p.Id))
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
                    Model3DUrl = p.Model3DUrl,
                    CategoryName = p.Category != null ? p.Category.Name : string.Empty,
                    ViewCount = p.ViewCount
                })
                .ToListAsync();

            var rankingOrder = rankedProductIds
                .Select((productId, index) => new { productId, index })
                .ToDictionary(x => x.productId, x => x.index);

            return products
                .OrderBy(p => rankingOrder[p.Id])
                .ToList();
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
                    Model3DUrl = p.Model3DUrl,
                    CategoryName = p.Category != null ? p.Category.Name : string.Empty,
                    ViewCount = p.ViewCount
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

        public async Task<ProductDetailDto?> GetProductByIdForAdminAsync(Guid id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .Include(p => p.ProductVariants)
                .FirstOrDefaultAsync(p => p.Id == id);

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

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

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

            await _context.SaveChangesAsync();

            var updatedProduct = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .Include(p => p.ProductVariants)
                .FirstOrDefaultAsync(p => p.Id == id);

            return MapToDetailDto(updatedProduct!);
        }

        public async Task<ProductDetailDto?> UploadProductImagesAsync(Guid id, UploadProductImagesRequest request)
        {
            var product = await _context.Products
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (product == null)
            {
                throw new NotFoundException($"Product with ID {id} not found.");
            }

            var newImageCount = request.ImageUrls?.Count ?? 0;
            if (newImageCount > MaxProductImages)
            {
                throw new BadRequestException($"A product can have at most {MaxProductImages} images.");
            }

            var totalImagesAfterUpdate = request.ReplaceImages
                ? newImageCount
                : product.ProductImages.Count + newImageCount;
            if (totalImagesAfterUpdate > MaxProductImages)
            {
                throw new BadRequestException($"A product can have at most {MaxProductImages} images after update.");
            }

            var uploadedImageUrls = await UploadImagesAsync(request.ImageUrls, "recafe/products");

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

            product.UpdatedAt = DateTime.UtcNow;
            _context.Products.Update(product);
            await _context.SaveChangesAsync();

            var updatedProduct = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .Include(p => p.ProductVariants)
                .FirstOrDefaultAsync(p => p.Id == id);

            return MapToDetailDto(updatedProduct!);
        }

        public async Task<ProductDetailDto?> UploadProductModel3DAsync(Guid id, UploadProductModel3DRequest request)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .Include(p => p.ProductVariants)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (product == null)
            {
                throw new NotFoundException($"Product with ID {id} not found.");
            }

            if (!product.IsPersonalizable)
            {
                throw new BadRequestException("This product is not personalizable. Enable IsPersonalizable before uploading a 3D model.");
            }

            ValidateModel3DFile(request.File);

            var (url, publicId) = await _cloudinaryService.UploadRawFileAsync(request.File, "recafe/products-3d");
            product.Model3DUrl = url;
            product.Model3DPublicId = publicId;
            product.UpdatedAt = DateTime.UtcNow;

            _context.Products.Update(product);
            await _context.SaveChangesAsync();

            return MapToDetailDto(product);
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
                Model3DUrl = product.Model3DUrl,
                IsPersonalizable = product.IsPersonalizable,
                RewardPoints = product.RewardPoints,
                ViewCount = product.ViewCount,
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

        public async Task<ProductViewCountDto?> IncrementProductViewCountAsync(Guid id)
        {
            var updatedRows = await _context.Products
                .Where(product => product.Id == id && product.IsActive)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(product => product.ViewCount, product => product.ViewCount + 1));

            if (updatedRows == 0)
            {
                return null;
            }

            var viewCount = await _context.Products
                .Where(product => product.Id == id)
                .Select(product => product.ViewCount)
                .SingleAsync();

            return new ProductViewCountDto { ViewCount = viewCount };
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

        private static void ValidateModel3DFile(Microsoft.AspNetCore.Http.IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new BadRequestException("3D model file is required.");
            }

            if (file.Length > MaxModel3DFileSizeBytes)
            {
                throw new BadRequestException($"3D model file is too large. Maximum allowed size is {MaxModel3DFileSizeBytes / (1024 * 1024)}MB.");
            }

            var extension = Path.GetExtension(file.FileName);
            if (string.IsNullOrWhiteSpace(extension) || !AllowedModel3DExtensions.Contains(extension))
            {
                throw new BadRequestException("Invalid 3D model format. Only .glb, .gltf, and .stl files are supported.");
            }
        }

        private async Task<List<string>> UploadImagesAsync(IReadOnlyCollection<Microsoft.AspNetCore.Http.IFormFile>? imageFiles, string folder)
        {
            var imageUrls = new List<string>();
            if (imageFiles == null || imageFiles.Count == 0)
            {
                return imageUrls;
            }

            foreach (var imageFile in imageFiles)
            {
                var (url, _) = await _cloudinaryService.UploadImageAsync(imageFile, folder);
                imageUrls.Add(url);
            }

            return imageUrls;
        }
    }
}
