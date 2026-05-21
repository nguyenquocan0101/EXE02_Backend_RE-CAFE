using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EXE02_Backend_RE_CAFE.Data;
using EXE02_Backend_RE_CAFE.DTOs;
using EXE02_Backend_RE_CAFE.Interfaces;
using EXE02_Backend_RE_CAFE.Exceptions;
using EXE02_Backend_RE_CAFE.Models;

namespace EXE02_Backend_RE_CAFE.Services
{
    public class CartService : ICartService
    {
        private readonly ApplicationDbContext _context;

        public CartService(ApplicationDbContext context)
        {
            _context = context;
        }

        private async Task<Cart> GetOrCreateCartInternalAsync(Guid userId)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product!)
                        .ThenInclude(p => p.ProductImages)
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Variant)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            return cart;
        }

        public async Task<CartDto> GetCartByUserIdAsync(Guid userId)
        {
            var cart = await GetOrCreateCartInternalAsync(userId);
            return MapToDto(cart);
        }

        public async Task<CartDto> AddItemToCartAsync(Guid userId, AddCartItemRequest request)
        {
            var cart = await GetOrCreateCartInternalAsync(userId);

            // Verify product exists and is active
            var product = await _context.Products
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.Id == request.ProductId && p.IsActive);

            if (product == null)
            {
                throw new NotFoundException($"Active product with ID {request.ProductId} not found.");
            }

            // Verify variant if specified
            ProductVariant? variant = null;
            if (request.VariantId.HasValue)
            {
                variant = await _context.ProductVariants
                    .FirstOrDefaultAsync(pv => pv.Id == request.VariantId.Value && pv.ProductId == request.ProductId && pv.IsActive);

                if (variant == null)
                {
                    throw new NotFoundException($"Active variant with ID {request.VariantId} not found for product {request.ProductId}.");
                }
            }

            // Check if item already exists in cart
            var existingItem = cart.CartItems.FirstOrDefault(ci => 
                ci.ProductId == request.ProductId && 
                ci.VariantId == request.VariantId);

            if (existingItem != null)
            {
                existingItem.Quantity += request.Quantity;
                if (!string.IsNullOrEmpty(request.PersonalizationNote))
                {
                    existingItem.PersonalizationNote = request.PersonalizationNote;
                }
                _context.CartItems.Update(existingItem);
            }
            else
            {
                var newItem = new CartItem
                {
                    CartId = cart.Id,
                    ProductId = request.ProductId,
                    VariantId = request.VariantId,
                    Quantity = request.Quantity,
                    PersonalizationNote = request.PersonalizationNote
                };
                _context.CartItems.Add(newItem);
                cart.CartItems.Add(newItem); // keep local collection in sync
            }

            cart.UpdatedAt = DateTime.UtcNow;
            _context.Carts.Update(cart);
            await _context.SaveChangesAsync();

            // Refresh cart from database to ensure eager loaded properties are populated correctly for mapping
            var updatedCart = await GetOrCreateCartInternalAsync(userId);
            return MapToDto(updatedCart);
        }

        public async Task<CartDto> UpdateCartItemAsync(Guid userId, Guid cartItemId, UpdateCartItemRequest request)
        {
            var cart = await GetOrCreateCartInternalAsync(userId);

            var cartItem = cart.CartItems.FirstOrDefault(ci => ci.Id == cartItemId);
            if (cartItem == null)
            {
                throw new NotFoundException($"Cart item with ID {cartItemId} not found in your cart.");
            }

            cartItem.Quantity = request.Quantity;
            cartItem.PersonalizationNote = request.PersonalizationNote;

            _context.CartItems.Update(cartItem);
            cart.UpdatedAt = DateTime.UtcNow;
            _context.Carts.Update(cart);
            await _context.SaveChangesAsync();

            var updatedCart = await GetOrCreateCartInternalAsync(userId);
            return MapToDto(updatedCart);
        }

        public async Task<CartDto> RemoveCartItemAsync(Guid userId, Guid cartItemId)
        {
            var cart = await GetOrCreateCartInternalAsync(userId);

            var cartItem = cart.CartItems.FirstOrDefault(ci => ci.Id == cartItemId);
            if (cartItem == null)
            {
                throw new NotFoundException($"Cart item with ID {cartItemId} not found in your cart.");
            }

            _context.CartItems.Remove(cartItem);
            cart.CartItems.Remove(cartItem); // keep local collection in sync
            cart.UpdatedAt = DateTime.UtcNow;
            _context.Carts.Update(cart);
            await _context.SaveChangesAsync();

            var updatedCart = await GetOrCreateCartInternalAsync(userId);
            return MapToDto(updatedCart);
        }

        public async Task<CartDto> ClearCartAsync(Guid userId)
        {
            var cart = await GetOrCreateCartInternalAsync(userId);

            if (cart.CartItems.Any())
            {
                _context.CartItems.RemoveRange(cart.CartItems);
                cart.CartItems.Clear(); // keep local collection in sync
            }

            cart.UpdatedAt = DateTime.UtcNow;
            _context.Carts.Update(cart);
            await _context.SaveChangesAsync();

            var updatedCart = await GetOrCreateCartInternalAsync(userId);
            return MapToDto(updatedCart);
        }

        private CartDto MapToDto(Cart cart)
        {
            var cartDto = new CartDto
            {
                Id = cart.Id,
                UserId = cart.UserId,
                CreatedAt = cart.CreatedAt,
                UpdatedAt = cart.UpdatedAt,
                CartItems = cart.CartItems.Select(ci =>
                {
                    decimal unitPrice = ci.Product != null ? ci.Product.Price : 0;
                    string? thumbnail = null;

                    if (ci.Product != null)
                    {
                        if (ci.Variant != null)
                        {
                            unitPrice = ci.Variant.Price;
                        }
                        else if (ci.Product.SalePrice.HasValue)
                        {
                            unitPrice = ci.Product.SalePrice.Value;
                        }

                        thumbnail = ci.Product.ProductImages?.FirstOrDefault(pi => pi.IsThumbnail)?.ImageUrl 
                                    ?? ci.Product.ProductImages?.FirstOrDefault()?.ImageUrl;
                    }

                    return new CartItemDto
                    {
                        Id = ci.Id,
                        ProductId = ci.ProductId,
                        ProductName = ci.Product?.Name ?? "Unknown Product",
                        ProductThumbnail = thumbnail,
                        UnitPrice = unitPrice,
                        VariantId = ci.VariantId,
                        VariantName = ci.Variant?.VariantName,
                        Quantity = ci.Quantity,
                        PersonalizationNote = ci.PersonalizationNote,
                        TotalPrice = unitPrice * ci.Quantity
                    };
                }).ToList()
            };

            cartDto.TotalAmount = cartDto.CartItems.Sum(ci => ci.TotalPrice);
            return cartDto;
        }
    }
}
