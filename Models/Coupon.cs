using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EXE02_Backend_RE_CAFE.Models
{
    public class Coupon
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [StringLength(50)]
        public string Code { get; set; } = string.Empty;

        [Required]
        public CouponType Type { get; set; }

        [Required]
        public decimal Value { get; set; }

        [Required]
        public CouponScope Scope { get; set; } = CouponScope.Order;

        public decimal? MaxDiscountAmount { get; set; }

        public decimal? MinimumOrderAmount { get; set; }

        public int UsageLimit { get; set; }

        public int UsedCount { get; set; } = 0;

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation property
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<CouponProduct> CouponProducts { get; set; } = new List<CouponProduct>();
    }
}
