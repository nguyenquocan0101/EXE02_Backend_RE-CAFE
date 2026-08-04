using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using EXE02_Backend_RE_CAFE.DTOs;
using EXE02_Backend_RE_CAFE.Interfaces;

namespace EXE02_Backend_RE_CAFE.Controllers.Product
{
    [ApiController]
    [Route("api/product-stories")]
    public class ProductStoriesController : BaseApiController
    {
        private readonly IProductStoryService _productStoryService;

        public ProductStoriesController(IProductStoryService productStoryService)
        {
            _productStoryService = productStoryService;
        }

        [HttpGet("{slug}")]
        [ProducesResponseType(typeof(ApiResponse<ProductStoryPublicDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPublishedStory(string slug)
        {
            var story = await _productStoryService.GetPublishedBySlugAsync(slug);
            if (story == null)
            {
                return NotFound(ErrorResponse<object>(
                    message: "Product story not found.",
                    action: "GetPublishedProductStory",
                    statusCode: StatusCodes.Status404NotFound));
            }

            return Ok(SuccessResponse(
                message: "Product story retrieved successfully.",
                action: "GetPublishedProductStory",
                data: story,
                statusCode: StatusCodes.Status200OK));
        }

        [HttpPost("{slug}/open")]
        public async Task<IActionResult> RegisterPageOpen(string slug)
        {
            if (!await _productStoryService.RegisterPageOpenAsync(slug))
            {
                return NotFound(ErrorResponse<object>(
                    message: "Product story not found.",
                    action: "RegisterProductStoryOpen",
                    statusCode: StatusCodes.Status404NotFound));
            }

            return NoContent();
        }
    }
}
