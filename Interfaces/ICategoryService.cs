using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EXE02_Backend_RE_CAFE.DTOs;

namespace EXE02_Backend_RE_CAFE.Interfaces
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryDto>> GetActiveCategoriesAsync();
        Task<IEnumerable<CategoryDto>> GetCategoriesForAdminAsync(bool? isActive = null);
        Task<CategoryDto?> GetCategoryByIdAsync(Guid id);
        Task<CategoryDto?> CreateCategoryAsync(CreateCategoryRequest request);
        Task<CategoryDto?> UpdateCategoryAsync(Guid id, UpdateCategoryRequest request);
        Task<bool> SoftDeleteCategoryAsync(Guid id);
    }
}
