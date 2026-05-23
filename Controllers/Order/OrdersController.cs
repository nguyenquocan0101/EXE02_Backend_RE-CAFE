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
    [Route("api/orders")]
    [Authorize]
    public class OrdersController : BaseApiController
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
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

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
        {
            var userId = GetUserId();
            var order = await _orderService.CreateOrderAsync(userId, request);
            return StatusCode(StatusCodes.Status201Created, SuccessResponse(
                message: "Order created successfully.",
                action: "CreateOrder",
                data: order,
                statusCode: StatusCodes.Status201Created));
        }

        [HttpGet("my-orders")]
        public async Task<IActionResult> GetMyOrders()
        {
            var userId = GetUserId();
            var orders = await _orderService.GetMyOrdersAsync(userId);
            return Ok(SuccessResponse(
                message: "Orders retrieved successfully.",
                action: "GetMyOrders",
                data: orders,
                statusCode: StatusCodes.Status200OK));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderById(Guid id)
        {
            var userId = GetUserId();
            var order = await _orderService.GetOrderByIdAsync(userId, id);
            if (order == null)
            {
                return NotFound(ErrorResponse<object>(
                    message: $"Order with ID {id} not found.",
                    action: "GetOrderById",
                    statusCode: StatusCodes.Status404NotFound));
            }
            return Ok(SuccessResponse(
                message: "Order retrieved successfully.",
                action: "GetOrderById",
                data: order,
                statusCode: StatusCodes.Status200OK));
        }

        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> CancelOrder(Guid id)
        {
            var userId = GetUserId();
            var order = await _orderService.CancelOrderAsync(userId, id);
            return Ok(SuccessResponse(
                message: "Order cancelled successfully.",
                action: "CancelOrder",
                data: order,
                statusCode: StatusCodes.Status200OK));
        }
    }
}
