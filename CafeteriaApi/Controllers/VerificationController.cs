using Microsoft.AspNetCore.Mvc;
using CafeteriaApi.DTOs;
using CafeteriaApi.Services;

namespace CafeteriaApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VerificationController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly ILogger<VerificationController> _logger;

        public VerificationController(AuthService authService, ILogger<VerificationController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendVerificationCode([FromBody] ResendVerificationDto request)
        {
            try
            {
                var result = await _authService.SendVerificationCodeAsync(request.Email);
                if (!result)
                    return BadRequest(new { message = "No se pudo enviar el código de verificación" });

                return Ok(new { message = "Código de verificación enviado exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar código de verificación");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpPost("verify")]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailDto request)
        {
            try
            {
                var result = await _authService.VerifyEmailAsync(request.Email, request.Code);
                if (!result)
                    return BadRequest(new { message = "Código inválido o expirado" });

                return Ok(new { message = "Correo verificado exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar correo");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto request)
        {
            try
            {
                var result = await _authService.SendPasswordResetCodeAsync(request.Email);
                if (!result)
                    return BadRequest(new { message = "No se pudo enviar el código de recuperación" });

                return Ok(new { message = "Código de recuperación enviado exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar código de recuperación");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto request)
        {
            try
            {
                var result = await _authService.ResetPasswordAsync(request.Email, request.Code, request.NewPassword);
                if (!result)
                    return BadRequest(new { message = "Código inválido o expirado" });

                return Ok(new { message = "Contraseña restablecida exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al restablecer contraseña");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }
    }
}