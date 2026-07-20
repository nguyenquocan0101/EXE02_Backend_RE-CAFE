using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EXE02_Backend_RE_CAFE.Models
{
    public class Review
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public Guid ProductId { get; set; }

        [Required]
        public Guid OrderId { get; set; }

        [Required]
        public int Rating { get; set; } // 1 to 5 stars

        [StringLength(1000)]
        public string? Comment { get; set; }

        public bool IsVisible { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<ReviewMedia> Media { get; set; } = new List<ReviewMedia>();

        // Navigation properties
        public User? User { get; set; }
        public Product? Product { get; set; }
        public Order? Order { get; set; }
    }
}
