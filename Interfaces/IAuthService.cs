using System;
using System.Threading.Tasks;
using EXE02_Backend_RE_CAFE.DTOs;

namespace EXE02_Backend_RE_CAFE.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse?> RegisterAsync(RegisterRequest request);
        Task<AuthResponse?> LoginAsync(LoginRequest request);
        Task<UserResponse?> GetMeAsync(Guid userId);
        Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordRequest request);
    }
}
