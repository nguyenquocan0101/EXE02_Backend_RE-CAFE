using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EXE02_Backend_RE_CAFE.DTOs
{
    public class CreateProductStoryRequest
    {
        [Required]
        public Guid ProductId { get; set; }

        [Required]
        public Guid CoffeeTypeId { get; set; }

        [Required]
        [StringLength(50000)]
        public string ContentHtmlVi { get; set; } = string.Empty;

        [Required]
        [StringLength(50000)]
        public string ContentHtmlEn { get; set; } = string.Empty;
    }

    public class UpdateProductStoryRequest
    {
        [Required]
        [StringLength(50000)]
        public string ContentHtmlVi { get; set; } = string.Empty;

        [Required]
        [StringLength(50000)]
        public string ContentHtmlEn { get; set; } = string.Empty;
    }

    public class SetProductStoryPublicationRequest
    {
        public bool IsPublished { get; set; }
    }

    public class ProductStoryQueryParameters
    {
        [Range(1, int.MaxValue)]
        public int Page { get; set; } = 1;

        [Range(1, int.MaxValue)]
        public int PageSize { get; set; } = 20;

        public string? Keyword { get; set; }
        public Guid? ProductId { get; set; }
        public Guid? CoffeeTypeId { get; set; }
        public bool? IsPublished { get; set; }
    }

    public class CoffeeTypeDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }
    }

    public class CreateCoffeeTypeRequest
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(80)]
        public string Slug { get; set; } = string.Empty;

        [Range(0, int.MaxValue)]
        public int DisplayOrder { get; set; }
    }

    public class UpdateCoffeeTypeRequest : CreateCoffeeTypeRequest
    {
    }

    public class SetCoffeeTypeActiveRequest
    {
        public bool IsActive { get; set; }
    }

    public class ProductStoryPublicDto
    {
        public string Slug { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string ProductSlug { get; set; } = string.Empty;
        public string CoffeeTypeName { get; set; } = string.Empty;
        public string CoffeeTypeSlug { get; set; } = string.Empty;
        public string ContentHtmlVi { get; set; } = string.Empty;
        public string ContentHtmlEn { get; set; } = string.Empty;
        public string LandingPageUrl { get; set; } = string.Empty;
        public DateTime? UpdatedAt { get; set; }
    }

    public class ProductStoryAdminDto : ProductStoryPublicDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public Guid CoffeeTypeId { get; set; }
        public bool IsPublished { get; set; }
        public DateTime CreatedAt { get; set; }
        public int SharedQrCount { get; set; }
    }

    public class ProductStoryAdminPageDto
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalStories { get; set; }
        public int TotalPages { get; set; }
        public List<ProductStoryAdminDto> Stories { get; set; } = new();
    }
}
