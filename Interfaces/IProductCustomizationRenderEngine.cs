using System.Threading;
using System.Threading.Tasks;
using EXE02_Backend_RE_CAFE.Models;

namespace EXE02_Backend_RE_CAFE.Interfaces
{
    public interface IProductCustomizationRenderEngine
    {
        Task<(string ResultModelUrl, string ResultModelPublicId)> RenderAndUploadAsync(
            ProductCustomization customization,
            Product product,
            CancellationToken cancellationToken = default);
    }
}
