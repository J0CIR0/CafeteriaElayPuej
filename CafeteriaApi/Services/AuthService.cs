using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using CafeteriaApi.Data;
using CafeteriaApi.Models;
using CafeteriaApi.DTOs;
using CafeteriaApi.Helpers;

namespace CafeteriaApi.Services
{
    public class AuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly JwtHelper _jwtHelper;
        private readonly IConfiguration _configuration;

        public AuthService(ApplicationDbContext context, JwtHelper jwtHelper, IConfiguration configuration)
        {
            _context = context;
            _jwtHelper = jwtHelper;
            _configuration = configuration;
        }

        public async Task<AuthResponseDto> Register(RegisterDto registerDto)
        {
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == registerDto.Email);
            if (existingUser != null)
                throw new Exception("El email ya está registrado");

            var validRoles = new[] { "admin", "worker", "customer" };
            var role = validRoles.Contains(registerDto.Role.ToLower()) ? registerDto.Role.ToLower() : "customer";

            var user = new User
            {
                Email = registerDto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                FullName = registerDto.FullName,
                Phone = registerDto.Phone,
                Role = role,
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                IsActive = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var token = _jwtHelper.GenerateToken(user);
            var refreshToken = GenerateRefreshToken();

            return new AuthResponseDto
            {
                Token = token,
                RefreshToken = refreshToken,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role,
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_configuration.GetValue<int>("JwtSettings:ExpirationMinutes", 120))
            };
        }

        public async Task<AuthResponseDto> Login(LoginDto loginDto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginDto.Email);
            if (user == null || !user.IsActive)
                throw new Exception("Credenciales inválidas");

            if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
                throw new Exception("Credenciales inválidas");

            user.ConcurrencyStamp = Guid.NewGuid().ToString();
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var token = _jwtHelper.GenerateToken(user);
            var refreshToken = GenerateRefreshToken();

            return new AuthResponseDto
            {
                Token = token,
                RefreshToken = refreshToken,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role,
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_configuration.GetValue<int>("JwtSettings:ExpirationMinutes", 120))
            };
        }

        public async Task<bool> ValidateSession(int userId, string tokenConcurrencyStamp)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null || !user.IsActive)
                return false;

            return user.ConcurrencyStamp == tokenConcurrencyStamp;
        }

        public async Task<User?> GetUserById(int userId)
        {
            return await _context.Users.FindAsync(userId);
        }

        public async Task<bool> ChangePassword(int userId, ChangePasswordDto changePasswordDto)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return false;

            if (!BCrypt.Net.BCrypt.Verify(changePasswordDto.CurrentPassword, user.PasswordHash))
                throw new Exception("Contraseña actual incorrecta");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(changePasswordDto.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return true;
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }
}