using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CafeteriaApi.Data;
using CafeteriaApi.Models;
using CafeteriaApi.DTOs;

namespace CafeteriaApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "admin,worker")]
    public class RecipesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RecipesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("product/{productId}")]
        public async Task<ActionResult<ProductRecipeDto>> GetProductRecipe(int productId)
        {
            var product = await _context.Products
                .Include(p => p.ProductIngredients)
                .ThenInclude(pi => pi.Ingredient)
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null)
                return NotFound(new { message = "Producto no encontrado" });

            decimal totalCost = 0;
            var ingredientItems = new List<RecipeIngredientItemDto>();

            foreach (var pi in product.ProductIngredients)
            {
                if (pi.Ingredient != null && pi.Ingredient.IsActive)
                {
                    decimal subtotal = pi.QuantityRequired * pi.Ingredient.UnitCost;
                    totalCost += subtotal;
                    ingredientItems.Add(new RecipeIngredientItemDto
                    {
                        IngredientId = pi.IngredientId,
                        IngredientName = pi.Ingredient.Name,
                        UnitOfMeasure = pi.Ingredient.UnitOfMeasure,
                        QuantityRequired = pi.QuantityRequired,
                        UnitCost = pi.Ingredient.UnitCost
                    });
                }
            }

            var recipeDto = new ProductRecipeDto
            {
                ProductId = product.Id,
                ProductName = product.Name,
                SalePrice = product.Price,
                EstimatedCost = totalCost,
                Ingredients = ingredientItems
            };

            return Ok(recipeDto);
        }

        [HttpPut("product/{productId}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> SaveProductRecipe(int productId, [FromBody] SaveRecipeDto dto)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
                return NotFound(new { message = "Producto no encontrado" });

            var existingRecipe = await _context.ProductIngredients
                .Where(pi => pi.ProductId == productId)
                .ToListAsync();

            _context.ProductIngredients.RemoveRange(existingRecipe);

            if (dto.Ingredients != null && dto.Ingredients.Any())
            {
                foreach (var item in dto.Ingredients)
                {
                    if (item.QuantityRequired > 0)
                    {
                        var ingredient = await _context.Ingredients.FindAsync(item.IngredientId);
                        if (ingredient != null && ingredient.IsActive)
                        {
                            _context.ProductIngredients.Add(new ProductIngredient
                            {
                                ProductId = productId,
                                IngredientId = item.IngredientId,
                                QuantityRequired = item.QuantityRequired
                            });
                        }
                    }
                }
            }

            product.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
