using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using EXE02_Backend_RE_CAFE.DTOs;
using EXE02_Backend_RE_CAFE.Interfaces;

namespace EXE02_Backend_RE_CAFE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : BaseApiController
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var response = await _authService.RegisterAsync(request);
            if (response == null)
            {
                var error = ErrorResponse<object>(
                    message: "Registration failed. Username or email already exists.",
                    action: "Register",
                    statusCode: StatusCodes.Status409Conflict);

                return Conflict(error);
            }

            var success = SuccessResponse(
                message: "Registration successful. Account created and access token issued.",
                action: "Register",
                data: response,
                statusCode: StatusCodes.Status201Created);

            return StatusCode(StatusCodes.Status201Created, success);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var response = await _authService.LoginAsync(request);
            if (response == null)
            {
                var error = ErrorResponse<object>(
                    message: "Login failed. Invalid username/email or password.",
                    action: "Login",
                    statusCode: StatusCodes.Status401Unauthorized);

                return Unauthorized(error);
            }

            var success = SuccessResponse(
                message: "Login successful. Access token issued.",
                action: "Login",
                data: response,
                statusCode: StatusCodes.Status200OK);

            return Ok(success);
        }

        [Authorize]
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            var success = SuccessResponse<object>(
                message: "Logout successful. Please remove token on client side.",
                action: "Logout",
                data: null,
                statusCode: StatusCodes.Status200OK);

            return Ok(success);
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                var error = ErrorResponse<object>(
                    message: "Unauthorized request. User identifier is missing or invalid.",
                    action: "GetMe",
                    statusCode: StatusCodes.Status401Unauthorized);

                return Unauthorized(error);
            }

            var user = await _authService.GetMeAsync(userId);
            if (user == null)
            {
                var notFound = ErrorResponse<object>(
                    message: "User profile not found.",
                    action: "GetMe",
                    statusCode: StatusCodes.Status404NotFound);

                return NotFound(notFound);
            }

            var success = SuccessResponse(
                message: "User profile retrieved successfully.",
                action: "GetMe",
                data: user,
                statusCode: StatusCodes.Status200OK);

            return Ok(success);
        }

        [Authorize]
        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                var error = ErrorResponse<object>(
                    message: "Unauthorized request. User identifier is missing or invalid.",
                    action: "ChangePassword",
                    statusCode: StatusCodes.Status401Unauthorized);

                return Unauthorized(error);
            }

            var result = await _authService.ChangePasswordAsync(userId, request);
            if (!result)
            {
                var badRequest = ErrorResponse<object>(
                    message: "Change password failed. Current password is incorrect or user does not exist.",
                    action: "ChangePassword",
                    statusCode: StatusCodes.Status400BadRequest);

                return BadRequest(badRequest);
            }

            var success = SuccessResponse<object>(
                message: "Password changed successfully.",
                action: "ChangePassword",
                data: null,
                statusCode: StatusCodes.Status200OK);

            return Ok(success);
        }
    }
}
