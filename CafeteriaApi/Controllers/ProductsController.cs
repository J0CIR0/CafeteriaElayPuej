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
                    CategoryName = p.Category != null ? p.Category.Name : string.Empty
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
                    CategoryName = p.Category != null ? p.Category.Name : string.Empty
                })
                .FirstOrDefaultAsync();

            if (product == null)
                return NotFound();

            return Ok(product);
        }

        [HttpPost]
        public async Task<ActionResult<Product>> CreateProduct(Product product)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            product.CreatedAt = DateTime.UtcNow;
            product.UpdatedAt = DateTime.UtcNow;

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, Product product)
        {
            if (id != product.Id)
                return BadRequest();

            var existingProduct = await _context.Products.FindAsync(id);
            if (existingProduct == null)
                return NotFound();

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

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await ProductExists(id))
                    return NotFound();
                throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound();

            product.IsAvailable = false;
            product.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private async Task<bool> ProductExists(int id)
        {
            return await _context.Products.AnyAsync(p => p.Id == id);
        }
    }
}