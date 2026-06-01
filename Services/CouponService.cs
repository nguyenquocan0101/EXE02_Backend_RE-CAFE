using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EXE02_Backend_RE_CAFE.Data;
using EXE02_Backend_RE_CAFE.DTOs;
using EXE02_Backend_RE_CAFE.Exceptions;
using EXE02_Backend_RE_CAFE.Interfaces;
using EXE02_Backend_RE_CAFE.Models;

namespace EXE02_Backend_RE_CAFE.Services
{
    public class CouponService : ICouponService
    {
        private readonly ApplicationDbContext _context;

        public CouponService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<CouponPreviewDto> PreviewCouponAsync(Guid userId, PreviewCouponRequest request)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Variant)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || !cart.CartItems.Any())
            {
                throw new BadRequestException("Your shopping cart is empty. Cannot preview coupon.");
            }

            var itemsToEvaluate = cart.CartItems.ToList();
            if (request.CartItemIds != null && request.CartItemIds.Any())
            {
                itemsToEvaluate = cart.CartItems.Where(ci => request.CartItemIds.Contains(ci.Id)).ToList();
                if (!itemsToEvaluate.Any())
                {
                    throw new BadRequestException("None of the selected items were found in your cart.");
                }
            }

            decimal subtotal = 0;
            var lineItems = new List<CouponLineItemInput>();

            foreach (var item in itemsToEvaluate)
            {
                if (item.Product == null || !item.Product.IsActive)
                {
                    throw new BadRequestException($"Product '{item.Product?.Name ?? "Unknown"}' is no longer active and cannot use coupon.");
                }

                decimal unitPrice = item.Product.Price;

                if (item.VariantId.HasValue)
                {
                    var variant = item.Variant;
                    if (variant == null || !variant.IsActive)
                    {
                        throw new BadRequestException($"Selected variant for product '{item.Product.Name}' is no longer active.");
                    }

                    unitPrice = variant.Price;
                }
                else if (item.Product.SalePrice.HasValue)
                {
                    unitPrice = item.Product.SalePrice.Value;
                }

                decimal lineTotal = unitPrice * item.Quantity;
                subtotal += lineTotal;

                lineItems.Add(new CouponLineItemInput
                {
                    CartItemId = item.Id,
                    ProductId = item.ProductId,
                    LineTotal = lineTotal
                });
            }

            decimal shippingFee = subtotal >= 200000 ? 0 : 30000;
            var calculation = await CalculateCouponAsync(request.CouponCode, lineItems, subtotal, shippingFee, false);

            return new CouponPreviewDto
            {
                CouponCode = calculation.CouponCode ?? string.Empty,
                Scope = calculation.Scope ?? string.Empty,
                DiscountType = calculation.DiscountType ?? string.Empty,
                DiscountValue = calculation.DiscountValue,
                MaxDiscountAmount = calculation.MaxDiscountAmount,
                EligibleSubtotal = calculation.EligibleSubtotal,
                CartSubtotal = calculation.CartSubtotal,
                DiscountAmount = calculation.DiscountAmount,
                ShippingFee = calculation.ShippingFee,
                TotalAfterDiscount = calculation.TotalAfterDiscount,
                ApplicableCartItemIds = calculation.ApplicableCartItemIds,
                InapplicableCartItemIds = calculation.InapplicableCartItemIds
            };
        }

        public async Task<CouponCalculationResult> CalculateCouponAsync(
            string? couponCode,
            IReadOnlyCollection<CouponLineItemInput> lineItems,
            decimal subtotal,
            decimal shippingFee,
            bool consumeUsage)
        {
            var result = new CouponCalculationResult
            {
                CartSubtotal = subtotal,
                ShippingFee = shippingFee,
                DiscountAmount = 0,
                TotalAfterDiscount = subtotal + shippingFee
            };

            if (string.IsNullOrWhiteSpace(couponCode))
            {
                return result;
            }

            var normalizedCode = couponCode.Trim().ToLower();
            var coupon = await _context.Coupons
                .Include(c => c.CouponProducts)
                .FirstOrDefaultAsync(c => c.Code.ToLower() == normalizedCode && c.IsActive);

            if (coupon == null)
            {
                throw new BadRequestException("The coupon code provided is invalid or inactive.");
            }

            if (DateTime.UtcNow < coupon.StartDate || DateTime.UtcNow > coupon.EndDate)
            {
                throw new BadRequestException("This coupon has either expired or is not yet active.");
            }

            if (coupon.UsageLimit > 0 && coupon.UsedCount >= coupon.UsageLimit)
            {
                throw new BadRequestException("This coupon has reached its maximum usage limit.");
            }

            var eligibleSubtotal = subtotal;
            var applicableCartItemIds = new List<Guid>();
            var inapplicableCartItemIds = new List<Guid>();

            if (coupon.Scope == CouponScope.Product)
            {
                var eligibleProductIds = coupon.CouponProducts
                    .Select(cp => cp.ProductId)
                    .ToHashSet();

                if (!eligibleProductIds.Any())
                {
                    throw new BadRequestException("This coupon is not configured for any products.");
                }

                var applicableItems = lineItems.Where(i => eligibleProductIds.Contains(i.ProductId)).ToList();
                var inapplicableItems = lineItems.Where(i => !eligibleProductIds.Contains(i.ProductId)).ToList();

                eligibleSubtotal = applicableItems.Sum(i => i.LineTotal);

                applicableCartItemIds = applicableItems
                    .Where(i => i.CartItemId.HasValue)
                    .Select(i => i.CartItemId!.Value)
                    .ToList();
                inapplicableCartItemIds = inapplicableItems
                    .Where(i => i.CartItemId.HasValue)
                    .Select(i => i.CartItemId!.Value)
                    .ToList();

                if (eligibleSubtotal <= 0)
                {
                    throw new BadRequestException("This coupon is not applicable to selected products.");
                }
            }
            else
            {
                applicableCartItemIds = lineItems
                    .Where(i => i.CartItemId.HasValue)
                    .Select(i => i.CartItemId!.Value)
                    .ToList();
            }

            if (coupon.MinimumOrderAmount.HasValue && eligibleSubtotal < coupon.MinimumOrderAmount.Value)
            {
                throw new BadRequestException($"Your eligible subtotal must be at least {coupon.MinimumOrderAmount.Value:N0} VND to use this coupon.");
            }

            decimal discountAmount = 0;

            if (coupon.Type == CouponType.Percentage)
            {
                var discountBase = coupon.Scope == CouponScope.Product ? eligibleSubtotal : subtotal;
                discountAmount = discountBase * (coupon.Value / 100m);

                if (coupon.MaxDiscountAmount.HasValue)
                {
                    discountAmount = Math.Min(discountAmount, coupon.MaxDiscountAmount.Value);
                }
            }
            else if (coupon.Type == CouponType.FixedAmount)
            {
                discountAmount = coupon.Value;
            }
            else if (coupon.Type == CouponType.FreeShipping)
            {
                discountAmount = shippingFee;
            }

            if (coupon.Scope == CouponScope.Product && coupon.Type != CouponType.FreeShipping)
            {
                discountAmount = Math.Min(discountAmount, eligibleSubtotal);
            }

            discountAmount = Math.Max(0, discountAmount);
            discountAmount = Math.Min(discountAmount, subtotal + shippingFee);

            if (consumeUsage)
            {
                coupon.UsedCount++;
                _context.Coupons.Update(coupon);
            }

            result.CouponId = coupon.Id;
            result.CouponCode = coupon.Code;
            result.Scope = coupon.Scope.ToString();
            result.DiscountType = coupon.Type.ToString();
            result.DiscountValue = coupon.Value;
            result.MaxDiscountAmount = coupon.MaxDiscountAmount;
            result.EligibleSubtotal = eligibleSubtotal;
            result.DiscountAmount = discountAmount;
            result.TotalAfterDiscount = Math.Max(0, subtotal + shippingFee - discountAmount);
            result.ApplicableCartItemIds = applicableCartItemIds;
            result.InapplicableCartItemIds = inapplicableCartItemIds;

            return result;
        }

        public async Task<IEnumerable<AdminCouponDto>> GetCouponsForAdminAsync(
            bool? isActive = null,
            CouponScope? scope = null,
            CouponType? type = null,
            string? keyword = null)
        {
            var query = _context.Coupons
                .Include(c => c.CouponProducts)
                .AsQueryable();

            if (isActive.HasValue)
            {
                query = query.Where(c => c.IsActive == isActive.Value);
            }

            if (scope.HasValue)
            {
                query = query.Where(c => c.Scope == scope.Value);
            }

            if (type.HasValue)
            {
                query = query.Where(c => c.Type == type.Value);
            }

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var normalizedKeyword = keyword.Trim().ToLower();
                query = query.Where(c => c.Code.ToLower().Contains(normalizedKeyword));
            }

            return await query
                .OrderByDescending(c => c.StartDate)
                .Select(c => new AdminCouponDto
                {
                    Id = c.Id,
                    Code = c.Code,
                    Type = c.Type,
                    Scope = c.Scope,
                    Value = c.Value,
                    MaxDiscountAmount = c.MaxDiscountAmount,
                    MinimumOrderAmount = c.MinimumOrderAmount,
                    UsageLimit = c.UsageLimit,
                    UsedCount = c.UsedCount,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    IsActive = c.IsActive,
                    ApplicableProductCount = c.CouponProducts.Count
                })
                .ToListAsync();
        }

        public async Task<AdminCouponDetailDto?> GetCouponByIdForAdminAsync(Guid id)
        {
            var coupon = await _context.Coupons
                .Include(c => c.CouponProducts)
                    .ThenInclude(cp => cp.Product)
                .FirstOrDefaultAsync(c => c.Id == id);

            return coupon == null ? null : MapToAdminDetailDto(coupon);
        }

        public async Task<AdminCouponDetailDto> CreateCouponAsync(AdminCreateCouponRequest request)
        {
            ValidateCouponPayload(
                request.Code,
                request.Type,
                request.Scope,
                request.Value,
                request.MaxDiscountAmount,
                request.StartDate,
                request.EndDate);

            var normalizedCode = request.Code.Trim().ToUpperInvariant();
            if (await _context.Coupons.AnyAsync(c => c.Code.ToLower() == normalizedCode.ToLower()))
            {
                throw new BadRequestException($"Coupon code '{normalizedCode}' already exists.");
            }

            var productIds = request.ProductIds?.Distinct().ToList() ?? new List<Guid>();
            await ValidateProductScopeAsync(request.Scope, productIds);

            var coupon = new Coupon
            {
                Code = normalizedCode,
                Type = request.Type,
                Scope = request.Scope,
                Value = request.Value,
                MaxDiscountAmount = request.Type == CouponType.Percentage ? request.MaxDiscountAmount : null,
                MinimumOrderAmount = request.MinimumOrderAmount,
                UsageLimit = request.UsageLimit,
                UsedCount = 0,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                IsActive = request.IsActive
            };

            _context.Coupons.Add(coupon);

            if (coupon.Scope == CouponScope.Product)
            {
                foreach (var productId in productIds)
                {
                    _context.CouponProducts.Add(new CouponProduct
                    {
                        CouponId = coupon.Id,
                        ProductId = productId
                    });
                }
            }

            await _context.SaveChangesAsync();

            var created = await _context.Coupons
                .Include(c => c.CouponProducts)
                    .ThenInclude(cp => cp.Product)
                .FirstOrDefaultAsync(c => c.Id == coupon.Id);

            return MapToAdminDetailDto(created!);
        }

        public async Task<AdminCouponDetailDto> UpdateCouponAsync(Guid id, AdminUpdateCouponRequest request)
        {
            ValidateCouponPayload(
                request.Code,
                request.Type,
                request.Scope,
                request.Value,
                request.MaxDiscountAmount,
                request.StartDate,
                request.EndDate);

            var coupon = await _context.Coupons
                .Include(c => c.CouponProducts)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (coupon == null)
            {
                throw new NotFoundException($"Coupon with ID {id} not found.");
            }

            var normalizedCode = request.Code.Trim().ToUpperInvariant();
            if (await _context.Coupons.AnyAsync(c => c.Id != id && c.Code.ToLower() == normalizedCode.ToLower()))
            {
                throw new BadRequestException($"Coupon code '{normalizedCode}' already exists on another coupon.");
            }

            var productIds = request.ProductIds?.Distinct().ToList() ?? new List<Guid>();
            await ValidateProductScopeAsync(request.Scope, productIds);

            coupon.Code = normalizedCode;
            coupon.Type = request.Type;
            coupon.Scope = request.Scope;
            coupon.Value = request.Value;
            coupon.MaxDiscountAmount = request.Type == CouponType.Percentage ? request.MaxDiscountAmount : null;
            coupon.MinimumOrderAmount = request.MinimumOrderAmount;
            coupon.UsageLimit = request.UsageLimit;
            coupon.StartDate = request.StartDate;
            coupon.EndDate = request.EndDate;
            coupon.IsActive = request.IsActive;

            _context.CouponProducts.RemoveRange(coupon.CouponProducts);

            if (coupon.Scope == CouponScope.Product)
            {
                foreach (var productId in productIds)
                {
                    _context.CouponProducts.Add(new CouponProduct
                    {
                        CouponId = coupon.Id,
                        ProductId = productId
                    });
                }
            }

            _context.Coupons.Update(coupon);
            await _context.SaveChangesAsync();

            var updated = await _context.Coupons
                .Include(c => c.CouponProducts)
                    .ThenInclude(cp => cp.Product)
                .FirstOrDefaultAsync(c => c.Id == coupon.Id);

            return MapToAdminDetailDto(updated!);
        }

        public async Task<bool> SoftDeleteCouponAsync(Guid id)
        {
            var coupon = await _context.Coupons.FirstOrDefaultAsync(c => c.Id == id);
            if (coupon == null)
            {
                throw new NotFoundException($"Coupon with ID {id} not found.");
            }

            coupon.IsActive = false;
            _context.Coupons.Update(coupon);
            await _context.SaveChangesAsync();
            return true;
        }

        private static void ValidateCouponPayload(
            string code,
            CouponType type,
            CouponScope scope,
            decimal value,
            decimal? maxDiscountAmount,
            DateTime startDate,
            DateTime endDate)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new BadRequestException("Coupon code is required.");
            }

            if (endDate <= startDate)
            {
                throw new BadRequestException("EndDate must be greater than StartDate.");
            }

            if (value <= 0)
            {
                throw new BadRequestException("Coupon value must be greater than 0.");
            }

            if (type == CouponType.Percentage && value > 100)
            {
                throw new BadRequestException("Percentage coupon value cannot exceed 100.");
            }

            if (type != CouponType.Percentage && maxDiscountAmount.HasValue)
            {
                throw new BadRequestException("MaxDiscountAmount is only applicable for percentage coupons.");
            }
        }

        private async Task ValidateProductScopeAsync(CouponScope scope, List<Guid> productIds)
        {
            if (scope != CouponScope.Product)
            {
                return;
            }

            if (!productIds.Any())
            {
                throw new BadRequestException("Product scope coupon requires at least one product.");
            }

            var existingProductIds = await _context.Products
                .Where(p => productIds.Contains(p.Id))
                .Select(p => p.Id)
                .ToListAsync();

            var missingProductIds = productIds.Except(existingProductIds).ToList();
            if (missingProductIds.Any())
            {
                throw new NotFoundException($"Some products were not found for this coupon: {string.Join(", ", missingProductIds)}");
            }
        }

        private static AdminCouponDetailDto MapToAdminDetailDto(Coupon coupon)
        {
            return new AdminCouponDetailDto
            {
                Id = coupon.Id,
                Code = coupon.Code,
                Type = coupon.Type,
                Scope = coupon.Scope,
                Value = coupon.Value,
                MaxDiscountAmount = coupon.MaxDiscountAmount,
                MinimumOrderAmount = coupon.MinimumOrderAmount,
                UsageLimit = coupon.UsageLimit,
                UsedCount = coupon.UsedCount,
                StartDate = coupon.StartDate,
                EndDate = coupon.EndDate,
                IsActive = coupon.IsActive,
                ApplicableProductCount = coupon.CouponProducts.Count,
                ApplicableProducts = coupon.CouponProducts
                    .Select(cp => new CouponProductDto
                    {
                        ProductId = cp.ProductId,
                        ProductName = cp.Product?.Name ?? string.Empty
                    })
                    .ToList()
            };
        }
    }
}
