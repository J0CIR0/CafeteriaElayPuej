using System.Security.Claims;
using CafeteriaApi.Services;

namespace CafeteriaApi.Middleware
{
    public class SessionValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<SessionValidationMiddleware> _logger;

        public SessionValidationMiddleware(RequestDelegate next, ILogger<SessionValidationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, AuthService authService)
        {
            var user = context.User;
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
            var concurrencyStampClaim = user.FindFirst("ConcurrencyStamp");

            if (user.Identity != null && user.Identity.IsAuthenticated && userIdClaim != null)
            {
                if (int.TryParse(userIdClaim.Value, out int userId))
                {
                    var tokenStamp = concurrencyStampClaim?.Value;
                    
                    if (string.IsNullOrEmpty(tokenStamp))
                    {
                        _logger.LogWarning($"Token sin ConcurrencyStamp para usuario {userId}");
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        await context.Response.WriteAsync("Sesión inválida");
                        return;
                    }

                    var isValid = await authService.ValidateSession(userId, tokenStamp);
                    
                    if (!isValid)
                    {
                        _logger.LogWarning($"Sesión inválida para usuario {userId} - posible inicio de sesión en otro dispositivo");
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        await context.Response.WriteAsync("Sesión expirada - has iniciado sesión en otro dispositivo");
                        return;
                    }
                }
            }

            await _next(context);
        }
    }
}