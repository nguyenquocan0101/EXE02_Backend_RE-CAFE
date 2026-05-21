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
        public Guid ProductionBatchId { get; set; }

        [Required]
        [StringLength(2000)]
        public string OriginStory { get; set; } = string.Empty;

        [Required]
        [StringLength(2000)]
        public string RecyclingProcess { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string SustainabilityMessage { get; set; } = string.Empty;

        [Required]
        public decimal EstimatedWasteReducedGram { get; set; }

        // Navigation properties
        public Product? Product { get; set; }
        public ProductionBatch? ProductionBatch { get; set; }
        public ICollection<QRCode> QRCodes { get; set; } = new List<QRCode>();
    }
}
