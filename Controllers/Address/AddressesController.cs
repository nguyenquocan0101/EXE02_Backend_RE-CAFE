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
    [Route("api/addresses")]
    [Authorize]
    public class AddressesController : BaseApiController
    {
        private readonly IAddressService _addressService;

        public AddressesController(IAddressService addressService)
        {
            _addressService = addressService;
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

        [HttpGet]
        public async Task<IActionResult> GetMyAddresses()
        {
            var userId = GetUserId();
            var addresses = await _addressService.GetMyAddressesAsync(userId);
            return Ok(SuccessResponse(
                message: "Addresses retrieved successfully.",
                action: "GetMyAddresses",
                data: addresses,
                statusCode: StatusCodes.Status200OK));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetMyAddressById(Guid id)
        {
            var userId = GetUserId();
            var address = await _addressService.GetMyAddressByIdAsync(userId, id);
            if (address == null)
            {
                return NotFound(ErrorResponse<object>(
                    message: $"Address with ID {id} not found.",
                    action: "GetMyAddressById",
                    statusCode: StatusCodes.Status404NotFound));
            }

            return Ok(SuccessResponse(
                message: "Address retrieved successfully.",
                action: "GetMyAddressById",
                data: address,
                statusCode: StatusCodes.Status200OK));
        }

        [HttpPost]
        [Consumes("application/json")]
        public async Task<IActionResult> CreateAddress([FromBody] CreateAddressRequest request)
        {
            var userId = GetUserId();
            var address = await _addressService.CreateAddressAsync(userId, request);

            return StatusCode(StatusCodes.Status201Created, SuccessResponse(
                message: "Address created successfully.",
                action: "CreateAddress",
                data: address,
                statusCode: StatusCodes.Status201Created));
        }

        [HttpPut("{id}")]
        [Consumes("application/json")]
        public async Task<IActionResult> UpdateAddress(Guid id, [FromBody] UpdateAddressRequest request)
        {
            var userId = GetUserId();
            var address = await _addressService.UpdateAddressAsync(userId, id, request);

            return Ok(SuccessResponse(
                message: "Address updated successfully.",
                action: "UpdateAddress",
                data: address,
                statusCode: StatusCodes.Status200OK));
        }

        [HttpPut("{id}/set-default")]
        public async Task<IActionResult> SetDefaultAddress(Guid id)
        {
            var userId = GetUserId();
            var address = await _addressService.SetDefaultAddressAsync(userId, id);

            return Ok(SuccessResponse(
                message: "Default address updated successfully.",
                action: "SetDefaultAddress",
                data: address,
                statusCode: StatusCodes.Status200OK));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAddress(Guid id)
        {
            var userId = GetUserId();
            await _addressService.DeleteAddressAsync(userId, id);

            return Ok(SuccessResponse<object>(
                message: "Address deleted successfully.",
                action: "DeleteAddress",
                data: null,
                statusCode: StatusCodes.Status200OK));
        }
    }
}
