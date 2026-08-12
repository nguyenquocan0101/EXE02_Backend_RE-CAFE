using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EXE02_Backend_RE_CAFE.Models
{
    public class Product
    {
        public Guid Id { get; set; } = Guid.NewGuid();

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
        public decimal Price { get; set; }

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

        [StringLength(500)]
        public string? Model3DUrl { get; set; }

        [StringLength(255)]
        public string? Model3DPublicId { get; set; }

        public bool IsPersonalizable { get; set; } = false;

        public bool IsActive { get; set; } = true;

        public int RewardPoints { get; set; } = 0;

        public int ViewCount { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public Category? Category { get; set; }
        public ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();
        public ICollection<ProductVariant> ProductVariants { get; set; } = new List<ProductVariant>();
        public ICollection<InventoryTransaction> InventoryTransactions { get; set; } = new List<InventoryTransaction>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<QRCode> QRCodes { get; set; } = new List<QRCode>();
        public ICollection<ProductCustomization> ProductCustomizations { get; set; } = new List<ProductCustomization>();
        public ICollection<CouponProduct> CouponProducts { get; set; } = new List<CouponProduct>();
        public ICollection<ProductStory> ProductStories { get; set; } = new List<ProductStory>();
    }
}
