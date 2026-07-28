using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CafeteriaApi.DTOs;
using CafeteriaApi.Services;
using System.Security.Claims;

namespace CafeteriaApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(AuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            try
            {
                var result = await _authService.Register(registerDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en registro de usuario");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                var result = await _authService.Login(loginDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en login");
                return Unauthorized(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                    return Unauthorized(new { message = "Usuario no autenticado" });

                var result = await _authService.ChangePassword(userId, changePasswordDto);
                return Ok(new { message = "Contraseña actualizada exitosamente" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("validate-session")]
        public async Task<IActionResult> ValidateSession()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                    return Unauthorized(new { message = "Usuario no autenticado" });

                var concurrencyStamp = User.FindFirst("ConcurrencyStamp")?.Value;
                if (string.IsNullOrEmpty(concurrencyStamp))
                    return Unauthorized(new { message = "Sesión inválida" });

                var isValid = await _authService.ValidateSession(userId, concurrencyStamp);
                if (!isValid)
                    return Unauthorized(new { message = "Sesión expirada - has iniciado sesión en otro dispositivo" });

                var user = await _authService.GetUserById(userId);
                return Ok(new
                {
                    userId = user?.Id,
                    email = user?.Email,
                    fullName = user?.FullName,
                    role = user?.Role
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validando sesión");
                return StatusCode(500, new { message = "Error interno" });
            }
        }
    }
}