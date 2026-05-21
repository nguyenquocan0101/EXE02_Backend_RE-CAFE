using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EXE02_Backend_RE_CAFE.Models
{
    public class Reward
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        public int RequiredPoints { get; set; }

        [Required]
        public RewardType Type { get; set; }

        public decimal? DiscountValue { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation property
        public ICollection<RewardRedemption> RewardRedemptions { get; set; } = new List<RewardRedemption>();
    }
}
