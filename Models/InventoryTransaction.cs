using System;
using System.ComponentModel.DataAnnotations;

namespace EXE02_Backend_RE_CAFE.Models
{
    public class InventoryTransaction
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid ProductId { get; set; }

        public Guid? VariantId { get; set; }

        [Required]
        public InventoryType Type { get; set; }

        [Required]
        public int Quantity { get; set; }

        [StringLength(500)]
        public string? Note { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Product? Product { get; set; }
        public ProductVariant? Variant { get; set; }
    }
}
