using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EXE02_Backend_RE_CAFE.Models
{
    public class QRCode
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid ProductId { get; set; }

        public Guid? ProductStoryId { get; set; }

        [Required]
        [StringLength(250)]
        public string QRValue { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string LandingPageUrl { get; set; } = string.Empty;

        public bool IsShared { get; set; }

        public int? ScanLimit { get; set; } = 1;

        public int ScanCount { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ExpiredAt { get; set; }

        // Navigation properties
        public Product? Product { get; set; }
        public ProductStory? ProductStory { get; set; }
        public ICollection<QRScanLog> QRScanLogs { get; set; } = new List<QRScanLog>();
    }
}
