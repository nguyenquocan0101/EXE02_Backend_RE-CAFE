using System;
using System.Threading.Tasks;
using EXE02_Backend_RE_CAFE.DTOs;

namespace EXE02_Backend_RE_CAFE.Interfaces
{
    public interface ICartService
    {
        Task<CartDto> GetCartByUserIdAsync(Guid userId);
        Task<CartDto> AddItemToCartAsync(Guid userId, AddCartItemRequest request);
        Task<CartDto> UpdateCartItemAsync(Guid userId, Guid cartItemId, UpdateCartItemRequest request);
        Task<CartDto> RemoveCartItemAsync(Guid userId, Guid cartItemId);
        Task<CartDto> ClearCartAsync(Guid userId);
    }
}
