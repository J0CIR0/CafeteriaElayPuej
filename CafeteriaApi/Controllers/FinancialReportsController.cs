using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CafeteriaApi.Data;
using CafeteriaApi.DTOs;

namespace CafeteriaApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "admin")]
    public class FinancialReportsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public FinancialReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("summary")]
        public async Task<ActionResult<FinancialSummaryDto>> GetFinancialSummary()
        {
            var orders = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .ThenInclude(p => p.Category)
                .Where(o => o.PaymentStatus == "paid" || o.OrderStatus == "completed")
                .ToListAsync();

            var recipes = await _context.ProductIngredients
                .Include(pi => pi.Ingredient)
                .ToListAsync();

            var unitCostMap = new Dictionary<int, decimal>();
            var products = await _context.Products.Include(p => p.Category).ToListAsync();

            foreach (var p in products)
            {
                var pIngredients = recipes.Where(r => r.ProductId == p.Id).ToList();
                decimal cost = 0;
                foreach (var pi in pIngredients)
                {
                    if (pi.Ingredient != null && pi.Ingredient.IsActive)
                    {
                        cost += pi.QuantityRequired * pi.Ingredient.UnitCost;
                    }
                }
                unitCostMap[p.Id] = cost;
            }

            var productSales = new Dictionary<int, int>();
            foreach (var order in orders)
            {
                foreach (var detail in order.OrderDetails)
                {
                    if (!productSales.ContainsKey(detail.ProductId))
                        productSales[detail.ProductId] = 0;
                    productSales[detail.ProductId] += detail.Quantity;
                }
            }

            var profitabilityList = new List<ProductProfitabilityDto>();
            decimal totalRevenue = 0;
            decimal totalProductionCost = 0;

            foreach (var p in products)
            {
                int quantitySold = productSales.ContainsKey(p.Id) ? productSales[p.Id] : 0;
                decimal unitCost = unitCostMap.ContainsKey(p.Id) ? unitCostMap[p.Id] : 0;

                var item = new ProductProfitabilityDto
                {
                    ProductId = p.Id,
                    ProductName = p.Name,
                    CategoryName = p.Category?.Name ?? "Sin categoría",
                    SalePrice = p.Price,
                    UnitCost = unitCost,
                    TotalQuantitySold = quantitySold
                };

                profitabilityList.Add(item);

                totalRevenue += item.TotalRevenue;
                totalProductionCost += item.TotalCost;
            }

            var wasteLosses = await _context.IngredientMovements
                .Include(im => im.Ingredient)
                .Where(im => im.MovementType == "waste_loss")
                .OrderByDescending(im => im.CreatedAt)
                .ToListAsync();

            decimal totalWasteLossCost = wasteLosses.Sum(w => w.TotalCostLoss);

            var wasteLossDtos = wasteLosses.Select(w => new WasteLossDto
            {
                Id = w.Id,
                IngredientName = w.Ingredient?.Name ?? "Insumo eliminado",
                Quantity = w.Quantity,
                UnitOfMeasure = w.Ingredient?.UnitOfMeasure ?? "",
                UnitCostAtTime = w.UnitCostAtTime,
                TotalCostLoss = w.TotalCostLoss,
                Reason = w.Reason ?? "Pérdida por insumo mermado/vencido",
                CreatedAt = w.CreatedAt
            }).ToList();

            var nextWeek = DateTime.UtcNow.AddDays(7);
            var expiringIngredients = await _context.Ingredients
                .Where(i => i.IsActive && i.ExpirationDate.HasValue && i.ExpirationDate.Value <= nextWeek)
                .OrderBy(i => i.ExpirationDate)
                .Select(i => new ExpiringIngredientDto
                {
                    IngredientId = i.Id,
                    Name = i.Name,
                    StockQuantity = i.StockQuantity,
                    UnitOfMeasure = i.UnitOfMeasure,
                    ExpirationDate = i.ExpirationDate.Value
                })
                .ToListAsync();

            var summary = new FinancialSummaryDto
            {
                TotalRevenue = totalRevenue,
                TotalProductionCost = totalProductionCost,
                TotalNetProfit = totalRevenue - totalProductionCost - totalWasteLossCost,
                TotalWasteLossCost = totalWasteLossCost,
                ProductProfitability = profitabilityList.OrderByDescending(p => p.TotalRevenue).ToList(),
                ExpiringIngredients = expiringIngredients,
                WasteLosses = wasteLossDtos
            };

            return Ok(summary);
        }
    }
}
