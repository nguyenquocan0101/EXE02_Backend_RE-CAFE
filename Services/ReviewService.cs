using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using EXE02_Backend_RE_CAFE.Data;
using EXE02_Backend_RE_CAFE.DTOs;
using EXE02_Backend_RE_CAFE.Exceptions;
using EXE02_Backend_RE_CAFE.Interfaces;
using EXE02_Backend_RE_CAFE.Models;

namespace EXE02_Backend_RE_CAFE.Services
{
    public class ReviewService : IReviewService
    {
        private const int MaxImages = 2;
        private const int MaxVideos = 1;
        private const int MaxFiles = 3;
        private const long MaxImageBytes = 10L * 1024 * 1024;
        private const long MaxVideoBytes = 50L * 1024 * 1024;
        private const string ReviewFolder = "recafe/reviews";

        private static readonly HashSet<string> AllowedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".png", ".webp"
        };

        private static readonly HashSet<string> AllowedImageContentTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg", "image/png", "image/webp"
        };

        private static readonly HashSet<string> AllowedVideoExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".mp4", ".webm"
        };

        private static readonly HashSet<string> AllowedVideoContentTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "video/mp4", "video/webm"
        };

        private readonly ApplicationDbContext _context;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly ILogger<ReviewService> _logger;

        public ReviewService(
            ApplicationDbContext context,
            ICloudinaryService cloudinaryService,
            ILogger<ReviewService> logger)
        {
            _context = context;
            _cloudinaryService = cloudinaryService;
            _logger = logger;
        }

        public async Task<ReviewDto> CreateReviewAsync(Guid userId, CreateReviewRequest request)
        {
            ValidateRequest(request);

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == request.OrderId && o.UserId == userId);

            if (order == null)
            {
                throw new NotFoundException("Order not found.");
            }

            if (order.Status != OrderStatus.Completed)
            {
                throw new BadRequestException("Only completed orders can be reviewed.");
            }

            if (!order.OrderItems.Any(item => item.ProductId == request.ProductId))
            {
                throw new BadRequestException("The product was not included in this order.");
            }

            var duplicateExists = await _context.Reviews.AnyAsync(review =>
                review.UserId == userId &&
                review.OrderId == request.OrderId &&
                review.ProductId == request.ProductId);

            if (duplicateExists)
            {
                throw new BadRequestException("You have already reviewed this product in this order. Delete the existing review to create a new one.");
            }

            var review = new Review
            {
                UserId = userId,
                ProductId = request.ProductId,
                OrderId = request.OrderId,
                Rating = request.Rating,
                Comment = string.IsNullOrWhiteSpace(request.Comment) ? null : request.Comment.Trim(),
                IsVisible = true
            };

            var uploadedMedia = new List<(string PublicId, string MediaType)>();

            try
            {
                foreach (var file in request.Files)
                {
                    var mediaType = GetMediaType(file);
                    var uploaded = mediaType == "video"
                        ? await _cloudinaryService.UploadVideoAsync(file, $"{ReviewFolder}/{request.ProductId}")
                        : await _cloudinaryService.UploadImageAsync(file, $"{ReviewFolder}/{request.ProductId}");

                    review.Media.Add(new ReviewMedia
                    {
                        Url = uploaded.Url,
                        PublicId = uploaded.PublicId,
                        MediaType = mediaType
                    });
                    uploadedMedia.Add((uploaded.PublicId, mediaType));
                }

                _context.Reviews.Add(review);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException exception)
            {
                await CleanupUploadedMediaAsync(uploadedMedia, review.Id);
                _logger.LogWarning(exception, "Review persistence failed for order {OrderId}, product {ProductId}.", request.OrderId, request.ProductId);
                throw new BadRequestException("A review for this product and order already exists or could not be saved.");
            }
            catch
            {
                await CleanupUploadedMediaAsync(uploadedMedia, review.Id);
                throw;
            }

            var createdReview = await _context.Reviews
                .AsNoTracking()
                .Include(r => r.User)
                .Include(r => r.Product)
                .Include(r => r.Order)
                .Include(r => r.Media)
                .FirstAsync(r => r.Id == review.Id);

            return MapToDto(createdReview);
        }

        public async Task<ReviewPageDto> GetProductReviewsAsync(Guid productId, ReviewQueryParameters parameters)
        {
            if (!await _context.Products.AnyAsync(product => product.Id == productId))
            {
                throw new NotFoundException("Product not found.");
            }

            var page = Math.Max(1, parameters.Page);
            var pageSize = Math.Clamp(parameters.PageSize, 1, 50);
            var query = VisibleProductReviews(productId);

            if (parameters.Rating.HasValue)
            {
                query = query.Where(review => review.Rating == parameters.Rating.Value);
            }

            if (parameters.WithMedia == true)
            {
                query = query.Where(review => review.Media.Any());
            }

            var totalReviews = await query.CountAsync();
            var averageRating = await query
                .Select(review => (double?)review.Rating)
                .AverageAsync() ?? 0;
            var ratingDistribution = await query
                .GroupBy(review => review.Rating)
                .Select(group => new { Rating = group.Key, Count = group.Count() })
                .ToDictionaryAsync(item => item.Rating, item => item.Count);

            var reviews = await query
                .Include(review => review.User)
                .Include(review => review.Product)
                .Include(review => review.Order)
                .Include(review => review.Media)
                .OrderByDescending(review => review.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            return new ReviewPageDto
            {
                ProductId = productId,
                AverageRating = Math.Round(averageRating, 2),
                TotalReviews = totalReviews,
                RatingDistribution = ratingDistribution,
                Page = page,
                PageSize = pageSize,
                TotalPages = totalReviews == 0 ? 0 : (int)Math.Ceiling(totalReviews / (double)pageSize),
                Reviews = reviews.Select(MapToDto).ToList()
            };
        }

        public async Task<ReviewDto> GetMyReviewAsync(Guid userId, Guid reviewId)
        {
            var review = await _context.Reviews
                .AsNoTracking()
                .Include(item => item.User)
                .Include(item => item.Product)
                .Include(item => item.Order)
                .Include(item => item.Media)
                .FirstOrDefaultAsync(item => item.Id == reviewId && item.UserId == userId);

            if (review == null)
            {
                throw new NotFoundException("Review not found.");
            }

            return MapToDto(review);
        }

        public async Task DeleteReviewAsync(Guid userId, Guid reviewId)
        {
            var review = await _context.Reviews
                .Include(item => item.Media)
                .FirstOrDefaultAsync(item => item.Id == reviewId && item.UserId == userId);

            if (review == null)
            {
                throw new NotFoundException("Review not found.");
            }

            var mediaToDelete = review.Media
                .Select(media => (media.PublicId, media.MediaType))
                .ToList();

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();

            foreach (var media in mediaToDelete)
            {
                try
                {
                    var deleted = await _cloudinaryService.DeleteAsync(media.PublicId, media.MediaType);
                    if (!deleted)
                    {
                        _logger.LogWarning("Cloudinary cleanup returned false for review {ReviewId}, public ID {PublicId}.", reviewId, media.PublicId);
                    }
                }
                catch (Exception exception)
                {
                    _logger.LogWarning(exception, "Cloudinary cleanup failed for review {ReviewId}, public ID {PublicId}.", reviewId, media.PublicId);
                }
            }
        }

        public async Task<AdminReviewPageDto> GetAdminReviewsAsync(AdminReviewQueryParameters parameters)
        {
            var page = Math.Max(1, parameters.Page);
            var pageSize = Math.Clamp(parameters.PageSize, 1, 100);
            var query = _context.Reviews.AsQueryable();

            if (parameters.IsVisible.HasValue)
            {
                query = query.Where(review => review.IsVisible == parameters.IsVisible.Value);
            }

            if (parameters.ProductId.HasValue)
            {
                query = query.Where(review => review.ProductId == parameters.ProductId.Value);
            }

            if (!string.IsNullOrWhiteSpace(parameters.ProductKeyword))
            {
                var keyword = parameters.ProductKeyword.Trim();
                query = query.Where(review => review.Product != null && review.Product.Name.Contains(keyword));
            }

            if (parameters.Rating.HasValue)
            {
                query = query.Where(review => review.Rating == parameters.Rating.Value);
            }

            var totalReviews = await query.CountAsync();
            var reviews = await query
                .Include(review => review.User)
                .Include(review => review.Product)
                .Include(review => review.Order)
                .Include(review => review.Media)
                .OrderByDescending(review => review.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            return new AdminReviewPageDto
            {
                Page = page,
                PageSize = pageSize,
                TotalReviews = totalReviews,
                TotalPages = totalReviews == 0 ? 0 : (int)Math.Ceiling(totalReviews / (double)pageSize),
                Reviews = reviews.Select(MapToAdminDto).ToList()
            };
        }

        public async Task<AdminReviewDto> SetReviewVisibilityAsync(Guid reviewId, UpdateReviewVisibilityRequest request)
        {
            var review = await _context.Reviews
                .Include(item => item.User)
                .Include(item => item.Product)
                .Include(item => item.Order)
                .Include(item => item.Media)
                .FirstOrDefaultAsync(item => item.Id == reviewId);

            if (review == null)
            {
                throw new NotFoundException("Review not found.");
            }

            review.IsVisible = request.IsVisible;
            await _context.SaveChangesAsync();
            return MapToAdminDto(review);
        }

        private IQueryable<Review> VisibleProductReviews(Guid productId)
        {
            return _context.Reviews
                .Where(review => review.ProductId == productId && review.IsVisible);
        }

        private static void ValidateRequest(CreateReviewRequest request)
        {
            if (request.OrderId == Guid.Empty || request.ProductId == Guid.Empty)
            {
                throw new BadRequestException("OrderId and ProductId are required.");
            }

            if (request.Rating is < 1 or > 5)
            {
                throw new BadRequestException("Rating must be between 1 and 5.");
            }

            if (request.Comment?.Length > 1000)
            {
                throw new BadRequestException("Comment cannot exceed 1,000 characters.");
            }

            if (request.Files.Count > MaxFiles)
            {
                throw new BadRequestException("A review can contain at most 3 media files.");
            }

            var imageCount = 0;
            var videoCount = 0;
            foreach (var file in request.Files)
            {
                var mediaType = GetMediaType(file);
                if (mediaType == "image") imageCount++;
                if (mediaType == "video") videoCount++;
            }

            if (imageCount > MaxImages)
            {
                throw new BadRequestException("A review can contain at most 2 images.");
            }

            if (videoCount > MaxVideos)
            {
                throw new BadRequestException("A review can contain at most 1 video.");
            }
        }

        private static string GetMediaType(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new BadRequestException("Media files cannot be empty.");
            }

            var extension = Path.GetExtension(file.FileName);
            var contentType = file.ContentType.Trim();

            if (AllowedImageExtensions.Contains(extension) && AllowedImageContentTypes.Contains(contentType))
            {
                if (file.Length > MaxImageBytes)
                {
                    throw new BadRequestException("Each image must be 10 MB or smaller.");
                }

                return "image";
            }

            if (AllowedVideoExtensions.Contains(extension) && AllowedVideoContentTypes.Contains(contentType))
            {
                if (file.Length > MaxVideoBytes)
                {
                    throw new BadRequestException("Each video must be 50 MB or smaller.");
                }

                return "video";
            }

            throw new BadRequestException("Supported media: JPG, JPEG, PNG, WEBP images and MP4, WEBM videos.");
        }

        private async Task CleanupUploadedMediaAsync(IEnumerable<(string PublicId, string MediaType)> media, Guid reviewId)
        {
            foreach (var item in media)
            {
                try
                {
                    var deleted = await _cloudinaryService.DeleteAsync(item.PublicId, item.MediaType);
                    if (!deleted)
                    {
                        _logger.LogWarning("Cloudinary rollback returned false for review {ReviewId}, public ID {PublicId}.", reviewId, item.PublicId);
                    }
                }
                catch (Exception exception)
                {
                    _logger.LogWarning(exception, "Cloudinary rollback failed for review {ReviewId}, public ID {PublicId}.", reviewId, item.PublicId);
                }
            }
        }

        private static ReviewDto MapToDto(Review review)
        {
            return new ReviewDto
            {
                Id = review.Id,
                ProductId = review.ProductId,
                ProductName = review.Product?.Name ?? string.Empty,
                ReviewerName = review.User?.FullName ?? review.User?.Username ?? "Customer",
                Rating = review.Rating,
                Comment = review.Comment,
                IsVerifiedPurchase = review.Order?.Status == OrderStatus.Completed,
                CreatedAt = review.CreatedAt,
                Media = review.Media
                    .OrderBy(media => media.CreatedAt)
                    .Select(media => new ReviewMediaDto
                    {
                        Id = media.Id,
                        Url = media.Url,
                        MediaType = media.MediaType
                    })
                    .ToList()
            };
        }

        private static AdminReviewDto MapToAdminDto(Review review)
        {
            return new AdminReviewDto
            {
                Id = review.Id,
                UserId = review.UserId,
                ProductId = review.ProductId,
                OrderId = review.OrderId,
                ProductName = review.Product?.Name ?? string.Empty,
                ReviewerName = review.User?.FullName ?? review.User?.Username ?? "Customer",
                Rating = review.Rating,
                Comment = review.Comment,
                IsVisible = review.IsVisible,
                IsVerifiedPurchase = review.Order?.Status == OrderStatus.Completed,
                CreatedAt = review.CreatedAt,
                Media = review.Media
                    .OrderBy(media => media.CreatedAt)
                    .Select(media => new ReviewMediaDto
                    {
                        Id = media.Id,
                        Url = media.Url,
                        MediaType = media.MediaType
                    })
                    .ToList()
            };
        }
    }
}
