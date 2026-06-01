using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EXE02_Backend_RE_CAFE.DTOs;
using EXE02_Backend_RE_CAFE.Models;

namespace EXE02_Backend_RE_CAFE.Interfaces
{
    public interface ICouponService
    {
        Task<CouponPreviewDto> PreviewCouponAsync(Guid userId, PreviewCouponRequest request);
        Task<CouponCalculationResult> CalculateCouponAsync(
            string? couponCode,
            IReadOnlyCollection<CouponLineItemInput> lineItems,
            decimal subtotal,
            decimal shippingFee,
            bool consumeUsage);
        Task<IEnumerable<AdminCouponDto>> GetCouponsForAdminAsync(
            bool? isActive = null,
            CouponScope? scope = null,
            CouponType? type = null,
            string? keyword = null);
        Task<AdminCouponDetailDto?> GetCouponByIdForAdminAsync(Guid id);
        Task<AdminCouponDetailDto> CreateCouponAsync(AdminCreateCouponRequest request);
        Task<AdminCouponDetailDto> UpdateCouponAsync(Guid id, AdminUpdateCouponRequest request);
        Task<bool> SoftDeleteCouponAsync(Guid id);
    }
}
