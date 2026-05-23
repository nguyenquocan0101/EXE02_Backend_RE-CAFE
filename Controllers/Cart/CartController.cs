using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using EXE02_Backend_RE_CAFE.DTOs;
using EXE02_Backend_RE_CAFE.Interfaces;
using EXE02_Backend_RE_CAFE.Exceptions;

namespace EXE02_Backend_RE_CAFE.Controllers
{
    [ApiController]
    [Route("api/cart")]
    [Authorize]
    public class CartController : BaseApiController
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
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

        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            var userId = GetUserId();
            var cart = await _cartService.GetCartByUserIdAsync(userId);
            return Ok(SuccessResponse(
                message: "Cart retrieved successfully.",
                action: "GetCart",
                data: cart,
                statusCode: StatusCodes.Status200OK));
        }

        [HttpPost("items")]
        public async Task<IActionResult> AddItemToCart([FromBody] AddCartItemRequest request)
        {
            var userId = GetUserId();
            var cart = await _cartService.AddItemToCartAsync(userId, request);
            return Ok(SuccessResponse(
                message: "Item added to cart successfully.",
                action: "AddItemToCart",
                data: cart,
                statusCode: StatusCodes.Status200OK));
        }

        [HttpPut("items/{id}")]
        public async Task<IActionResult> UpdateCartItem(Guid id, [FromBody] UpdateCartItemRequest request)
        {
            var userId = GetUserId();
            var cart = await _cartService.UpdateCartItemAsync(userId, id, request);
            return Ok(SuccessResponse(
                message: "Cart item updated successfully.",
                action: "UpdateCartItem",
                data: cart,
                statusCode: StatusCodes.Status200OK));
        }

        [HttpDelete("items/{id}")]
        public async Task<IActionResult> RemoveCartItem(Guid id)
        {
            var userId = GetUserId();
            var cart = await _cartService.RemoveCartItemAsync(userId, id);
            return Ok(SuccessResponse(
                message: "Cart item removed successfully.",
                action: "RemoveCartItem",
                data: cart,
                statusCode: StatusCodes.Status200OK));
        }

        [HttpDelete("clear")]
        public async Task<IActionResult> ClearCart()
        {
            var userId = GetUserId();
            var cart = await _cartService.ClearCartAsync(userId);
            return Ok(SuccessResponse(
                message: "Cart cleared successfully.",
                action: "ClearCart",
                data: cart,
                statusCode: StatusCodes.Status200OK));
        }
    }
}
