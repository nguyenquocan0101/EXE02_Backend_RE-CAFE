using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EXE02_Backend_RE_CAFE.Data;
using EXE02_Backend_RE_CAFE.DTOs;
using EXE02_Backend_RE_CAFE.Exceptions;
using EXE02_Backend_RE_CAFE.Interfaces;
using EXE02_Backend_RE_CAFE.Models;

namespace EXE02_Backend_RE_CAFE.Services
{
    public class UserManagementService : IUserManagementService
    {
        private readonly ApplicationDbContext _context;

        public UserManagementService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AdminUserDto>> GetUsersAsync(UserRole? role, bool? isActive, string? keyword)
        {
            var query = _context.Users.AsQueryable();

            if (role.HasValue)
            {
                query = query.Where(u => u.Role == role.Value);
            }

            if (isActive.HasValue)
            {
                query = query.Where(u => u.IsActive == isActive.Value);
            }

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var normalizedKeyword = keyword.Trim().ToLower();
                query = query.Where(u =>
                    u.Username.ToLower().Contains(normalizedKeyword) ||
                    u.Email.ToLower().Contains(normalizedKeyword) ||
                    u.FullName.ToLower().Contains(normalizedKeyword) ||
                    (u.Phone != null && u.Phone.Contains(normalizedKeyword)));
            }

            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();

            return users.Select(MapToDto);
        }

        public async Task<AdminUserDto?> GetUserByIdAsync(Guid userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            return user == null ? null : MapToDto(user);
        }

        public async Task<AdminUserDto> UpdateUserAsync(Guid userId, AdminUpdateUserRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                throw new NotFoundException("User not found.");
            }

            var normalizedEmail = request.Email.Trim().ToLower();
            var emailExists = await _context.Users.AnyAsync(u => u.Id != userId && u.Email.ToLower() == normalizedEmail);
            if (emailExists)
            {
                throw new BadRequestException("Email already exists.");
            }

            user.Email = request.Email.Trim();
            user.FullName = request.FullName.Trim();
            user.Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim();
            user.Birthday = request.Birthday;
            user.Role = request.Role;
            user.IsActive = request.IsActive;
            user.UpdatedAt = DateTime.UtcNow;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return MapToDto(user);
        }

        public async Task<AdminUserDto> SetUserActiveAsync(Guid userId, bool isActive)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                throw new NotFoundException("User not found.");
            }

            user.IsActive = isActive;
            user.UpdatedAt = DateTime.UtcNow;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return MapToDto(user);
        }

        private static AdminUserDto MapToDto(User user)
        {
            return new AdminUserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                Phone = user.Phone,
                IsActive = user.IsActive,
                Role = user.Role.ToString(),
                TotalPoints = user.TotalPoints,
                Level = user.Level.ToString(),
                Birthday = user.Birthday,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };
        }
    }
}
