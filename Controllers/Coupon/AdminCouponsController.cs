using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using EXE02_Backend_RE_CAFE.DTOs;
using EXE02_Backend_RE_CAFE.Interfaces;
using EXE02_Backend_RE_CAFE.Models;

namespace EXE02_Backend_RE_CAFE.Controllers
{
    [ApiController]
    [Route("api/admin/coupons")]
    [Authorize(Roles = "Admin")]
    public class AdminCouponsController : BaseApiController
    {
        private readonly ICouponService _couponService;

        public AdminCouponsController(ICouponService couponService)
        {
            _couponService = couponService;
        }

        [HttpGet]
        public async Task<IActionResult> GetCoupons(
            [FromQuery] bool? isActive = null,
            [FromQuery] CouponScope? scope = null,
            [FromQuery] CouponType? type = null,
            [FromQuery] string? keyword = null)
        {
            var coupons = await _couponService.GetCouponsForAdminAsync(isActive, scope, type, keyword);
            return Ok(SuccessResponse(
                message: "Admin coupons retrieved successfully.",
                action: "GetAdminCoupons",
                data: coupons,
                statusCode: StatusCodes.Status200OK));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCouponById(Guid id)
        {
            var coupon = await _couponService.GetCouponByIdForAdminAsync(id);
            if (coupon == null)
            {
                return NotFound(ErrorResponse<object>(
                    message: $"Coupon with ID {id} not found.",
                    action: "GetAdminCouponById",
                    statusCode: StatusCodes.Status404NotFound));
            }

            return Ok(SuccessResponse(
                message: "Admin coupon retrieved successfully.",
                action: "GetAdminCouponById",
                data: coupon,
                statusCode: StatusCodes.Status200OK));
        }

        [HttpPost]
        public async Task<IActionResult> CreateCoupon([FromBody] AdminCreateCouponRequest request)
        {
            var coupon = await _couponService.CreateCouponAsync(request);
            return StatusCode(StatusCodes.Status201Created, SuccessResponse(
                message: "Coupon created successfully.",
                action: "CreateCoupon",
                data: coupon,
                statusCode: StatusCodes.Status201Created));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCoupon(Guid id, [FromBody] AdminUpdateCouponRequest request)
        {
            var coupon = await _couponService.UpdateCouponAsync(id, request);
            return Ok(SuccessResponse(
                message: "Coupon updated successfully.",
                action: "UpdateCoupon",
                data: coupon,
                statusCode: StatusCodes.Status200OK));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCoupon(Guid id)
        {
            var result = await _couponService.SoftDeleteCouponAsync(id);
            if (!result)
            {
                return BadRequest(ErrorResponse<object>(
                    message: "Failed to delete coupon.",
                    action: "DeleteCoupon",
                    statusCode: StatusCodes.Status400BadRequest));
            }

            return Ok(SuccessResponse<object>(
                message: "Coupon soft-deleted successfully.",
                action: "DeleteCoupon",
                data: null,
                statusCode: StatusCodes.Status200OK));
        }
    }
}
