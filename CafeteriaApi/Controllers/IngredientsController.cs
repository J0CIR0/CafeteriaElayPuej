using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CafeteriaApi.Data;
using CafeteriaApi.Models;
using CafeteriaApi.DTOs;
using System.Security.Claims;

namespace CafeteriaApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "admin,worker")]
    public class IngredientsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public IngredientsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Ingredient>>> GetIngredients()
        {
            var ingredients = await _context.Ingredients
                .Where(i => i.IsActive)
                .OrderBy(i => i.Name)
                .ToListAsync();

            return Ok(ingredients);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Ingredient>> GetIngredient(int id)
        {
            var ingredient = await _context.Ingredients.FindAsync(id);
            if (ingredient == null || !ingredient.IsActive)
                return NotFound(new { message = "Insumo no encontrado" });

            return Ok(ingredient);
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<Ingredient>> CreateIngredient([FromBody] IngredientDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var ingredient = new Ingredient
            {
                Name = dto.Name,
                UnitOfMeasure = dto.UnitOfMeasure,
                StockQuantity = dto.StockQuantity,
                MinStockQuantity = dto.MinStockQuantity,
                UnitCost = dto.UnitCost,
                ExpirationDate = dto.ExpirationDate,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Ingredients.Add(ingredient);
            await _context.SaveChangesAsync();

            if (dto.StockQuantity > 0)
            {
                await RecordIngredientMovement(ingredient.Id, "entry", dto.StockQuantity, dto.UnitCost, 0, "Inventario inicial de insumo", GetUserId());
            }

            return CreatedAtAction(nameof(GetIngredient), new { id = ingredient.Id }, ingredient);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> UpdateIngredient(int id, [FromBody] IngredientDto dto)
        {
            if (id != dto.Id && dto.Id != 0)
                return BadRequest(new { message = "El ID del insumo no coincide" });

            var ingredient = await _context.Ingredients.FindAsync(id);
            if (ingredient == null || !ingredient.IsActive)
                return NotFound(new { message = "Insumo no encontrado" });

            decimal oldStock = ingredient.StockQuantity;

            ingredient.Name = dto.Name;
            ingredient.UnitOfMeasure = dto.UnitOfMeasure;
            ingredient.StockQuantity = dto.StockQuantity;
            ingredient.MinStockQuantity = dto.MinStockQuantity;
            ingredient.UnitCost = dto.UnitCost;
            ingredient.ExpirationDate = dto.ExpirationDate;
            ingredient.IsActive = dto.IsActive;
            ingredient.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            if (oldStock != dto.StockQuantity)
            {
                string type = dto.StockQuantity > oldStock ? "entry" : "exit";
                decimal diff = Math.Abs(dto.StockQuantity - oldStock);
                await RecordIngredientMovement(id, type, diff, dto.UnitCost, 0, "Ajuste manual de inventario de insumo", GetUserId());
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteIngredient(int id)
        {
            var ingredient = await _context.Ingredients
                .Include(i => i.ProductIngredients)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (ingredient == null)
                return NotFound(new { message = "Insumo no encontrado" });

            if (ingredient.ProductIngredients.Any())
                return BadRequest(new { message = "No se puede eliminar un insumo que forma parte de una receta de producto activa" });

            var movements = await _context.IngredientMovements.Where(m => m.IngredientId == id).ToListAsync();
            _context.IngredientMovements.RemoveRange(movements);

            _context.Ingredients.Remove(ingredient);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("{id}/waste")]
        [Authorize(Roles = "admin,worker")]
        public async Task<IActionResult> RegisterWaste(int id, [FromBody] RegisterWasteDto dto)
        {
            var ingredient = await _context.Ingredients.FindAsync(id);
            if (ingredient == null || !ingredient.IsActive)
                return NotFound(new { message = "Insumo no encontrado" });

            if (dto.Quantity <= 0)
                return BadRequest(new { message = "La cantidad mermada/vencida debe ser mayor a 0" });

            if (dto.Quantity > ingredient.StockQuantity)
                return BadRequest(new { message = "La cantidad mermada no puede ser mayor al stock actual" });

            ingredient.StockQuantity -= dto.Quantity;
            ingredient.UpdatedAt = DateTime.UtcNow;

            decimal totalLoss = dto.Quantity * ingredient.UnitCost;

            await RecordIngredientMovement(
                id,
                "waste_loss",
                dto.Quantity,
                ingredient.UnitCost,
                totalLoss,
                dto.Reason ?? "Baja por insumo vencido o mermado",
                GetUserId()
            );

            await _context.SaveChangesAsync();

            return Ok(new { message = "Baja de insumo registrada correctamente", totalCostLoss = totalLoss, newStock = ingredient.StockQuantity });
        }

        private int GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                return userId;

            var firstUser = _context.Users.FirstOrDefault();
            return firstUser?.Id ?? 1;
        }

        private async Task RecordIngredientMovement(int ingredientId, string movementType, decimal quantity, decimal unitCost, decimal totalCostLoss, string reason, int userId)
        {
            var movement = new IngredientMovement
            {
                IngredientId = ingredientId,
                MovementType = movementType,
                Quantity = quantity,
                UnitCostAtTime = unitCost,
                TotalCostLoss = totalCostLoss,
                Reason = reason,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.IngredientMovements.Add(movement);
            await _context.SaveChangesAsync();
        }
    }
}
