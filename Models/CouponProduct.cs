using System;

namespace EXE02_Backend_RE_CAFE.Models
{
    public class CouponProduct
    {
        public Guid CouponId { get; set; }
        public Guid ProductId { get; set; }

        public Coupon? Coupon { get; set; }
        public Product? Product { get; set; }
    }
}
