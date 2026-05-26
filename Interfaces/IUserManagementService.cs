using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EXE02_Backend_RE_CAFE.DTOs;
using EXE02_Backend_RE_CAFE.Models;

namespace EXE02_Backend_RE_CAFE.Interfaces
{
    public interface IUserManagementService
    {
        Task<IEnumerable<AdminUserDto>> GetUsersAsync(UserRole? role, bool? isActive, string? keyword);
        Task<AdminUserDto?> GetUserByIdAsync(Guid userId);
        Task<AdminUserDto> UpdateUserAsync(Guid userId, AdminUpdateUserRequest request);
        Task<AdminUserDto> SetUserActiveAsync(Guid userId, bool isActive);
    }
}
