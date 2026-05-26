using System;
using System.ComponentModel.DataAnnotations;
using EXE02_Backend_RE_CAFE.Models;

namespace EXE02_Backend_RE_CAFE.DTOs
{
    public class AdminUserDto
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public bool IsActive { get; set; }
        public string Role { get; set; } = string.Empty;
        public int TotalPoints { get; set; }
        public string Level { get; set; } = string.Empty;
        public DateTime? Birthday { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class AdminUpdateUserRequest
    {
        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [StringLength(20)]
        public string? Phone { get; set; }

        public DateTime? Birthday { get; set; }

        [Required]
        public UserRole Role { get; set; } = UserRole.Customer;

        public bool IsActive { get; set; } = true;
    }

    public class AdminSetUserActiveRequest
    {
        [Required]
        public bool IsActive { get; set; }
    }
}
