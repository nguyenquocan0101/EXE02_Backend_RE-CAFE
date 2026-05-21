using System;
using System.ComponentModel.DataAnnotations;

namespace EXE02_Backend_RE_CAFE.Models
{
    public class QRScanLog
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid QRCodeId { get; set; }

        public Guid? UserId { get; set; }

        public DateTime ScannedAt { get; set; } = DateTime.UtcNow;

        [StringLength(50)]
        public string? IpAddress { get; set; }

        [StringLength(250)]
        public string? DeviceInfo { get; set; }

        public int PointsEarned { get; set; } = 0;

        // Navigation properties
        public QRCode? QRCode { get; set; }
        public User? User { get; set; }
    }
}
