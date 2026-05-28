using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using EXE02_Backend_RE_CAFE.DTOs;
using EXE02_Backend_RE_CAFE.Exceptions;
using EXE02_Backend_RE_CAFE.Interfaces;

namespace EXE02_Backend_RE_CAFE.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/products/{productId:guid}/customizations")]
    public class ProductCustomizationsController : BaseApiController
    {
        private readonly IProductCustomizationService _customizationService;

        public ProductCustomizationsController(IProductCustomizationService customizationService)
        {
            _customizationService = customizationService;
        }

        private Guid GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                throw new UnauthorizedException("Unauthorized request.");
            }

            return userId;
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CreateCustomization(Guid productId, [FromForm] CreateProductCustomizationRequest request)
        {
            var userId = GetUserId();
            var customization = await _customizationService.CreateCustomizationAsync(userId, productId, request);

            return StatusCode(StatusCodes.Status201Created, SuccessResponse(
                message: "Product customization created successfully.",
                action: "CreateProductCustomization",
                data: customization,
                statusCode: StatusCodes.Status201Created));
        }

        [HttpGet]
        public async Task<IActionResult> GetMyCustomizationsByProduct(Guid productId)
        {
            var userId = GetUserId();
            var customizations = await _customizationService.GetMyCustomizationsByProductAsync(userId, productId);

            return Ok(SuccessResponse(
                message: "Product customizations retrieved successfully.",
                action: "GetMyProductCustomizations",
                data: customizations,
                statusCode: StatusCodes.Status200OK));
        }

        [HttpGet("{customizationId:guid}")]
        public async Task<IActionResult> GetMyCustomizationById(Guid productId, Guid customizationId)
        {
            var userId = GetUserId();
            var customization = await _customizationService.GetMyCustomizationByIdAsync(userId, productId, customizationId);

            if (customization == null)
            {
                return NotFound(ErrorResponse<object>(
                    message: $"Customization with ID {customizationId} not found.",
                    action: "GetMyProductCustomizationById",
                    statusCode: StatusCodes.Status404NotFound));
            }

            return Ok(SuccessResponse(
                message: "Product customization retrieved successfully.",
                action: "GetMyProductCustomizationById",
                data: customization,
                statusCode: StatusCodes.Status200OK));
        }
    }
}
