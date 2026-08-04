using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using EXE02_Backend_RE_CAFE.DTOs;
using EXE02_Backend_RE_CAFE.Interfaces;

namespace EXE02_Backend_RE_CAFE.Controllers.Product
{
    [ApiController]
    [Route("api/admin/product-stories")]
    [Authorize(Roles = "Admin")]
    public class AdminProductStoriesController : BaseApiController
    {
        private readonly IProductStoryService _productStoryService;

        public AdminProductStoriesController(IProductStoryService productStoryService)
        {
            _productStoryService = productStoryService;
        }

        [HttpGet]
        public async Task<IActionResult> GetStories([FromQuery] ProductStoryQueryParameters parameters)
        {
            var stories = await _productStoryService.GetAdminStoriesAsync(parameters);
            return Ok(SuccessResponse(
                message: "Product stories retrieved successfully.",
                action: "GetAdminProductStories",
                data: stories,
                statusCode: StatusCodes.Status200OK));
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetStory(Guid id)
        {
            var story = await _productStoryService.GetAdminByIdAsync(id);
            if (story == null)
            {
                return NotFound(ErrorResponse<object>(
                    message: "Traceability page not found.",
                    action: "GetAdminProductStory",
                    statusCode: StatusCodes.Status404NotFound));
            }

            return Ok(SuccessResponse(
                message: "Product story retrieved successfully.",
                action: "GetAdminProductStory",
                data: story,
                statusCode: StatusCodes.Status200OK));
        }

        [HttpPost]
        public async Task<IActionResult> CreateStory([FromBody] CreateProductStoryRequest request)
        {
            var story = await _productStoryService.CreateAsync(request);
            return StatusCode(StatusCodes.Status201Created, SuccessResponse(
                message: "Product story created successfully.",
                action: "CreateProductStory",
                data: story,
                statusCode: StatusCodes.Status201Created));
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateStory(Guid id, [FromBody] UpdateProductStoryRequest request)
        {
            var story = await _productStoryService.UpdateAsync(id, request);
            return Ok(SuccessResponse(
                message: "Product story updated successfully.",
                action: "UpdateProductStory",
                data: story,
                statusCode: StatusCodes.Status200OK));
        }

        [HttpPatch("{id:guid}/publication")]
        public async Task<IActionResult> SetPublication(Guid id, [FromBody] SetProductStoryPublicationRequest request)
        {
            var story = await _productStoryService.SetPublicationAsync(id, request.IsPublished);
            return Ok(SuccessResponse(
                message: "Product story publication updated successfully.",
                action: "SetProductStoryPublication",
                data: story,
                statusCode: StatusCodes.Status200OK));
        }

        [HttpGet("coffee-types")]
        public async Task<IActionResult> GetCoffeeTypes()
        {
            var coffeeTypes = await _productStoryService.GetActiveCoffeeTypesAsync();
            return Ok(SuccessResponse(
                message: "Active coffee types retrieved successfully.",
                action: "GetActiveCoffeeTypes",
                data: coffeeTypes,
                statusCode: StatusCodes.Status200OK));
        }
    }
}
