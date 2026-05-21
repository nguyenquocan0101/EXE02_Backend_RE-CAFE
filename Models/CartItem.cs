using System;
using System.ComponentModel.DataAnnotations;

namespace EXE02_Backend_RE_CAFE.Models
{
    public class CartItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid CartId { get; set; }

        [Required]
        public Guid ProductId { get; set; }

        public Guid? VariantId { get; set; }

        [Required]
        public int Quantity { get; set; } = 1;

        [StringLength(500)]
        public string? PersonalizationNote { get; set; } // Ghi chú cá nhân hóa/ thiết kế AI

        // Navigation properties
        public Cart? Cart { get; set; }
        public Product? Product { get; set; }
        public ProductVariant? Variant { get; set; }
    }
}
