using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EXE02_Backend_RE_CAFE.Models
{
    public class CoffeeType
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(80)]
        public string Slug { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public int DisplayOrder { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public ICollection<ProductStory> ProductStories { get; set; } = new List<ProductStory>();
    }
}
