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
    [Route("api/admin/coffee-types")]
    [Authorize(Roles = "Admin")]
    public class AdminCoffeeTypesController : BaseApiController
    {
        private readonly IProductStoryService _productStoryService;

        public AdminCoffeeTypesController(IProductStoryService productStoryService)
        {
            _productStoryService = productStoryService;
        }

        [HttpGet]
        public async Task<IActionResult> GetCoffeeTypes()
        {
            return Ok(SuccessResponse(
                message: "Coffee types retrieved successfully.",
                action: "GetCoffeeTypes",
                data: await _productStoryService.GetCoffeeTypesAsync(),
                statusCode: StatusCodes.Status200OK));
        }

        [HttpPost]
        public async Task<IActionResult> CreateCoffeeType([FromBody] CreateCoffeeTypeRequest request)
        {
            var coffeeType = await _productStoryService.CreateCoffeeTypeAsync(request);
            return StatusCode(StatusCodes.Status201Created, SuccessResponse(
                message: "Coffee type created successfully.",
                action: "CreateCoffeeType",
                data: coffeeType,
                statusCode: StatusCodes.Status201Created));
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateCoffeeType(Guid id, [FromBody] UpdateCoffeeTypeRequest request)
        {
            return Ok(SuccessResponse(
                message: "Coffee type updated successfully.",
                action: "UpdateCoffeeType",
                data: await _productStoryService.UpdateCoffeeTypeAsync(id, request),
                statusCode: StatusCodes.Status200OK));
        }

        [HttpPatch("{id:guid}/active")]
        public async Task<IActionResult> SetCoffeeTypeActive(Guid id, [FromBody] SetCoffeeTypeActiveRequest request)
        {
            return Ok(SuccessResponse(
                message: "Coffee type status updated successfully.",
                action: "SetCoffeeTypeActive",
                data: await _productStoryService.SetCoffeeTypeActiveAsync(id, request.IsActive),
                statusCode: StatusCodes.Status200OK));
        }
    }
}
