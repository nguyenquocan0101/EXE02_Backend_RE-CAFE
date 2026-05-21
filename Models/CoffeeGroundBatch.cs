using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EXE02_Backend_RE_CAFE.Models
{
    public class CoffeeGroundBatch
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid PartnerId { get; set; }

        [Required]
        public decimal WeightKg { get; set; }

        [Required]
        public DateTime CollectedDate { get; set; }

        [Required]
        [StringLength(100)]
        public string ProcessingStatus { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Note { get; set; }

        // Navigation properties
        public CoffeePartner? Partner { get; set; }
        public ICollection<ProductionBatch> ProductionBatches { get; set; } = new List<ProductionBatch>();
    }
}
