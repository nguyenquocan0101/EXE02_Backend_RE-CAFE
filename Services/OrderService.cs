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
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;
        private readonly IPaymentService _paymentService;
        private readonly IAddressService _addressService;

        public OrderService(ApplicationDbContext context, IPaymentService paymentService, IAddressService addressService)
        {
            _context = context;
            _paymentService = paymentService;
            _addressService = addressService;
        }

        public async Task<OrderDto> CreateOrderAsync(Guid userId, CreateOrderRequest request)
        {
            // 1. Fetch user's cart
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Variant)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || !cart.CartItems.Any())
            {
                throw new BadRequestException("Your shopping cart is empty. Cannot place an order.");
            }

            var itemsToOrder = cart.CartItems.ToList();
            if (request.CartItemIds != null && request.CartItemIds.Any())
            {
                itemsToOrder = cart.CartItems.Where(ci => request.CartItemIds.Contains(ci.Id)).ToList();
                if (!itemsToOrder.Any())
                {
                    throw new BadRequestException("None of the selected items were found in your cart.");
                }
            }

            // 2. Validate shipping address
            if (request.ShippingAddressId == Guid.Empty)
            {
                throw new BadRequestException("ShippingAddressId is required. Please select a valid shipping address.");
            }

            var address = await _context.Addresses
                .FirstOrDefaultAsync(a => a.Id == request.ShippingAddressId && a.UserId == userId);

            if (address == null)
            {
                throw new NotFoundException("Shipping address not found or does not belong to your account.");
            }

            // 3. Calculate financial totals
            decimal subtotal = 0;
            var orderItemsList = new List<OrderItem>();

            foreach (var item in itemsToOrder)
            {
                if (item.Product == null || !item.Product.IsActive)
                {
                    throw new BadRequestException($"Product '{item.Product?.Name ?? "Unknown"}' is no longer active and cannot be ordered.");
                }

                decimal unitPrice = item.Product.Price;

                if (item.VariantId.HasValue)
                {
                    var variant = item.Variant;
                    if (variant == null || !variant.IsActive)
                    {
                        throw new BadRequestException($"Selected variant for product '{item.Product.Name}' is no longer active.");
                    }

                    // Check and deduct stock if variant tracks inventory
                    if (variant.StockQuantity < item.Quantity)
                    {
                        throw new BadRequestException($"Insufficient stock for '{item.Product.Name} - {variant.VariantName}'. Available: {variant.StockQuantity}, Requested: {item.Quantity}");
                    }

                    variant.StockQuantity -= item.Quantity;
                    _context.ProductVariants.Update(variant);
                    unitPrice = variant.Price;
                }
                else if (item.Product.SalePrice.HasValue)
                {
                    unitPrice = item.Product.SalePrice.Value;
                }

                decimal itemTotal = unitPrice * item.Quantity;
                subtotal += itemTotal;

                orderItemsList.Add(new OrderItem
                {
                    ProductId = item.ProductId,
                    VariantId = item.VariantId,
                    ProductName = item.Product.Name + (item.Variant != null ? $" ({item.Variant.VariantName})" : ""),
                    UnitPrice = unitPrice,
                    Quantity = item.Quantity,
                    TotalPrice = itemTotal,
                    PersonalizationNote = item.PersonalizationNote
                });
            }

            // Flat shipping fee of 30,000 VND, free shipping for orders over 200,000 VND
            decimal shippingFee = subtotal >= 200000 ? 0 : 30000;
            decimal discountAmount = 0;
            Coupon? coupon = null;

            // 4. Apply coupon if provided
            if (!string.IsNullOrWhiteSpace(request.CouponCode))
            {
                coupon = await _context.Coupons
                    .FirstOrDefaultAsync(c => c.Code.ToLower() == request.CouponCode.Trim().ToLower() && c.IsActive);

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

                if (coupon.MinimumOrderAmount.HasValue && subtotal < coupon.MinimumOrderAmount.Value)
                {
                    throw new BadRequestException($"Your order subtotal must be at least {coupon.MinimumOrderAmount.Value:N0} VND to use this coupon.");
                }

                // Calculate discount
                if (coupon.Type == CouponType.Percentage)
                {
                    discountAmount = subtotal * (coupon.Value / 100m);
                }
                else if (coupon.Type == CouponType.FixedAmount)
                {
                    discountAmount = coupon.Value;
                }
                else if (coupon.Type == CouponType.FreeShipping)
                {
                    discountAmount = shippingFee;
                }

                // Bound discount amount to not exceed the subtotal + shipping fee
                discountAmount = Math.Min(discountAmount, subtotal + shippingFee);
                coupon.UsedCount++;
                _context.Coupons.Update(coupon);
            }

            decimal totalAmount = Math.Max(0, subtotal + shippingFee - discountAmount);

            // 5. Create Order
            var orderCode = $"ORD-{DateTime.UtcNow:yyyyMMdd}-{new Random().Next(1000, 9999)}";
            var order = new Order
            {
                UserId = userId,
                OrderCode = orderCode,
                ShippingAddressId = request.ShippingAddressId,
                Subtotal = subtotal,
                ShippingFee = shippingFee,
                DiscountAmount = discountAmount,
                TotalAmount = totalAmount,
                Status = OrderStatus.Pending,
                PaymentStatus = PaymentStatus.Unpaid,
                Note = request.Note,
                CreatedAt = DateTime.UtcNow,
                CouponId = coupon?.Id,
                Payment = new Payment
                {
                    Method = request.PaymentMethod,
                    Status = PaymentStatus.Unpaid,
                    Amount = totalAmount
                }
            };

            foreach (var orderItem in orderItemsList)
            {
                orderItem.OrderId = order.Id;
                _context.OrderItems.Add(orderItem);
                order.OrderItems.Add(orderItem);
            }

            _context.Orders.Add(order);

            // 6. Clear selected shopping cart items
            _context.CartItems.RemoveRange(itemsToOrder);
            foreach (var item in itemsToOrder)
            {
                cart.CartItems.Remove(item);
            }
            _context.Carts.Update(cart);

            await _context.SaveChangesAsync();

            // Reload order with relations for DTO mapping
            var createdOrder = await _context.Orders
                .Include(o => o.ShippingAddress)
                .Include(o => o.Coupon)
                .Include(o => o.Payment)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Variant)
                .FirstOrDefaultAsync(o => o.Id == order.Id);

            return MapToDto(createdOrder!);
        }

        public async Task<OrderDto> CheckoutAsync(Guid userId, CheckoutOrderRequest request)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var createdAddress = await _addressService.CreateAddressAsync(userId, request.ShippingAddress);
                var createOrderRequest = new CreateOrderRequest
                {
                    ShippingAddressId = createdAddress.Id,
                    Note = request.Note,
                    CouponCode = request.CouponCode,
                    PaymentMethod = request.PaymentMethod,
                    CartItemIds = request.CartItemIds
                };

                var order = await CreateOrderAsync(userId, createOrderRequest);
                await transaction.CommitAsync();
                return order;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<IEnumerable<OrderDto>> GetMyOrdersAsync(Guid userId)
        {
            var orders = await _context.Orders
                .Include(o => o.ShippingAddress)
                .Include(o => o.Coupon)
                .Include(o => o.Payment)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Variant)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return orders.Select(MapToDto);
        }

        public async Task<OrderDto?> GetOrderByIdAsync(Guid userId, Guid orderId)
        {
            var order = await _context.Orders
                .Include(o => o.ShippingAddress)
                .Include(o => o.Coupon)
                .Include(o => o.Payment)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Variant)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

            if (order == null)
            {
                return null;
            }

            return MapToDto(order);
        }

        public async Task<OrderDto> CancelOrderAsync(Guid userId, Guid orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

            if (order == null)
            {
                throw new NotFoundException($"Order with ID {orderId} not found.");
            }

            if (order.Status == OrderStatus.Cancelled)
            {
                throw new BadRequestException("Order is already cancelled.");
            }

            if (order.Status == OrderStatus.Completed || order.Status == OrderStatus.Shipping)
            {
                throw new BadRequestException($"Orders that are in status '{order.Status}' cannot be cancelled.");
            }

            // Transition status
            order.Status = OrderStatus.Cancelled;
            _context.Orders.Update(order);

            // Restock variants if necessary
            foreach (var item in order.OrderItems)
            {
                if (item.VariantId.HasValue)
                {
                    var variant = await _context.ProductVariants.FindAsync(item.VariantId.Value);
                    if (variant != null)
                    {
                        variant.StockQuantity += item.Quantity;
                        _context.ProductVariants.Update(variant);
                    }
                }
            }

            await _context.SaveChangesAsync();

            var updatedOrder = await _context.Orders
                .Include(o => o.ShippingAddress)
                .Include(o => o.Coupon)
                .Include(o => o.Payment)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Variant)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            return MapToDto(updatedOrder!);
        }

        public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync()
        {
            var orders = await _context.Orders
                .Include(o => o.ShippingAddress)
                .Include(o => o.Coupon)
                .Include(o => o.Payment)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Variant)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return orders.Select(MapToDto);
        }

        public async Task<OrderDto?> GetOrderByIdAdminAsync(Guid orderId)
        {
            var order = await _context.Orders
                .Include(o => o.ShippingAddress)
                .Include(o => o.Coupon)
                .Include(o => o.Payment)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Variant)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                return null;
            }

            return MapToDto(order);
        }

        public async Task<OrderDto> UpdateOrderStatusAsync(Guid orderId, UpdateOrderStatusRequest request)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                throw new NotFoundException($"Order with ID {orderId} not found.");
            }

            if (!Enum.TryParse<OrderStatus>(request.Status, true, out var targetStatus))
            {
                throw new BadRequestException($"Invalid order status status: '{request.Status}'. Valid statuses: Pending, Confirmed, Preparing, Shipping, Completed, Cancelled, Returned.");
            }

            if (order.Status == targetStatus)
            {
                // No change
                var currentOrder = await _context.Orders
                    .Include(o => o.ShippingAddress)
                    .Include(o => o.Coupon)
                    .Include(o => o.Payment)
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Variant)
                    .FirstOrDefaultAsync(o => o.Id == orderId);
                return MapToDto(currentOrder!);
            }

            // If transitioning to Cancelled, restock inventory
            if (targetStatus == OrderStatus.Cancelled)
            {
                foreach (var item in order.OrderItems)
                {
                    if (item.VariantId.HasValue)
                    {
                        var variant = await _context.ProductVariants.FindAsync(item.VariantId.Value);
                        if (variant != null)
                        {
                            variant.StockQuantity += item.Quantity;
                            _context.ProductVariants.Update(variant);
                        }
                    }
                }
            }

            order.Status = targetStatus;
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();

            var updatedOrder = await _context.Orders
                .Include(o => o.ShippingAddress)
                .Include(o => o.Coupon)
                .Include(o => o.Payment)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Variant)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            return MapToDto(updatedOrder!);
        }

        private OrderDto MapToDto(Order order)
        {
            var dto = new OrderDto
            {
                Id = order.Id,
                UserId = order.UserId,
                OrderCode = order.OrderCode,
                ShippingAddressId = order.ShippingAddressId,
                Subtotal = order.Subtotal,
                ShippingFee = order.ShippingFee,
                DiscountAmount = order.DiscountAmount,
                TotalAmount = order.TotalAmount,
                Status = order.Status.ToString(),
                PaymentStatus = order.PaymentStatus.ToString(),
                Note = order.Note,
                CreatedAt = order.CreatedAt,
                CouponCode = order.Coupon?.Code,
                PaymentMethod = order.Payment != null ? order.Payment.Method.ToString() : string.Empty,
                ShippingAddress = order.ShippingAddress != null ? new OrderAddressDto
                {
                    Id = order.ShippingAddress.Id,
                    ReceiverName = order.ShippingAddress.ReceiverName,
                    Phone = order.ShippingAddress.Phone,
                    Province = order.ShippingAddress.Province,
                    District = order.ShippingAddress.District,
                    Ward = order.ShippingAddress.Ward,
                    DetailAddress = order.ShippingAddress.DetailAddress
                } : null,
                OrderItems = order.OrderItems.Select(oi => new OrderItemDto
                {
                    Id = oi.Id,
                    ProductId = oi.ProductId,
                    ProductName = oi.ProductName,
                    VariantId = oi.VariantId,
                    VariantName = oi.Variant?.VariantName,
                    UnitPrice = oi.UnitPrice,
                    Quantity = oi.Quantity,
                    TotalPrice = oi.TotalPrice,
                    PersonalizationNote = oi.PersonalizationNote
                }).ToList()
            };

            if (order.PaymentStatus == PaymentStatus.Unpaid && order.Payment != null && order.Payment.Method == PaymentMethod.BankTransfer)
            {
                dto.PaymentQrUrl = _paymentService.GetPaymentQrUrl(order.OrderCode, order.TotalAmount);
            }

            return dto;
        }
    }
}
