using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CafeteriaApi.Data;
using CafeteriaApi.Models;
using CafeteriaApi.DTOs;
using System.Security.Claims;

namespace CafeteriaApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = "admin,worker")]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
        {
            var orders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return Ok(orders);
        }

        [HttpGet("my-orders")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Order>>> GetMyOrders()
        {
            var userId = GetCurrentUserId();

            var orders = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return Ok(orders);
        }

        [HttpGet("pending-payment")]
        [Authorize(Roles = "admin,worker")]
        public async Task<ActionResult<IEnumerable<Order>>> GetPendingPaymentOrders()
        {
            var orders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .Where(o => o.PaymentStatus == "pending")
                .OrderBy(o => o.CreatedAt)
                .ToListAsync();

            return Ok(orders);
        }

        [HttpGet("paid")]
        [Authorize(Roles = "admin,worker")]
        public async Task<ActionResult<IEnumerable<Order>>> GetPaidOrders()
        {
            var orders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .Where(o => o.PaymentStatus == "paid")
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return Ok(orders);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Order>> GetOrder(int id)
        {
            var userId = GetCurrentUserId();
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return NotFound(new { message = "Pedido no encontrado" });

            if (userRole != "admin" && userRole != "worker" && order.UserId != userId)
                return Forbid();

            return Ok(order);
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Order>> CreateOrder(CreateOrderDto createOrderDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var userId = GetCurrentUserId();
                var orderNumber = GenerateOrderNumber();

                var paymentMethod = (createOrderDto.PaymentMethod ?? "qr").ToLower().Trim();
                if (paymentMethod == "efectivo") paymentMethod = "cash";
                if (paymentMethod != "qr" && paymentMethod != "cash" && paymentMethod != "card") paymentMethod = "qr";

                decimal subtotal = 0;
                var orderDetails = new List<OrderDetail>();

                foreach (var item in createOrderDto.OrderItems)
                {
                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product == null)
                        return BadRequest(new { message = $"Producto con ID {item.ProductId} no encontrado" });

                    if (!product.IsAvailable)
                        return BadRequest(new { message = $"Producto '{product.Name}' no está disponible" });

                    if (product.Stock < item.Quantity)
                        return BadRequest(new { message = $"Stock insuficiente para '{product.Name}' (disponible: {product.Stock})" });

                    decimal itemSubtotal = product.Price * item.Quantity;
                    subtotal += itemSubtotal;

                    orderDetails.Add(new OrderDetail
                    {
                        ProductId = product.Id,
                        Quantity = item.Quantity,
                        UnitPrice = product.Price,
                        Subtotal = itemSubtotal
                    });

                    product.Stock -= item.Quantity;
                    product.UpdatedAt = DateTime.UtcNow;
                }

                decimal tax = 0;
                decimal total = subtotal;

                var order = new Order
                {
                    UserId = userId,
                    OrderNumber = orderNumber,
                    Subtotal = subtotal,
                    Tax = tax,
                    Total = total,
                    PaymentMethod = paymentMethod,
                    PaymentStatus = "pending",
                    OrderStatus = "pending",
                    PickupTime = createOrderDto.PickupTime,
                    Notes = createOrderDto.Notes,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    OrderDetails = orderDetails
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                foreach (var item in createOrderDto.OrderItems)
                {
                    await RecordInventoryMovement(item.ProductId, item.Quantity, "exit", $"Pedido {orderNumber}", userId);

                    var recipeItems = await _context.ProductIngredients
                        .Include(pi => pi.Ingredient)
                        .Include(pi => pi.Product)
                        .Where(pi => pi.ProductId == item.ProductId)
                        .ToListAsync();

                    foreach (var pi in recipeItems)
                    {
                        if (pi.Ingredient != null && pi.Ingredient.IsActive)
                        {
                            decimal qtyDeducted = pi.QuantityRequired * item.Quantity;
                            pi.Ingredient.StockQuantity -= qtyDeducted;
                            if (pi.Ingredient.StockQuantity < 0)
                                pi.Ingredient.StockQuantity = 0;
                            pi.Ingredient.UpdatedAt = DateTime.UtcNow;

                            _context.IngredientMovements.Add(new IngredientMovement
                            {
                                IngredientId = pi.IngredientId,
                                MovementType = "sale_deduction",
                                Quantity = qtyDeducted,
                                UnitCostAtTime = pi.Ingredient.UnitCost,
                                TotalCostLoss = 0,
                                Reason = $"Deducción por venta de {item.Quantity}x {pi.Product?.Name ?? "Producto"} (Pedido #{orderNumber})",
                                UserId = userId,
                                CreatedAt = DateTime.UtcNow
                            });
                        }
                    }
                }
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error al procesar el pedido: {ex.Message}" });
            }
        }

        [Authorize(Roles = "worker")]
        [HttpPatch("{id}/payment-status")]
        public async Task<IActionResult> UpdatePaymentStatus(int id, [FromBody] UpdatePaymentStatusDto updateDto)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
                return NotFound(new { message = "Pedido no encontrado" });

            if (order.PaymentStatus == "paid")
                return BadRequest(new { message = "El pedido ya está pagado" });

            order.PaymentStatus = updateDto.Status;
            order.UpdatedAt = DateTime.UtcNow;

            if (updateDto.Status == "paid")
            {
                order.OrderStatus = "preparing";
            }

            await _context.SaveChangesAsync();

            if (updateDto.Status == "paid")
            {
                var qrPayment = new QRPayment
                {
                    OrderId = order.Id,
                    QrImageUrl = "/images/qr-code.png",
                    Amount = order.Total,
                    VerifiedAt = DateTime.UtcNow,
                    VerifiedBy = GetCurrentUserId(),
                    CreatedAt = DateTime.UtcNow
                };

                _context.QRPayments.Add(qrPayment);
                await _context.SaveChangesAsync();
            }

            return NoContent();
        }

        [Authorize(Roles = "worker")]
        [HttpPatch("{id}/order-status")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusDto updateDto)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
                return NotFound(new { message = "Pedido no encontrado" });

            if (order.PaymentStatus != "paid")
                return BadRequest(new { message = "El pedido debe estar pagado para cambiar el estado" });

            order.OrderStatus = updateDto.Status;
            order.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [Authorize(Roles = "admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return NotFound(new { message = "Pedido no encontrado" });

            if (order.PaymentStatus == "paid")
                return BadRequest(new { message = "No se puede eliminar un pedido pagado" });

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private string GenerateOrderNumber()
        {
            var now = DateTime.Now;
            var datePart = now.ToString("yyyyMMdd");
            var lastOrder = _context.Orders
                .Where(o => o.OrderNumber.StartsWith(datePart))
                .OrderByDescending(o => o.OrderNumber)
                .FirstOrDefault();

            int sequence = 1;
            if (lastOrder != null)
            {
                var lastSequence = int.Parse(lastOrder.OrderNumber.Substring(8));
                sequence = lastSequence + 1;
            }

            return $"{datePart}{sequence:D4}";
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub") ?? User.FindFirst("nameid");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                throw new UnauthorizedAccessException("Usuario no autenticado");

            return userId;
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