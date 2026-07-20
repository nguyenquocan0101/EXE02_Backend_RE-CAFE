using System;
using System.ComponentModel.DataAnnotations;

namespace EXE02_Backend_RE_CAFE.Models
{
    public class ReviewMedia
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid ReviewId { get; set; }

        [Required]
        [StringLength(500)]
        public string Url { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string PublicId { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string MediaType { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Review? Review { get; set; }
    }
}
