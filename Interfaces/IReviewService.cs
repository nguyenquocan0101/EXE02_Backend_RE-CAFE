using System;
using System.Threading.Tasks;
using EXE02_Backend_RE_CAFE.DTOs;

namespace EXE02_Backend_RE_CAFE.Interfaces
{
    public interface IReviewService
    {
        Task<ReviewDto> CreateReviewAsync(Guid userId, CreateReviewRequest request);
        Task<ReviewDto> GetMyReviewAsync(Guid userId, Guid reviewId);
        Task<ReviewPageDto> GetProductReviewsAsync(Guid productId, ReviewQueryParameters parameters);
        Task DeleteReviewAsync(Guid userId, Guid reviewId);
        Task<AdminReviewPageDto> GetAdminReviewsAsync(AdminReviewQueryParameters parameters);
        Task<AdminReviewDto> SetReviewVisibilityAsync(Guid reviewId, UpdateReviewVisibilityRequest request);
    }
}
