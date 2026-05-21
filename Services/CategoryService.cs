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
    public class CategoryService : ICategoryService
    {
        private readonly ApplicationDbContext _context;

        public CategoryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CategoryDto>> GetActiveCategoriesAsync()
        {
            return await _context.Categories
                .Where(c => c.IsActive)
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Slug = c.Slug,
                    Description = c.Description,
                    IsActive = c.IsActive,
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<CategoryDto?> GetCategoryByIdAsync(Guid id)
        {
            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id && c.IsActive);
            if (category == null)
            {
                return null;
            }

            return MapToDto(category);
        }

        public async Task<CategoryDto?> CreateCategoryAsync(CreateCategoryRequest request)
        {
            if (await _context.Categories.AnyAsync(c => c.Slug == request.Slug))
            {
                throw new BadRequestException($"Category with Slug '{request.Slug}' already exists.");
            }

            var category = new Category
            {
                Name = request.Name,
                Slug = request.Slug,
                Description = request.Description,
                IsActive = request.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return MapToDto(category);
        }

        public async Task<CategoryDto?> UpdateCategoryAsync(Guid id, UpdateCategoryRequest request)
        {
            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
            if (category == null)
            {
                throw new NotFoundException($"Category with ID {id} not found.");
            }

            if (await _context.Categories.AnyAsync(c => c.Slug == request.Slug && c.Id != id))
            {
                throw new BadRequestException($"Category with Slug '{request.Slug}' already exists on another category.");
            }

            category.Name = request.Name;
            category.Slug = request.Slug;
            category.Description = request.Description;
            category.IsActive = request.IsActive;

            _context.Categories.Update(category);
            await _context.SaveChangesAsync();

            return MapToDto(category);
        }

        public async Task<bool> SoftDeleteCategoryAsync(Guid id)
        {
            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
            if (category == null)
            {
                throw new NotFoundException($"Category with ID {id} not found.");
            }

            category.IsActive = false;

            _context.Categories.Update(category);
            await _context.SaveChangesAsync();
            return true;
        }

        private CategoryDto MapToDto(Category c)
        {
            return new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Slug = c.Slug,
                Description = c.Description,
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt
            };
        }
    }
}
