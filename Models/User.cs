using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EXE02_Backend_RE_CAFE.Models
{
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        [Required]
        [StringLength(100)]
        public string Username { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; } = string.Empty;
        
        public string PasswordHash { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [StringLength(20)]
        public string? Phone { get; set; }

        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        [Required]
        public UserRole Role { get; set; } = UserRole.Customer;

        public int TotalPoints { get; set; } = 0;

        [Required]
        public CustomerLevel Level { get; set; } = CustomerLevel.Normal;

        public DateTime? Birthday { get; set; }

        // Navigation properties
        public ICollection<Address> Addresses { get; set; } = new List<Address>();
        public Cart? Cart { get; set; }
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<ProductCustomization> ProductCustomizations { get; set; } = new List<ProductCustomization>();
        public ICollection<LoyaltyPointTransaction> LoyaltyPointTransactions { get; set; } = new List<LoyaltyPointTransaction>();
        public ICollection<RewardRedemption> RewardRedemptions { get; set; } = new List<RewardRedemption>();
    }
}
