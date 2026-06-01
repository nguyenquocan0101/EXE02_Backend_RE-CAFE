using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using EXE02_Backend_RE_CAFE.DTOs;
using EXE02_Backend_RE_CAFE.Exceptions;
using EXE02_Backend_RE_CAFE.Interfaces;

namespace EXE02_Backend_RE_CAFE.Controllers
{
    [ApiController]
    [Route("api/coupons")]
    [Authorize]
    public class CouponsController : BaseApiController
    {
        private readonly ICouponService _couponService;

        public CouponsController(ICouponService couponService)
        {
            _couponService = couponService;
        }

        private Guid GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                throw new UnauthorizedException("Unauthorized request.");
            }
            return userId;
        }

        [HttpPost("preview")]
        public async Task<IActionResult> Preview([FromBody] PreviewCouponRequest request)
        {
            var userId = GetUserId();
            var preview = await _couponService.PreviewCouponAsync(userId, request);
            return Ok(SuccessResponse(
                message: "Coupon is valid and applied.",
                action: "PreviewCoupon",
                data: preview,
                statusCode: StatusCodes.Status200OK));
        }
    }
}
