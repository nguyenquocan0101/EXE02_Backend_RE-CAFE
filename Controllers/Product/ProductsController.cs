using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using EXE02_Backend_RE_CAFE.Interfaces;

namespace EXE02_Backend_RE_CAFE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : BaseApiController
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            var products = await _productService.GetActiveProductsAsync();
            return Ok(SuccessResponse(
                message: "Products retrieved successfully.",
                action: "GetProducts",
                data: products,
                statusCode: StatusCodes.Status200OK));
        }

        [HttpGet("featured")]
        public async Task<IActionResult> GetFeaturedProducts()
        {
            var products = await _productService.GetFeaturedProductsAsync();
            return Ok(SuccessResponse(
                message: "Featured products retrieved successfully.",
                action: "GetFeaturedProducts",
                data: products,
                statusCode: StatusCodes.Status200OK));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById(Guid id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound(ErrorResponse<object>(
                    message: $"Product with ID {id} not found.",
                    action: "GetProductById",
                    statusCode: StatusCodes.Status404NotFound));
            }
            return Ok(SuccessResponse(
                message: "Product retrieved successfully.",
                action: "GetProductById",
                data: product,
                statusCode: StatusCodes.Status200OK));
        }

        [HttpGet("slug/{slug}")]
        public async Task<IActionResult> GetProductBySlug(string slug)
        {
            var product = await _productService.GetProductBySlugAsync(slug);
            if (product == null)
            {
                return NotFound(ErrorResponse<object>(
                    message: $"Product with slug '{slug}' not found.",
                    action: "GetProductBySlug",
                    statusCode: StatusCodes.Status404NotFound));
            }
            return Ok(SuccessResponse(
                message: "Product retrieved successfully.",
                action: "GetProductBySlug",
                data: product,
                statusCode: StatusCodes.Status200OK));
        }
    }
}
