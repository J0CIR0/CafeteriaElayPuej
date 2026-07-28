using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CafeteriaApi.Data;
using CafeteriaApi.Models;
using CafeteriaApi.DTOs;

namespace CafeteriaApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.IsAvailable)
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    PreparationTime = p.PreparationTime,
                    Origin = p.Origin,
                    FlavorNotes = p.FlavorNotes,
                    ImageUrl = p.ImageUrl,
                    Stock = p.Stock,
                    CategoryName = p.Category != null ? p.Category.Name : string.Empty,
                    CategoryId = p.CategoryId
                })
                .OrderBy(p => p.Name)
                .ToListAsync();

            return Ok(products);
        }

        [HttpGet("available")]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetAvailableProducts()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.IsAvailable && p.Stock > 0)
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    PreparationTime = p.PreparationTime,
                    Origin = p.Origin,
                    FlavorNotes = p.FlavorNotes,
                    ImageUrl = p.ImageUrl,
                    Stock = p.Stock,
                    CategoryName = p.Category != null ? p.Category.Name : string.Empty,
                    CategoryId = p.CategoryId
                })
                .OrderBy(p => p.CategoryName)
                .ThenBy(p => p.Name)
                .ToListAsync();

            return Ok(products);
        }

        [HttpGet("category/{categoryId}")]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetProductsByCategory(int categoryId)
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.CategoryId == categoryId && p.IsAvailable)
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    PreparationTime = p.PreparationTime,
                    Origin = p.Origin,
                    FlavorNotes = p.FlavorNotes,
                    ImageUrl = p.ImageUrl,
                    Stock = p.Stock,
                    CategoryName = p.Category != null ? p.Category.Name : string.Empty,
                    CategoryId = p.CategoryId
                })
                .ToListAsync();

            return Ok(products);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDto>> GetProduct(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.Id == id && p.IsAvailable)
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    PreparationTime = p.PreparationTime,
                    Origin = p.Origin,
                    FlavorNotes = p.FlavorNotes,
                    ImageUrl = p.ImageUrl,
                    Stock = p.Stock,
                    CategoryName = p.Category != null ? p.Category.Name : string.Empty,
                    CategoryId = p.CategoryId
                })
                .FirstOrDefaultAsync();

            if (product == null)
                return NotFound(new { message = "Producto no encontrado" });

            return Ok(product);
        }

        [Authorize(Roles = "admin")]
        [HttpPost]
        public async Task<ActionResult<Product>> CreateProduct(Product product)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var category = await _context.Categories.FindAsync(product.CategoryId);
            if (category == null || !category.IsActive)
                return BadRequest(new { message = "Categoría no válida" });

            product.CreatedAt = DateTime.UtcNow;
            product.UpdatedAt = DateTime.UtcNow;
            product.IsAvailable = true;

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            await RecordInventoryMovement(product.Id, product.Stock, "entry", "Creación de producto", 1);

            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        }

        [Authorize(Roles = "admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, Product product)
        {
            if (id != product.Id)
                return BadRequest(new { message = "El ID del producto no coincide" });

            var existingProduct = await _context.Products.FindAsync(id);
            if (existingProduct == null)
                return NotFound(new { message = "Producto no encontrado" });

            var category = await _context.Categories.FindAsync(product.CategoryId);
            if (category == null || !category.IsActive)
                return BadRequest(new { message = "Categoría no válida" });

            int oldStock = existingProduct.Stock;

            existingProduct.Name = product.Name;
            existingProduct.Description = product.Description;
            existingProduct.Price = product.Price;
            existingProduct.PreparationTime = product.PreparationTime;
            existingProduct.Origin = product.Origin;
            existingProduct.FlavorNotes = product.FlavorNotes;
            existingProduct.ImageUrl = product.ImageUrl;
            existingProduct.Stock = product.Stock;
            existingProduct.MinStock = product.MinStock;
            existingProduct.IsAvailable = product.IsAvailable;
            existingProduct.CategoryId = product.CategoryId;
            existingProduct.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            if (oldStock != product.Stock)
            {
                string movementType = product.Stock > oldStock ? "entry" : "exit";
                int quantity = Math.Abs(product.Stock - oldStock);
                await RecordInventoryMovement(id, quantity, movementType, "Actualización de inventario", 1);
            }

            return NoContent();
        }

        [Authorize(Roles = "admin")]
        [HttpPatch("{id}/stock")]
        public async Task<IActionResult> UpdateStock(int id, [FromBody] UpdateStockDto updateStockDto)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound(new { message = "Producto no encontrado" });

            int oldStock = product.Stock;
            product.Stock = updateStockDto.NewStock;
            product.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            string movementType = updateStockDto.NewStock > oldStock ? "entry" : "exit";
            int quantity = Math.Abs(updateStockDto.NewStock - oldStock);

            await RecordInventoryMovement(id, quantity, movementType, updateStockDto.Reason ?? "Ajuste de inventario", 1);

            return NoContent();
        }

        [Authorize(Roles = "admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products
                .Include(p => p.OrderDetails)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return NotFound(new { message = "Producto no encontrado" });

            if (product.OrderDetails.Any())
                return BadRequest(new { message = "No se puede eliminar un producto que tiene pedidos asociados" });

            product.IsAvailable = false;
            product.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("low-stock")]
        [Authorize(Roles = "admin,worker")]
        public async Task<ActionResult<IEnumerable<Product>>> GetLowStockProducts()
        {
            var products = await _context.Products
                .Where(p => p.IsAvailable && p.Stock <= p.MinStock)
                .OrderBy(p => p.Stock)
                .ToListAsync();

            return Ok(products);
        }

        private async Task RecordInventoryMovement(int productId, int quantity, string movementType, string reason, int userId)
        {
            var movement = new InventoryMovement
            {
                ProductId = productId,
                Quantity = quantity,
                MovementType = movementType,
                Reason = reason,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.InventoryMovements.Add(movement);
            await _context.SaveChangesAsync();
        }
    }
}