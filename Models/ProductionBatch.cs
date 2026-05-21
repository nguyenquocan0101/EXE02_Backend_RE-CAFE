using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EXE02_Backend_RE_CAFE.Models
{
    public class ProductionBatch
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid CoffeeGroundBatchId { get; set; }

        [Required]
        [StringLength(50)]
        public string BatchCode { get; set; } = string.Empty;

        [Required]
        public DateTime ProductionDate { get; set; }

        [Required]
        public int QuantityProduced { get; set; }

        [Required]
        [StringLength(100)]
        public string QualityStatus { get; set; } = string.Empty;

        // Navigation properties
        public CoffeeGroundBatch? CoffeeGroundBatch { get; set; }
        public ICollection<ProductStory> ProductStories { get; set; } = new List<ProductStory>();
    }
}
