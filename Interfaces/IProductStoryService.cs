using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EXE02_Backend_RE_CAFE.DTOs;

namespace EXE02_Backend_RE_CAFE.Interfaces
{
    public interface IProductStoryService
    {
        Task<ProductStoryPublicDto?> GetPublishedBySlugAsync(string slug);
        Task<ProductStoryAdminPageDto> GetAdminStoriesAsync(ProductStoryQueryParameters parameters);
        Task<ProductStoryAdminDto?> GetAdminByIdAsync(Guid id);
        Task<ProductStoryAdminDto> CreateAsync(CreateProductStoryRequest request);
        Task<ProductStoryAdminDto> UpdateAsync(Guid id, UpdateProductStoryRequest request);
        Task<ProductStoryAdminDto> SetPublicationAsync(Guid id, bool isPublished);
        Task<IReadOnlyList<CoffeeTypeDto>> GetActiveCoffeeTypesAsync();
    }
}
