using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EXE02_Backend_RE_CAFE.DTOs;

namespace EXE02_Backend_RE_CAFE.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<ProductListDto>> GetActiveProductsAsync();
        Task<ProductDetailDto?> GetProductByIdAsync(Guid id);
        Task<ProductDetailDto?> CreateProductAsync(CreateProductRequest request);
        Task<ProductDetailDto?> UpdateProductAsync(Guid id, UpdateProductRequest request);
        Task<ProductDetailDto?> GetProductBySlugAsync(string slug);
        Task<bool> SoftDeleteProductAsync(Guid id);
    }
}
