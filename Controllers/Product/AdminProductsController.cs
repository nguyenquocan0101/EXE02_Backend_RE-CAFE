using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EXE02_Backend_RE_CAFE.DTOs;
using EXE02_Backend_RE_CAFE.Interfaces;

namespace EXE02_Backend_RE_CAFE.Controllers
{
    [ApiController]
    [Route("api/admin/products")]
    [Authorize(Roles = "Admin")]
    public class AdminProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public AdminProductsController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request)
        {
            var product = await _productService.CreateProductAsync(request);
            if (product == null)
            {
                return BadRequest(new { message = "Failed to create product." });
            }
            return StatusCode(201, product);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] UpdateProductRequest request)
        {
            var product = await _productService.UpdateProductAsync(id, request);
            if (product == null)
            {
                return BadRequest(new { message = "Failed to update product." });
            }
            return Ok(product);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(Guid id)
        {
            var result = await _productService.SoftDeleteProductAsync(id);
            if (!result)
            {
                return BadRequest(new { message = "Failed to delete product." });
            }
            return Ok(new { message = "Product soft-deleted successfully." });
        }
    }
}
