using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace EXE02_Backend_RE_CAFE.DTOs
{
    public class CreateReviewRequest
    {
        [Required]
        public Guid OrderId { get; set; }

        [Required]
        public Guid ProductId { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        [StringLength(1000)]
        public string? Comment { get; set; }

        public List<IFormFile> Files { get; set; } = new();
    }

    public class ReviewMediaDto
    {
        public Guid Id { get; set; }
        public string Url { get; set; } = string.Empty;
        public string MediaType { get; set; } = string.Empty;
    }

    public class ReviewDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ReviewerName { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public bool IsVerifiedPurchase { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<ReviewMediaDto> Media { get; set; } = new();
    }

    public class ReviewPageDto
    {
        public Guid ProductId { get; set; }
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public Dictionary<int, int> RatingDistribution { get; set; } = new();
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public List<ReviewDto> Reviews { get; set; } = new();
    }

    public class ReviewQueryParameters
    {
        [Range(1, int.MaxValue)]
        public int Page { get; set; } = 1;

        [Range(1, 50)]
        public int PageSize { get; set; } = 10;

        [Range(1, 5)]
        public int? Rating { get; set; }

        public bool? WithMedia { get; set; }
    }

    public class AdminReviewQueryParameters
    {
        [Range(1, int.MaxValue)]
        public int Page { get; set; } = 1;

        [Range(1, 100)]
        public int PageSize { get; set; } = 20;

        public bool? IsVisible { get; set; }
        public Guid? ProductId { get; set; }
        public string? ProductKeyword { get; set; }

        [Range(1, 5)]
        public int? Rating { get; set; }
    }

    public class AdminReviewDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid ProductId { get; set; }
        public Guid OrderId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ReviewerName { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public bool IsVisible { get; set; }
        public bool IsVerifiedPurchase { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<ReviewMediaDto> Media { get; set; } = new();
    }

    public class AdminReviewPageDto
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalReviews { get; set; }
        public int TotalPages { get; set; }
        public List<AdminReviewDto> Reviews { get; set; } = new();
    }

    public class UpdateReviewVisibilityRequest
    {
        public bool IsVisible { get; set; }
    }
}
