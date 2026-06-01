using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace EXE02_Backend_RE_CAFE.DTOs
{
    public class CreateProductCustomizationRequest
    {
        [Required]
        public IFormFile PortraitImage { get; set; } = default!;

        [Range(-10, 10)]
        public decimal PositionX { get; set; } = 0;

        [Range(-10, 10)]
        public decimal PositionY { get; set; } = 0;

        [Range(-10, 10)]
        public decimal PositionZ { get; set; } = 0;

        [Range(-360, 360)]
        public decimal RotationX { get; set; } = 0;

        [Range(-360, 360)]
        public decimal RotationY { get; set; } = 0;

        [Range(-360, 360)]
        public decimal RotationZ { get; set; } = 0;

        [Range(0.1, 10)]
        public decimal Scale { get; set; } = 1;

        [Range(0.1, 5)]
        public decimal EngraveDepth { get; set; } = 1;

        [StringLength(1000)]
        public string? Note { get; set; }
    }

    public class ProductCustomizationDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductSlug { get; set; } = string.Empty;
        public string? BaseModel3DUrl { get; set; }

        public string SourceImageUrl { get; set; } = string.Empty;
        public string? PreviewImageUrl { get; set; }
        public string? ResultModelUrl { get; set; }

        public string Status { get; set; } = string.Empty;
        public bool IsMockResult { get; set; }
        public string? FailureReason { get; set; }
        public string? Note { get; set; }

        public decimal PositionX { get; set; }
        public decimal PositionY { get; set; }
        public decimal PositionZ { get; set; }
        public decimal RotationX { get; set; }
        public decimal RotationY { get; set; }
        public decimal RotationZ { get; set; }
        public decimal Scale { get; set; }
        public decimal EngraveDepth { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }

    public class ProductCustomizationBootstrapDto
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductSlug { get; set; } = string.Empty;
        public string BaseModel3DUrl { get; set; } = string.Empty;
        public List<ProductCustomizationDto> Customizations { get; set; } = new();
    }
}
