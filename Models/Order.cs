using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EXE02_Backend_RE_CAFE.Models
{
    public class Order
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid UserId { get; set; }

        [Required]
        [StringLength(50)]
        public string OrderCode { get; set; } = string.Empty;

        [Required]
        public Guid ShippingAddressId { get; set; }

        [Required]
        public decimal Subtotal { get; set; }

        [Required]
        public decimal ShippingFee { get; set; }

        [Required]
        public decimal DiscountAmount { get; set; } = 0.00m;

        [Required]
        public decimal TotalAmount { get; set; }

        [Required]
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        [Required]
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Unpaid;

        [StringLength(500)]
        public string? Note { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Guid? CouponId { get; set; }

        // Navigation properties
        public User? User { get; set; }
        public Address? ShippingAddress { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public Payment? Payment { get; set; }
        public Shipment? Shipment { get; set; }
        public Coupon? Coupon { get; set; }
    }
}
