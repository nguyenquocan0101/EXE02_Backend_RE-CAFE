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
    [Route("api/admin/products")]
    [Authorize(Roles = "Admin")]
    public class AdminProductsController : BaseApiController
    {
        private readonly IProductService _productService;

        public AdminProductsController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts([FromQuery] bool? isActive = null)
        {
            var products = await _productService.GetProductsForAdminAsync(isActive);
            return Ok(SuccessResponse(
                message: "Admin products retrieved successfully.",
                action: "GetAdminProducts",
                data: products,
                statusCode: StatusCodes.Status200OK));
        }

        [HttpPost]
        [Consumes("application/json")]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request)
        {
            var product = await _productService.CreateProductAsync(request);
            if (product == null)
            {
                return BadRequest(ErrorResponse<object>(
                    message: "Failed to create product.",
                    action: "CreateProduct",
                    statusCode: StatusCodes.Status400BadRequest));
            }
            return StatusCode(StatusCodes.Status201Created, SuccessResponse(
                message: "Product created successfully.",
                action: "CreateProduct",
                data: product,
                statusCode: StatusCodes.Status201Created));
        }

        [HttpPut("{id}")]
        [Consumes("application/json")]
        public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] UpdateProductRequest request)
        {
            var product = await _productService.UpdateProductAsync(id, request);
            if (product == null)
            {
                return BadRequest(ErrorResponse<object>(
                    message: "Failed to update product.",
                    action: "UpdateProduct",
                    statusCode: StatusCodes.Status400BadRequest));
            }
            return Ok(SuccessResponse(
                message: "Product updated successfully.",
                action: "UpdateProduct",
                data: product,
                statusCode: StatusCodes.Status200OK));
        }

        [HttpPost("{id}/images")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadProductImages(Guid id, [FromForm] UploadProductImagesRequest request)
        {
            var product = await _productService.UploadProductImagesAsync(id, request);
            if (product == null)
            {
                return BadRequest(ErrorResponse<object>(
                    message: "Failed to upload product images.",
                    action: "UploadProductImages",
                    statusCode: StatusCodes.Status400BadRequest));
            }

            return Ok(SuccessResponse(
                message: "Product images uploaded successfully.",
                action: "UploadProductImages",
                data: product,
                statusCode: StatusCodes.Status200OK));
        }

        [HttpPost("{id}/model-3d")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadProductModel3D(Guid id, [FromForm] UploadProductModel3DRequest request)
        {
            var product = await _productService.UploadProductModel3DAsync(id, request);
            if (product == null)
            {
                return BadRequest(ErrorResponse<object>(
                    message: "Failed to upload product 3D model.",
                    action: "UploadProductModel3D",
                    statusCode: StatusCodes.Status400BadRequest));
            }

            return Ok(SuccessResponse(
                message: "Product 3D model uploaded successfully.",
                action: "UploadProductModel3D",
                data: product,
                statusCode: StatusCodes.Status200OK));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(Guid id)
        {
            var result = await _productService.SoftDeleteProductAsync(id);
            if (!result)
            {
                return BadRequest(ErrorResponse<object>(
                    message: "Failed to delete product.",
                    action: "DeleteProduct",
                    statusCode: StatusCodes.Status400BadRequest));
            }
            return Ok(SuccessResponse<object>(
                message: "Product soft-deleted successfully.",
                action: "DeleteProduct",
                data: null,
                statusCode: StatusCodes.Status200OK));
        }
    }
}
