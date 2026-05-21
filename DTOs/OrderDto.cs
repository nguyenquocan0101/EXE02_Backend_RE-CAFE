using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EXE02_Backend_RE_CAFE.DTOs
{
    public class OrderDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public Guid ShippingAddressId { get; set; }
        public OrderAddressDto? ShippingAddress { get; set; }
        public decimal Subtotal { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public string? Note { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CouponCode { get; set; }
        public List<OrderItemDto> OrderItems { get; set; } = new List<OrderItemDto>();
    }

    public class OrderItemDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public Guid? VariantId { get; set; }
        public string? VariantName { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
        public string? PersonalizationNote { get; set; }
    }

    public class OrderAddressDto
    {
        public Guid Id { get; set; }
        public string ReceiverName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Province { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string Ward { get; set; } = string.Empty;
        public string DetailAddress { get; set; } = string.Empty;
    }

    public class CreateOrderRequest
    {
        [Required]
        public Guid ShippingAddressId { get; set; }

        [StringLength(500)]
        public string? Note { get; set; }

        [StringLength(50)]
        public string? CouponCode { get; set; }
    }

    public class UpdateOrderStatusRequest
    {
        [Required]
        public string Status { get; set; } = string.Empty;
    }
}
