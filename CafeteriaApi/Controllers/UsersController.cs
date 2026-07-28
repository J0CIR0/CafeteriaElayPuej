using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CafeteriaApi.Data;
using CafeteriaApi.DTOs;
using CafeteriaApi.Models;
using System.Security.Claims;

namespace CafeteriaApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "admin")]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            var users = await _context.Users
                .Select(u => new
                {
                    u.Id,
                    u.Email,
                    u.FullName,
                    u.Role,
                    u.Phone,
                    u.IsActive,
                    u.IsEmailVerified,
                    u.CreatedAt
                })
                .OrderBy(u => u.FullName)
                .ToListAsync();

            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _context.Users
                .Select(u => new
                {
                    u.Id,
                    u.Email,
                    u.FullName,
                    u.Role,
                    u.Phone,
                    u.IsActive,
                    u.IsEmailVerified,
                    u.CreatedAt,
                    u.UpdatedAt
                })
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return NotFound(new { message = "Usuario no encontrado" });

            return Ok(user);
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] AdminUserDto adminUserDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == adminUserDto.Email);
            if (existingUser != null)
                return BadRequest(new { message = "El email ya está registrado" });

            var normalizedRole = adminUserDto.Role.ToLower();
            if (normalizedRole == "mesero")
                normalizedRole = "worker";
            else if (normalizedRole == "cliente")
                normalizedRole = "customer";
            else if (normalizedRole != "admin" && normalizedRole != "worker" && normalizedRole != "customer")
                normalizedRole = "customer";

            var user = new User
            {
                Email = adminUserDto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminUserDto.Password),
                FullName = adminUserDto.FullName,
                Phone = adminUserDto.Phone,
                Role = normalizedRole,
                IsActive = adminUserDto.IsActive,
                IsEmailVerified = adminUserDto.IsEmailVerified,
                EmailVerifiedAt = adminUserDto.IsEmailVerified ? DateTime.UtcNow : null,
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, new
            {
                user.Id,
                user.Email,
                user.FullName,
                user.Role,
                user.Phone,
                user.IsActive,
                user.IsEmailVerified,
                user.CreatedAt
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto dto)
        {
            if (id != dto.Id)
                return BadRequest(new { message = "El ID del usuario no coincide" });

            var existingUser = await _context.Users.FindAsync(id);
            if (existingUser == null)
                return NotFound(new { message = "Usuario no encontrado" });

            var duplicateEmail = await _context.Users
                .AnyAsync(u => u.Email == dto.Email && u.Id != id);

            if (duplicateEmail)
                return BadRequest(new { message = "El email ya está registrado por otro usuario" });

            var normalizedRole = (dto.Role ?? "").ToLower();
            if (normalizedRole == "mesero")
                normalizedRole = "worker";
            else if (normalizedRole == "cliente")
                normalizedRole = "customer";
            else if (normalizedRole != "admin" && normalizedRole != "worker" && normalizedRole != "customer")
                normalizedRole = "customer";

            existingUser.Email = dto.Email;
            existingUser.FullName = dto.FullName;
            existingUser.Phone = dto.Phone;
            existingUser.Role = normalizedRole;
            existingUser.IsActive = dto.IsActive;
            existingUser.IsEmailVerified = dto.IsEmailVerified;
            existingUser.EmailVerifiedAt = dto.IsEmailVerified ? (existingUser.EmailVerifiedAt ?? DateTime.UtcNow) : null;
            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                existingUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            }
            existingUser.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> ToggleUserStatus(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound(new { message = "Usuario no encontrado" });

            if (user.Role == "admin")
                return BadRequest(new { message = "No se puede desactivar al administrador" });

            user.IsActive = !user.IsActive;
            user.UpdatedAt = DateTime.UtcNow;

            if (!user.IsActive)
            {
                user.ConcurrencyStamp = Guid.NewGuid().ToString();
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = user.IsActive ? "Usuario activado" : "Usuario desactivado",
                user.IsActive
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users
                .Include(u => u.Orders)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return NotFound(new { message = "Usuario no encontrado" });

            if (user.Role == "admin")
                return BadRequest(new { message = "No se puede eliminar al administrador" });

            if (user.Orders.Any())
                return BadRequest(new { message = "No se puede eliminar un usuario con pedidos" });

            user.IsActive = false;
            user.IsDeleted = true;
            user.ConcurrencyStamp = Guid.NewGuid().ToString();
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}