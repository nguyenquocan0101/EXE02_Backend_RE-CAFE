using Microsoft.AspNetCore.Http;

namespace EXE02_Backend_RE_CAFE.DTOs
{
    public class UploadImageRequest
    {
        public IFormFile File { get; set; } = default!;
        public string? Folder { get; set; }
    }

    public class UploadImageResponse
    {
        public string Url { get; set; } = string.Empty;
        public string PublicId { get; set; } = string.Empty;
    }
}
