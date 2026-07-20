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
    [Route("api/reviews")]
    public class ReviewsController : BaseApiController
    {
        private readonly IReviewService _reviewService;

        public ReviewsController(IReviewService reviewService)
        {
            _reviewService = reviewService;
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
        [Authorize]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(80_000_000)]
        public async Task<IActionResult> CreateReview([FromForm] CreateReviewRequest request)
        {
            var review = await _reviewService.CreateReviewAsync(GetUserId(), request);
            return StatusCode(StatusCodes.Status201Created, SuccessResponse(
                message: "Review created successfully.",
                action: "CreateReview",
                data: review,
                statusCode: StatusCodes.Status201Created));
        }

        [HttpGet("product/{productId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProductReviews(Guid productId, [FromQuery] ReviewQueryParameters parameters)
        {
            var reviews = await _reviewService.GetProductReviewsAsync(productId, parameters);
            return Ok(SuccessResponse(
                message: "Product reviews retrieved successfully.",
                action: "GetProductReviews",
                data: reviews,
                statusCode: StatusCodes.Status200OK));
        }

        [HttpGet("{reviewId}")]
        [Authorize]
        public async Task<IActionResult> GetMyReview(Guid reviewId)
        {
            var review = await _reviewService.GetMyReviewAsync(GetUserId(), reviewId);
            return Ok(SuccessResponse(
                message: "Review retrieved successfully.",
                action: "GetMyReview",
                data: review,
                statusCode: StatusCodes.Status200OK));
        }

        [HttpDelete("{reviewId}")]
        [Authorize]
        public async Task<IActionResult> DeleteReview(Guid reviewId)
        {
            await _reviewService.DeleteReviewAsync(GetUserId(), reviewId);
            return NoContent();
        }
    }
}
