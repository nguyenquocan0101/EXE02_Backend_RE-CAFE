using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EXE02_Backend_RE_CAFE.DTOs;
using EXE02_Backend_RE_CAFE.Interfaces;
using EXE02_Backend_RE_CAFE.Exceptions;

namespace EXE02_Backend_RE_CAFE.Controllers
{
    [ApiController]
    [Route("api/orders")]
    [Authorize]
    public class OrdersController : ControllerBase
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
            return StatusCode(201, order);
        }

        [HttpGet("my-orders")]
        public async Task<IActionResult> GetMyOrders()
        {
            var userId = GetUserId();
            var orders = await _orderService.GetMyOrdersAsync(userId);
            return Ok(orders);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderById(Guid id)
        {
            var userId = GetUserId();
            var order = await _orderService.GetOrderByIdAsync(userId, id);
            if (order == null)
            {
                return NotFound(new { message = $"Order with ID {id} not found." });
            }
            return Ok(order);
        }

        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> CancelOrder(Guid id)
        {
            var userId = GetUserId();
            var order = await _orderService.CancelOrderAsync(userId, id);
            return Ok(order);
        }
    }
}
