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
            if (request.File == null || request.File.Length == 0)
            {
                return BadRequest(ErrorResponse<object>(
                    message: "File is required.",
                    action: "UploadImage",
                    statusCode: StatusCodes.Status400BadRequest));
            }

            var folder = string.IsNullOrWhiteSpace(request.Folder) ? "recafe/products" : request.Folder.Trim();
            
            string url;
            string publicId;
            bool isVideo = request.File.ContentType.StartsWith("video/", System.StringComparison.OrdinalIgnoreCase);
            bool isImage = request.File.ContentType.StartsWith("image/", System.StringComparison.OrdinalIgnoreCase);

            if (isImage)
            {
                (url, publicId) = await _cloudinaryService.UploadImageAsync(request.File, folder);
            }
            else if (isVideo)
            {
                (url, publicId) = await _cloudinaryService.UploadVideoAsync(request.File, folder);
            }
            else
            {
                return BadRequest(ErrorResponse<object>(
                    message: "Only image or video files are allowed.",
                    action: "UploadImage",
                    statusCode: StatusCodes.Status400BadRequest));
            }

            var data = new UploadImageResponse
            {
                Url = url,
                PublicId = publicId
            };

            return Ok(SuccessResponse(
                message: isVideo ? "Video uploaded successfully." : "Image uploaded successfully.",
                action: "UploadImage",
                data: data,
                statusCode: StatusCodes.Status200OK));
        }
    }
}
