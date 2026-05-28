using System;
using System.ComponentModel.DataAnnotations;

namespace EXE02_Backend_RE_CAFE.Models
{
    public class ProductCustomization
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public Guid ProductId { get; set; }

        [Required]
        [StringLength(500)]
        public string SourceImageUrl { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string SourceImagePublicId { get; set; } = string.Empty;

        [StringLength(500)]
        public string? PreviewImageUrl { get; set; }

        [StringLength(500)]
        public string? ResultModelUrl { get; set; }

        [StringLength(255)]
        public string? ResultModelPublicId { get; set; }

        [Required]
        public ProductCustomizationStatus Status { get; set; } = ProductCustomizationStatus.Queued;

        public bool IsMockResult { get; set; } = true;

        public decimal PositionX { get; set; } = 0;
        public decimal PositionY { get; set; } = 0;
        public decimal PositionZ { get; set; } = 0;

        public decimal RotationX { get; set; } = 0;
        public decimal RotationY { get; set; } = 0;
        public decimal RotationZ { get; set; } = 0;

        public decimal Scale { get; set; } = 1;
        public decimal EngraveDepth { get; set; } = 1;

        [StringLength(1000)]
        public string? Note { get; set; }

        [StringLength(1000)]
        public string? FailureReason { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        public User? User { get; set; }
        public Product? Product { get; set; }
    }
}
