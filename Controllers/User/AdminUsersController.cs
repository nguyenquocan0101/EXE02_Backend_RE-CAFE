using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using EXE02_Backend_RE_CAFE.DTOs;
using EXE02_Backend_RE_CAFE.Interfaces;
using EXE02_Backend_RE_CAFE.Models;

namespace EXE02_Backend_RE_CAFE.Controllers
{
    [ApiController]
    [Route("api/admin/users")]
    [Authorize(Roles = "Admin")]
    public class AdminUsersController : BaseApiController
    {
        private readonly IUserManagementService _userManagementService;

        public AdminUsersController(IUserManagementService userManagementService)
        {
            _userManagementService = userManagementService;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers([FromQuery] UserRole? role, [FromQuery] bool? isActive, [FromQuery] string? keyword)
        {
            var users = await _userManagementService.GetUsersAsync(role, isActive, keyword);
            return Ok(SuccessResponse(
                message: "Users retrieved successfully.",
                action: "GetUsers",
                data: users,
                statusCode: StatusCodes.Status200OK));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(Guid id)
        {
            var user = await _userManagementService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound(ErrorResponse<object>(
                    message: $"User with ID {id} not found.",
                    action: "GetUserById",
                    statusCode: StatusCodes.Status404NotFound));
            }

            return Ok(SuccessResponse(
                message: "User retrieved successfully.",
                action: "GetUserById",
                data: user,
                statusCode: StatusCodes.Status200OK));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(Guid id, [FromBody] AdminUpdateUserRequest request)
        {
            var user = await _userManagementService.UpdateUserAsync(id, request);
            return Ok(SuccessResponse(
                message: "User updated successfully.",
                action: "UpdateUser",
                data: user,
                statusCode: StatusCodes.Status200OK));
        }

        [HttpPut("{id}/active")]
        public async Task<IActionResult> SetUserActive(Guid id, [FromBody] AdminSetUserActiveRequest request)
        {
            var user = await _userManagementService.SetUserActiveAsync(id, request.IsActive);
            return Ok(SuccessResponse(
                message: "User active status updated successfully.",
                action: "SetUserActive",
                data: user,
                statusCode: StatusCodes.Status200OK));
        }
    }
}
