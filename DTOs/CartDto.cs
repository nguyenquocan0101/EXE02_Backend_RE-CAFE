using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EXE02_Backend_RE_CAFE.DTOs
{
    public class CartDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<CartItemDto> CartItems { get; set; } = new List<CartItemDto>();
        public decimal TotalAmount { get; set; }
    }

    public class CartItemDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ProductThumbnail { get; set; }
        public decimal UnitPrice { get; set; }
        
        public Guid? VariantId { get; set; }
        public string? VariantName { get; set; }
        
        public int Quantity { get; set; }
        public string? PersonalizationNote { get; set; }
        public decimal TotalPrice { get; set; }
    }

    public class AddCartItemRequest
    {
        [Required]
        public Guid ProductId { get; set; }

        public Guid? VariantId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; } = 1;

        [StringLength(500)]
        public string? PersonalizationNote { get; set; }
    }

    public class UpdateCartItemRequest
    {
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }

        [StringLength(500)]
        public string? PersonalizationNote { get; set; }
    }
}
