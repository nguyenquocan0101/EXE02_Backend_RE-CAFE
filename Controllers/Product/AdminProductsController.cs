using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using EXE02_Backend_RE_CAFE.DTOs;
using EXE02_Backend_RE_CAFE.Exceptions;
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
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CreateProduct([FromForm] string payload, [FromForm] List<IFormFile>? imageUrls)
        {
            var request = DeserializePayload<CreateProductRequest>(payload, "CreateProduct");
            request.ImageUrls = imageUrls;
            if (!TryValidateModel(request))
            {
                return ValidationProblem(ModelState);
            }

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
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateProduct(Guid id, [FromForm] string payload, [FromForm] List<IFormFile>? imageUrls)
        {
            var request = DeserializePayload<UpdateProductRequest>(payload, "UpdateProduct");
            request.ImageUrls = imageUrls;
            if (!TryValidateModel(request))
            {
                return ValidationProblem(ModelState);
            }

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

        private static T DeserializePayload<T>(string payload, string action) where T : class
        {
            if (string.IsNullOrWhiteSpace(payload))
            {
                throw new BadRequestException($"'{action}' requires a non-empty 'payload' JSON field.");
            }

            try
            {
                var model = JsonSerializer.Deserialize<T>(payload, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return model ?? throw new BadRequestException($"'{action}' payload JSON is invalid.");
            }
            catch (JsonException)
            {
                throw new BadRequestException($"'{action}' payload must be valid JSON.");
            }
        }
    }
}
