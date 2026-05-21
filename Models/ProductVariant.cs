using System;
using System.ComponentModel.DataAnnotations;

namespace EXE02_Backend_RE_CAFE.Models
{
    public class ProductVariant
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid ProductId { get; set; }

        [Required]
        [StringLength(100)]
        public string VariantName { get; set; } = string.Empty;

        [StringLength(50)]
        public string? Color { get; set; }

        [StringLength(50)]
        public string? Size { get; set; }

        [Required]
        public decimal Price { get; set; }

        public int StockQuantity { get; set; } = 0;

        [Required]
        [StringLength(50)]
        public string SKU { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        // Navigation property
        public Product? Product { get; set; }
    }
}
