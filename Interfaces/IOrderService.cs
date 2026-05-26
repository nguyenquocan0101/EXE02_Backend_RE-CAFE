using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EXE02_Backend_RE_CAFE.DTOs;

namespace EXE02_Backend_RE_CAFE.Interfaces
{
    public interface IOrderService
    {
        // User Operations
        Task<OrderDto> CreateOrderAsync(Guid userId, CreateOrderRequest request);
        Task<OrderDto> CheckoutAsync(Guid userId, CheckoutOrderRequest request);
        Task<IEnumerable<OrderDto>> GetMyOrdersAsync(Guid userId);
        Task<OrderDto?> GetOrderByIdAsync(Guid userId, Guid orderId);
        Task<OrderDto> CancelOrderAsync(Guid userId, Guid orderId);

        // Admin Operations
        Task<IEnumerable<OrderDto>> GetAllOrdersAsync();
        Task<OrderDto?> GetOrderByIdAdminAsync(Guid orderId);
        Task<OrderDto> UpdateOrderStatusAsync(Guid orderId, UpdateOrderStatusRequest request);
    }
}
