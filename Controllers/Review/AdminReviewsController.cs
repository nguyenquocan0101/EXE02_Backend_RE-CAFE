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
    [Route("api/admin/reviews")]
    [Authorize(Roles = "Admin")]
    public class AdminReviewsController : BaseApiController
    {
        private readonly IReviewService _reviewService;

        public AdminReviewsController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        [HttpGet]
        public async Task<IActionResult> GetReviews([FromQuery] AdminReviewQueryParameters parameters)
        {
            var reviews = await _reviewService.GetAdminReviewsAsync(parameters);
            return Ok(SuccessResponse(
                message: "Reviews retrieved successfully.",
                action: "GetAdminReviews",
                data: reviews,
                statusCode: StatusCodes.Status200OK));
        }

        [HttpPatch("{reviewId}/visibility")]
        public async Task<IActionResult> SetVisibility(Guid reviewId, [FromBody] UpdateReviewVisibilityRequest request)
        {
            var review = await _reviewService.SetReviewVisibilityAsync(reviewId, request);
            return Ok(SuccessResponse(
                message: "Review visibility updated successfully.",
                action: "SetReviewVisibility",
                data: review,
                statusCode: StatusCodes.Status200OK));
        }
    }
}
