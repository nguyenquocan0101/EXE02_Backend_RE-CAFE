using System;
using System.ComponentModel.DataAnnotations;

namespace EXE02_Backend_RE_CAFE.Models
{
    public class LoyaltyPointTransaction
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public PointTransactionType Type { get; set; }

        [Required]
        public int Points { get; set; }

        [Required]
        [StringLength(250)]
        public string Reason { get; set; } = string.Empty;

        public Guid? OrderId { get; set; }

        public Guid? QRScanLogId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public User? User { get; set; }
        public Order? Order { get; set; }
        public QRScanLog? QRScanLog { get; set; }
    }
}
