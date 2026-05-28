using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EXE02_Backend_RE_CAFE.DTOs;

namespace EXE02_Backend_RE_CAFE.Interfaces
{
    public interface IProductCustomizationService
    {
        Task<ProductCustomizationDto> CreateCustomizationAsync(Guid userId, Guid productId, CreateProductCustomizationRequest request);
        Task<ProductCustomizationBootstrapDto> GetCustomizationBootstrapAsync(Guid userId, Guid productId);
        Task<IEnumerable<ProductCustomizationDto>> GetMyCustomizationsByProductAsync(Guid userId, Guid productId);
        Task<ProductCustomizationDto?> GetMyCustomizationByIdAsync(Guid userId, Guid productId, Guid customizationId);
    }
}
