using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CafeteriaApi.Data;
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
                .Where(u => u.IsActive)
                .Select(u => new
                {
                    u.Id,
                    u.Email,
                    u.FullName,
                    u.Role,
                    u.Phone,
                    u.IsActive,
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
                .Where(u => u.Id == id && u.IsActive)
                .Select(u => new
                {
                    u.Id,
                    u.Email,
                    u.FullName,
                    u.Role,
                    u.Phone,
                    u.IsActive,
                    u.CreatedAt,
                    u.UpdatedAt
                })
                .FirstOrDefaultAsync();

            if (user == null)
                return NotFound(new { message = "Usuario no encontrado" });

            return Ok(user);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, User user)
        {
            if (id != user.Id)
                return BadRequest(new { message = "El ID del usuario no coincide" });

            var existingUser = await _context.Users.FindAsync(id);
            if (existingUser == null)
                return NotFound(new { message = "Usuario no encontrado" });

            var duplicateEmail = await _context.Users
                .AnyAsync(u => u.Email == user.Email && u.Id != id);

            if (duplicateEmail)
                return BadRequest(new { message = "El email ya está registrado por otro usuario" });

            existingUser.Email = user.Email;
            existingUser.FullName = user.FullName;
            existingUser.Phone = user.Phone;
            existingUser.Role = user.Role;
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
            user.ConcurrencyStamp = Guid.NewGuid().ToString();
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}