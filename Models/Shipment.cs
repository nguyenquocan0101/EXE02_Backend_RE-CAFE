using System;
using System.ComponentModel.DataAnnotations;

namespace EXE02_Backend_RE_CAFE.Models
{
    public class Shipment
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid OrderId { get; set; }

        [Required]
        [StringLength(100)]
        public string CarrierName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string TrackingCode { get; set; } = string.Empty;

        [Required]
        public ShippingStatus Status { get; set; } = ShippingStatus.Pending;

        public DateTime? ShippedAt { get; set; }

        public DateTime? DeliveredAt { get; set; }

        // Navigation property
        public Order? Order { get; set; }
    }
}
