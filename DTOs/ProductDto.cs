using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EXE02_Backend_RE_CAFE.DTOs
{

    public class ProductImageDto
    {
        public Guid Id { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsThumbnail { get; set; }
        public int SortOrder { get; set; }
    }

    public class ProductVariantDto
    {
        public Guid Id { get; set; }
        public string VariantName { get; set; } = string.Empty;
        public string? Color { get; set; }
        public string? Size { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string SKU { get; set; } = string.Empty;
    }

    public class ProductListDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal? SalePrice { get; set; }
        public string? ShortDescription { get; set; }
        public string? Size { get; set; }
        public string? Material { get; set; }
        public string? ThumbnailUrl { get; set; }
        public string CategoryName { get; set; } = string.Empty;
    }

    public class ProductDetailDto
    {
        public Guid Id { get; set; }
        public Guid CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal? SalePrice { get; set; }
        public string? ShortDescription { get; set; }
        public string? Description { get; set; }
        public string? Material { get; set; }
        public string? Size { get; set; }
        public string? UsageNote { get; set; }
        public bool IsPersonalizable { get; set; }
        public int RewardPoints { get; set; }
        public CategoryDto? Category { get; set; }
        public List<ProductImageDto> Images { get; set; } = new();
        public List<ProductVariantDto> Variants { get; set; } = new();
    }

    public class CreateProductRequest
    {
        [Required]
        public Guid CategoryId { get; set; }

        [Required]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Slug { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string SKU { get; set; } = string.Empty;

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? SalePrice { get; set; }

        [StringLength(500)]
        public string? ShortDescription { get; set; }

        [StringLength(2000)]
        public string? Description { get; set; }

        [StringLength(200)]
        public string? Material { get; set; }

        [StringLength(100)]
        public string? Size { get; set; }

        [StringLength(500)]
        public string? UsageNote { get; set; }

        public bool IsPersonalizable { get; set; } = false;

        public bool IsActive { get; set; } = true;

        [Range(0, int.MaxValue)]
        public int RewardPoints { get; set; } = 0;
    }

    public class UpdateProductRequest
    {
        [Required]
        public Guid CategoryId { get; set; }

        [Required]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Slug { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string SKU { get; set; } = string.Empty;

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? SalePrice { get; set; }

        [StringLength(500)]
        public string? ShortDescription { get; set; }

        [StringLength(2000)]
        public string? Description { get; set; }

        [StringLength(200)]
        public string? Material { get; set; }

        [StringLength(100)]
        public string? Size { get; set; }

        [StringLength(500)]
        public string? UsageNote { get; set; }

        public bool IsPersonalizable { get; set; }

        public bool IsActive { get; set; }

        [Range(0, int.MaxValue)]
        public int RewardPoints { get; set; }
    }
}
