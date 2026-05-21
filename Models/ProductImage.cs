using System;
using System.ComponentModel.DataAnnotations;

namespace EXE02_Backend_RE_CAFE.Models
{
    public class ProductImage
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid ProductId { get; set; }

        [Required]
        [StringLength(500)]
        public string ImageUrl { get; set; } = string.Empty;

        public bool IsThumbnail { get; set; } = false;

        public int SortOrder { get; set; } = 0;

        // Navigation property
        public Product? Product { get; set; }
    }
}
