using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using EXE02_Backend_RE_CAFE.DTOs;
using EXE02_Backend_RE_CAFE.Interfaces;

namespace EXE02_Backend_RE_CAFE.Controllers
{
    [ApiController]
    [Route("api/admin/media")]
    [Authorize(Roles = "Admin")]
    public class AdminMediaController : BaseApiController
    {
        private readonly ICloudinaryService _cloudinaryService;

        public AdminMediaController(ICloudinaryService cloudinaryService)
        {
            _cloudinaryService = cloudinaryService;
        }

        [HttpPost("upload-image")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadImage([FromForm] UploadImageRequest request)
        {
            var folder = string.IsNullOrWhiteSpace(request.Folder) ? "recafe/products" : request.Folder.Trim();
            var (url, publicId) = await _cloudinaryService.UploadImageAsync(request.File, folder);

            var data = new UploadImageResponse
            {
                Url = url,
                PublicId = publicId
            };

            return Ok(SuccessResponse(
                message: "Image uploaded successfully.",
                action: "UploadImage",
                data: data,
                statusCode: StatusCodes.Status200OK));
        }
    }
}
