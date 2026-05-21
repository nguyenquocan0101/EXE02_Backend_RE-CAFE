using System;
using System.ComponentModel.DataAnnotations;

namespace EXE02_Backend_RE_CAFE.Models
{
    public class RewardRedemption
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public Guid RewardId { get; set; }

        [Required]
        public int PointsUsed { get; set; }

        [Required]
        [StringLength(50)]
        public string RedemptionCode { get; set; } = string.Empty;

        [Required]
        public RedemptionStatus Status { get; set; } = RedemptionStatus.Pending;

        public DateTime RedeemedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public User? User { get; set; }
        public Reward? Reward { get; set; }
    }
}
