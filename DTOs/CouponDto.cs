using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using EXE02_Backend_RE_CAFE.Models;

namespace EXE02_Backend_RE_CAFE.DTOs
{
    public class PreviewCouponRequest
    {
        [Required]
        [StringLength(50)]
        public string CouponCode { get; set; } = string.Empty;

        public List<Guid>? CartItemIds { get; set; }
    }

    public class CouponPreviewDto
    {
        public string CouponCode { get; set; } = string.Empty;
        public string Scope { get; set; } = string.Empty;
        public string DiscountType { get; set; } = string.Empty;
        public decimal DiscountValue { get; set; }
        public decimal? MaxDiscountAmount { get; set; }
        public decimal EligibleSubtotal { get; set; }
        public decimal CartSubtotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal TotalAfterDiscount { get; set; }
        public List<Guid> ApplicableCartItemIds { get; set; } = new List<Guid>();
        public List<Guid> InapplicableCartItemIds { get; set; } = new List<Guid>();
    }

    public class CouponLineItemInput
    {
        public Guid ProductId { get; set; }
        public Guid? CartItemId { get; set; }
        public decimal LineTotal { get; set; }
    }

    public class CouponCalculationResult
    {
        public Guid? CouponId { get; set; }
        public string? CouponCode { get; set; }
        public string? Scope { get; set; }
        public string? DiscountType { get; set; }
        public decimal DiscountValue { get; set; }
        public decimal? MaxDiscountAmount { get; set; }
        public decimal EligibleSubtotal { get; set; }
        public decimal CartSubtotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal TotalAfterDiscount { get; set; }
        public List<Guid> ApplicableCartItemIds { get; set; } = new List<Guid>();
        public List<Guid> InapplicableCartItemIds { get; set; } = new List<Guid>();
    }

    public class CouponProductDto
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
    }

    public class AdminCouponDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public CouponType Type { get; set; }
        public CouponScope Scope { get; set; }
        public decimal Value { get; set; }
        public decimal? MaxDiscountAmount { get; set; }
        public decimal? MinimumOrderAmount { get; set; }
        public int UsageLimit { get; set; }
        public int UsedCount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        public int ApplicableProductCount { get; set; }
    }

    public class AdminCouponDetailDto : AdminCouponDto
    {
        public List<CouponProductDto> ApplicableProducts { get; set; } = new List<CouponProductDto>();
    }

    public class AdminCreateCouponRequest
    {
        [Required]
        [StringLength(50)]
        public string Code { get; set; } = string.Empty;

        [Required]
        public CouponType Type { get; set; }

        [Required]
        public CouponScope Scope { get; set; } = CouponScope.Order;

        [Range(0.01, double.MaxValue)]
        public decimal Value { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal? MaxDiscountAmount { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? MinimumOrderAmount { get; set; }

        [Range(0, int.MaxValue)]
        public int UsageLimit { get; set; } = 0;

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public bool IsActive { get; set; } = true;

        public List<Guid>? ProductIds { get; set; }
    }

    public class AdminUpdateCouponRequest
    {
        [Required]
        [StringLength(50)]
        public string Code { get; set; } = string.Empty;

        [Required]
        public CouponType Type { get; set; }

        [Required]
        public CouponScope Scope { get; set; } = CouponScope.Order;

        [Range(0.01, double.MaxValue)]
        public decimal Value { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal? MaxDiscountAmount { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? MinimumOrderAmount { get; set; }

        [Range(0, int.MaxValue)]
        public int UsageLimit { get; set; } = 0;

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public bool IsActive { get; set; } = true;

        public List<Guid>? ProductIds { get; set; }
    }
}
