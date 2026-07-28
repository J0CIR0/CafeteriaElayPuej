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
    [Authorize(Roles = "admin,worker")]
    public class InventoryController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public InventoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("movements")]
        public async Task<ActionResult<IEnumerable<InventoryMovement>>> GetMovements()
        {
            var movements = await _context.InventoryMovements
                .Include(m => m.Product)
                .Include(m => m.User)
                .OrderByDescending(m => m.CreatedAt)
                .Take(100)
                .ToListAsync();

            return Ok(movements);
        }

        [HttpGet("movements/product/{productId}")]
        public async Task<ActionResult<IEnumerable<InventoryMovement>>> GetMovementsByProduct(int productId)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
                return NotFound(new { message = "Producto no encontrado" });

            var movements = await _context.InventoryMovements
                .Include(m => m.User)
                .Where(m => m.ProductId == productId)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();

            return Ok(movements);
        }

        [HttpPost("movements")]
        public async Task<ActionResult<InventoryMovement>> CreateMovement(InventoryMovement movement)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var product = await _context.Products.FindAsync(movement.ProductId);
            if (product == null)
                return BadRequest(new { message = "Producto no encontrado" });

            var userId = GetCurrentUserId();

            movement.UserId = userId;
            movement.CreatedAt = DateTime.UtcNow;

            _context.InventoryMovements.Add(movement);
            await _context.SaveChangesAsync();

            if (movement.MovementType == "entry")
            {
                product.Stock += movement.Quantity;
            }
            else if (movement.MovementType == "exit")
            {
                if (product.Stock < movement.Quantity)
                    return BadRequest(new { message = "Stock insuficiente" });

                product.Stock -= movement.Quantity;
            }
            else
            {
                product.Stock = movement.Quantity;
            }

            product.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMovements), new { id = movement.Id }, movement);
        }

        [HttpGet("summary")]
        public async Task<ActionResult<object>> GetInventorySummary()
        {
            var products = await _context.Products
                .Where(p => p.IsAvailable)
                .ToListAsync();

            var totalProducts = products.Count;
            var totalStock = products.Sum(p => p.Stock);
            var lowStockCount = products.Count(p => p.Stock <= p.MinStock);
            var outOfStockCount = products.Count(p => p.Stock == 0);

            return Ok(new
            {
                totalProducts,
                totalStock,
                lowStockCount,
                outOfStockCount,
                lowStockProducts = products.Where(p => p.Stock <= p.MinStock && p.Stock > 0)
                    .Select(p => new { p.Id, p.Name, p.Stock, p.MinStock })
            });
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                throw new UnauthorizedAccessException("Usuario no autenticado");

            return int.Parse(userIdClaim.Value);
        }
    }
}