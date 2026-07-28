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
        private readonly EmailService _emailService;
        public AuthService(ApplicationDbContext context, JwtHelper jwtHelper, IConfiguration configuration, EmailService emailService)
        {
            _context = context;
            _jwtHelper = jwtHelper;
            _configuration = configuration;
            _emailService = emailService;
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
                IsActive = true,
                IsDeleted = false
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var verificationEmailSent = await SendVerificationCodeAsync(user.Email);
            if (!verificationEmailSent)
                throw new Exception("No se pudo enviar el código de verificación. Verifica la configuración de correo e inténtalo nuevamente.");

            return new AuthResponseDto
            {
                Token = string.Empty,
                RefreshToken = string.Empty,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role,
                UserId = user.Id,
                IsEmailVerified = user.IsEmailVerified,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_configuration.GetValue<int>("JwtSettings:ExpirationMinutes", 120))
            };
        }

        public async Task<AuthResponseDto> Login(LoginDto loginDto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginDto.Email);
            if (user == null || !user.IsActive || user.IsDeleted)
                throw new Exception("Credenciales inválidas");

            if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
                throw new Exception("Credenciales inválidas");

            if (!user.IsEmailVerified)
                throw new Exception("Debes verificar tu correo electrónico antes de iniciar sesión");

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
                IsEmailVerified = user.IsEmailVerified,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_configuration.GetValue<int>("JwtSettings:ExpirationMinutes", 120))
            };
        }

        public async Task<bool> ValidateSession(int userId, string tokenConcurrencyStamp)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null || !user.IsActive || user.IsDeleted)
                return false;

            if (!user.IsEmailVerified)
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
            if (user == null || user.IsDeleted)
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


        public async Task<bool> SendVerificationCodeAsync(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null || user.IsDeleted)
                return false;

            if (user.IsEmailVerified)
                return false;

            var code = GenerateVerificationCode();
            var expiresAt = DateTime.UtcNow.AddMinutes(_configuration.GetValue<int>("VerificationSettings:CodeExpirationMinutes", 10));

            var verification = new EmailVerification
            {
                UserId = user.Id,
                Code = code,
                ExpiresAt = expiresAt,
                IsUsed = false
            };

            _context.EmailVerifications.Add(verification);
            await _context.SaveChangesAsync();

            return await _emailService.SendVerificationCodeAsync(user.Email, user.FullName, code);
        }

        public async Task<bool> VerifyEmailAsync(string email, string code)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null || user.IsDeleted)
                return false;

            if (user.IsEmailVerified)
                return false;

            var verification = await _context.EmailVerifications
                .Where(v => v.UserId == user.Id && v.Code == code && !v.IsUsed && v.ExpiresAt > DateTime.UtcNow)
                .FirstOrDefaultAsync();

            if (verification == null)
                return false;

            verification.IsUsed = true;
            user.IsEmailVerified = true;
            user.EmailVerifiedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SendPasswordResetCodeAsync(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null || user.IsDeleted)
                return false;

            var code = GenerateVerificationCode();
            var expiresAt = DateTime.UtcNow.AddMinutes(_configuration.GetValue<int>("VerificationSettings:CodeExpirationMinutes", 10));

            var resetToken = new PasswordResetToken
            {
                UserId = user.Id,
                Code = code,
                ExpiresAt = expiresAt,
                IsUsed = false
            };

            _context.PasswordResetTokens.Add(resetToken);
            await _context.SaveChangesAsync();

            return await _emailService.SendPasswordResetCodeAsync(user.Email, user.FullName, code);
        }

        public async Task<bool> ResetPasswordAsync(string email, string code, string newPassword)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
                return false;

            var resetToken = await _context.PasswordResetTokens
                .Where(t => t.UserId == user.Id && t.Code == code && !t.IsUsed && t.ExpiresAt > DateTime.UtcNow)
                .FirstOrDefaultAsync();

            if (resetToken == null)
                return false;

            resetToken.IsUsed = true;
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        private string GenerateVerificationCode()
        {
            var random = new Random();
            var codeLength = _configuration.GetValue<int>("VerificationSettings:CodeLength", 6);
            return random.Next(0, (int)Math.Pow(10, codeLength) - 1).ToString($"D{codeLength}");
        }
    }
}