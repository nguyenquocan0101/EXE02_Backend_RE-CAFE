using System;
using System.ComponentModel.DataAnnotations;

namespace EXE02_Backend_RE_CAFE.Models
{
    public class B2BRequest
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [StringLength(150)]
        public string CompanyName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string ContactName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Phone { get; set; } = string.Empty;

        [Required]
        [StringLength(2000)]
        public string ProductRequirement { get; set; } = string.Empty;

        [Required]
        public int ExpectedQuantity { get; set; }

        public decimal? ExpectedBudget { get; set; }

        public DateTime? NeededDate { get; set; }

        [Required]
        public B2BRequestStatus Status { get; set; } = B2BRequestStatus.New;

        [StringLength(1000)]
        public string? AdminNote { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
