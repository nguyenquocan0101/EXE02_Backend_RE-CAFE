using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EXE02_Backend_RE_CAFE.Models
{
    public class ProductStory
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid ProductId { get; set; }

        [Required]
        public Guid CoffeeTypeId { get; set; }

        [Required]
        [StringLength(200)]
        public string Slug { get; set; } = string.Empty;

        [Required]
        [StringLength(50000)]
        public string ContentHtmlVi { get; set; } = string.Empty;

        [Required]
        [StringLength(50000)]
        public string ContentHtmlEn { get; set; } = string.Empty;

        public bool IsPublished { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public Guid? ProductionBatchId { get; set; }

        [StringLength(2000)]
        public string? OriginStory { get; set; }

        [StringLength(2000)]
        public string? RecyclingProcess { get; set; }

        [StringLength(1000)]
        public string? SustainabilityMessage { get; set; }

        public decimal? EstimatedWasteReducedGram { get; set; }

        // Navigation properties
        public Product? Product { get; set; }
        public CoffeeType? CoffeeType { get; set; }
        public ProductionBatch? ProductionBatch { get; set; }
        public ICollection<QRCode> QRCodes { get; set; } = new List<QRCode>();
    }
}
