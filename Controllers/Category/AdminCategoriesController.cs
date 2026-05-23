using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using EXE02_Backend_RE_CAFE.DTOs;
using EXE02_Backend_RE_CAFE.Interfaces;

namespace EXE02_Backend_RE_CAFE.Controllers
{
    [ApiController]
    [Route("api/admin/categories")]
    [Authorize(Roles = "Admin")]
    public class AdminCategoriesController : BaseApiController
    {
        private readonly ICategoryService _categoryService;

        public AdminCategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequest request)
        {
            var category = await _categoryService.CreateCategoryAsync(request);
            if (category == null)
            {
                return BadRequest(ErrorResponse<object>(
                    message: "Failed to create category.",
                    action: "CreateCategory",
                    statusCode: StatusCodes.Status400BadRequest));
            }
            return StatusCode(StatusCodes.Status201Created, SuccessResponse(
                message: "Category created successfully.",
                action: "CreateCategory",
                data: category,
                statusCode: StatusCodes.Status201Created));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] UpdateCategoryRequest request)
        {
            var category = await _categoryService.UpdateCategoryAsync(id, request);
            if (category == null)
            {
                return BadRequest(ErrorResponse<object>(
                    message: "Failed to update category.",
                    action: "UpdateCategory",
                    statusCode: StatusCodes.Status400BadRequest));
            }
            return Ok(SuccessResponse(
                message: "Category updated successfully.",
                action: "UpdateCategory",
                data: category,
                statusCode: StatusCodes.Status200OK));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(Guid id)
        {
            var result = await _categoryService.SoftDeleteCategoryAsync(id);
            if (!result)
            {
                return BadRequest(ErrorResponse<object>(
                    message: "Failed to delete category.",
                    action: "DeleteCategory",
                    statusCode: StatusCodes.Status400BadRequest));
            }
            return Ok(SuccessResponse<object>(
                message: "Category soft-deleted successfully.",
                action: "DeleteCategory",
                data: null,
                statusCode: StatusCodes.Status200OK));
        }
    }
}
