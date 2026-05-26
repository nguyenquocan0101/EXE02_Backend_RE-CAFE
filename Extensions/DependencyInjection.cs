using Microsoft.Extensions.DependencyInjection;
using EXE02_Backend_RE_CAFE.Interfaces;
using EXE02_Backend_RE_CAFE.Services;

namespace EXE02_Backend_RE_CAFE.Extensions
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<ICartService, CartService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IAddressService, AddressService>();
            services.AddScoped<IUserManagementService, UserManagementService>();
            services.AddScoped<ICloudinaryService, CloudinaryService>();
            services.AddScoped<IPaymentService, PaymentService>();
            
            return services;
        }
    }
}
