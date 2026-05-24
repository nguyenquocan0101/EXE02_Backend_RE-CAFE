using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace EXE02_Backend_RE_CAFE.Interfaces
{
    public interface ICloudinaryService
    {
        Task<(string Url, string PublicId)> UploadImageAsync(IFormFile file, string folder);
    }
}
