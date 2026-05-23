using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using EXE02_Backend_RE_CAFE.DTOs;
using EXE02_Backend_RE_CAFE.Interfaces;

namespace EXE02_Backend_RE_CAFE.Controllers
{
    [ApiController]
    [Route("api/admin/orders")]
    [Authorize(Roles = "Admin")]
    public class AdminOrdersController : BaseApiController
    {
        private readonly IOrderService _orderService;

        public AdminOrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllOrders()
        {
            var orders = await _orderService.GetAllOrdersAsync();
            return Ok(SuccessResponse(
                message: "All orders retrieved successfully.",
                action: "GetAllOrders",
                data: orders,
                statusCode: StatusCodes.Status200OK));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderById(Guid id)
        {
            var order = await _orderService.GetOrderByIdAdminAsync(id);
            if (order == null)
            {
                return NotFound(ErrorResponse<object>(
                    message: $"Order with ID {id} not found.",
                    action: "GetOrderByIdAdmin",
                    statusCode: StatusCodes.Status404NotFound));
            }
            return Ok(SuccessResponse(
                message: "Order retrieved successfully.",
                action: "GetOrderByIdAdmin",
                data: order,
                statusCode: StatusCodes.Status200OK));
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateOrderStatus(Guid id, [FromBody] UpdateOrderStatusRequest request)
        {
            var order = await _orderService.UpdateOrderStatusAsync(id, request);
            return Ok(SuccessResponse(
                message: "Order status updated successfully.",
                action: "UpdateOrderStatus",
                data: order,
                statusCode: StatusCodes.Status200OK));
        }
    }
}
