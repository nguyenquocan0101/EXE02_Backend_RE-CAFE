using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using EXE02_Backend_RE_CAFE.Data;
using EXE02_Backend_RE_CAFE.DTOs;
using EXE02_Backend_RE_CAFE.Interfaces;
using EXE02_Backend_RE_CAFE.Models;
using BC = BCrypt.Net.BCrypt;

namespace EXE02_Backend_RE_CAFE.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<AuthResponse?> RegisterAsync(RegisterRequest request)
        {
            if (await _context.Users.AnyAsync(u => u.Username == request.Username || u.Email == request.Email))
            {
                return null; // User already exists
            }

            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = BC.HashPassword(request.Password),
                Role = request.Role,
                FullName = string.IsNullOrWhiteSpace(request.FullName) ? request.Username : request.FullName
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return new AuthResponse
            {
                Token = GenerateJwtToken(user),
                Username = user.Username,
                Email = user.Email,
                Role = user.Role.ToString()
            };
        }

        public async Task<AuthResponse?> LoginAsync(LoginRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => 
                u.Username == request.UsernameOrEmail || u.Email == request.UsernameOrEmail);

            if (user == null || !BC.Verify(request.Password, user.PasswordHash))
            {
                return null; // Invalid credentials
            }

            return new AuthResponse
            {
                Token = GenerateJwtToken(user),
                Username = user.Username,
                Email = user.Email,
                Role = user.Role.ToString()
            };
        }

        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(double.Parse(jwtSettings["DurationInMinutes"]!)),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<UserResponse?> GetMeAsync(Guid userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return null;
            }

            return new UserResponse
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                Phone = user.Phone,
                Role = user.Role.ToString(),
                TotalPoints = user.TotalPoints,
                Level = user.Level.ToString(),
                Birthday = user.Birthday,
                CreatedAt = user.CreatedAt
            };
        }

        public async Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return false;
            }

            if (!BC.Verify(request.CurrentPassword, user.PasswordHash))
            {
                return false; // Current password does not match
            }

            user.PasswordHash = BC.HashPassword(request.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
