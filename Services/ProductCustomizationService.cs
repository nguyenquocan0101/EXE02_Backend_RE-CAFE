using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EXE02_Backend_RE_CAFE.Data;
using EXE02_Backend_RE_CAFE.DTOs;
using EXE02_Backend_RE_CAFE.Exceptions;
using EXE02_Backend_RE_CAFE.Interfaces;
using EXE02_Backend_RE_CAFE.Models;

namespace EXE02_Backend_RE_CAFE.Services
{
    public class ProductCustomizationService : IProductCustomizationService
    {
        private const string CustomizationImageFolder = "recafe/customizations/source-images";

        private readonly ApplicationDbContext _context;
        private readonly ICloudinaryService _cloudinaryService;

        public ProductCustomizationService(ApplicationDbContext context, ICloudinaryService cloudinaryService)
        {
            _context = context;
            _cloudinaryService = cloudinaryService;
        }

        public async Task<ProductCustomizationDto> CreateCustomizationAsync(Guid userId, Guid productId, CreateProductCustomizationRequest request)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == productId && p.IsActive);

            if (product == null)
            {
                throw new NotFoundException($"Product with ID {productId} not found.");
            }

            if (string.IsNullOrWhiteSpace(product.Model3DUrl))
            {
                throw new BadRequestException("This product does not have a 3D model yet.");
            }

            var (sourceImageUrl, sourceImagePublicId) =
                await _cloudinaryService.UploadImageAsync(request.PortraitImage, CustomizationImageFolder);

            var customization = new ProductCustomization
            {
                UserId = userId,
                ProductId = productId,
                SourceImageUrl = sourceImageUrl,
                SourceImagePublicId = sourceImagePublicId,
                PreviewImageUrl = sourceImageUrl,
                ResultModelUrl = null,
                Status = ProductCustomizationStatus.Queued,
                IsMockResult = false,
                PositionX = request.PositionX,
                PositionY = request.PositionY,
                PositionZ = request.PositionZ,
                RotationX = request.RotationX,
                RotationY = request.RotationY,
                RotationZ = request.RotationZ,
                Scale = request.Scale,
                EngraveDepth = request.EngraveDepth,
                Note = request.Note,
                CompletedAt = null,
                UpdatedAt = DateTime.UtcNow
            };

            _context.ProductCustomizations.Add(customization);
            await _context.SaveChangesAsync();

            customization.Product = product;
            return MapToDto(customization);
        }

        public async Task<ProductCustomizationBootstrapDto> GetCustomizationBootstrapAsync(Guid userId, Guid productId)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == productId && p.IsActive);

            if (product == null)
            {
                throw new NotFoundException($"Product with ID {productId} not found.");
            }

            if (string.IsNullOrWhiteSpace(product.Model3DUrl))
            {
                throw new BadRequestException("This product does not have a 3D model yet.");
            }

            var customizations = await _context.ProductCustomizations
                .Include(c => c.Product)
                .Where(c => c.UserId == userId && c.ProductId == productId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return new ProductCustomizationBootstrapDto
            {
                ProductId = product.Id,
                ProductName = product.Name,
                ProductSlug = product.Slug,
                BaseModel3DUrl = product.Model3DUrl!,
                Customizations = customizations.Select(MapToDto).ToList()
            };
        }

        public async Task<IEnumerable<ProductCustomizationDto>> GetMyCustomizationsByProductAsync(Guid userId, Guid productId)
        {
            var customizations = await _context.ProductCustomizations
                .Include(c => c.Product)
                .Where(c => c.UserId == userId && c.ProductId == productId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return customizations.Select(MapToDto);
        }

        public async Task<ProductCustomizationDto?> GetMyCustomizationByIdAsync(Guid userId, Guid productId, Guid customizationId)
        {
            var customization = await _context.ProductCustomizations
                .Include(c => c.Product)
                .FirstOrDefaultAsync(c => c.Id == customizationId && c.UserId == userId && c.ProductId == productId);

            if (customization == null)
            {
                return null;
            }

            return MapToDto(customization);
        }

        private static ProductCustomizationDto MapToDto(ProductCustomization customization)
        {
            return new ProductCustomizationDto
            {
                Id = customization.Id,
                ProductId = customization.ProductId,
                ProductName = customization.Product?.Name ?? string.Empty,
                ProductSlug = customization.Product?.Slug ?? string.Empty,
                BaseModel3DUrl = customization.Product?.Model3DUrl,
                SourceImageUrl = customization.SourceImageUrl,
                PreviewImageUrl = customization.PreviewImageUrl,
                ResultModelUrl = customization.ResultModelUrl,
                Status = customization.Status.ToString(),
                IsMockResult = customization.IsMockResult,
                FailureReason = customization.FailureReason,
                Note = customization.Note,
                PositionX = customization.PositionX,
                PositionY = customization.PositionY,
                PositionZ = customization.PositionZ,
                RotationX = customization.RotationX,
                RotationY = customization.RotationY,
                RotationZ = customization.RotationZ,
                Scale = customization.Scale,
                EngraveDepth = customization.EngraveDepth,
                CreatedAt = customization.CreatedAt,
                UpdatedAt = customization.UpdatedAt,
                CompletedAt = customization.CompletedAt
            };
        }
    }
}
