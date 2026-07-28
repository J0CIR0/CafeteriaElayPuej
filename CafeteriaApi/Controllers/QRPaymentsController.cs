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
    public class QRPaymentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public QRPaymentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("order/{orderId}")]
        public async Task<ActionResult<QRPayment>> GetQRPaymentByOrder(int orderId)
        {
            var qrPayment = await _context.QRPayments
                .Include(q => q.Order)
                .FirstOrDefaultAsync(q => q.OrderId == orderId);

            if (qrPayment == null)
                return NotFound(new { message = "No se encontró pago QR para este pedido" });

            return Ok(new
            {
                qrPayment.Id,
                qrPayment.OrderId,
                qrPayment.QrImageUrl,
                qrPayment.Amount,
                qrPayment.PaymentReference,
                qrPayment.VerifiedAt,
                qrPayment.CreatedAt,
                OrderNumber = qrPayment.Order?.OrderNumber,
                TotalAmount = qrPayment.Order?.Total
            });
        }

        [HttpPost]
        public async Task<ActionResult<QRPayment>> CreateQRPayment(QRPayment qrPayment)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var order = await _context.Orders.FindAsync(qrPayment.OrderId);
            if (order == null)
                return BadRequest(new { message = "Pedido no encontrado" });

            if (order.PaymentStatus != "pending")
                return BadRequest(new { message = "El pedido ya está pagado" });

            var existingQR = await _context.QRPayments
                .FirstOrDefaultAsync(q => q.OrderId == qrPayment.OrderId);

            if (existingQR != null)
                return BadRequest(new { message = "Ya existe un QR para este pedido" });

            qrPayment.QrImageUrl = "/images/qr-code.png";
            qrPayment.Amount = order.Total;
            qrPayment.CreatedAt = DateTime.UtcNow;

            _context.QRPayments.Add(qrPayment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetQRPaymentByOrder), new { orderId = qrPayment.OrderId }, qrPayment);
        }

        [Authorize(Roles = "admin,worker")]
        [HttpPatch("{id}/verify")]
        public async Task<IActionResult> VerifyQRPayment(int id)
        {
            var qrPayment = await _context.QRPayments
                .Include(q => q.Order)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (qrPayment == null)
                return NotFound(new { message = "Pago QR no encontrado" });

            if (qrPayment.VerifiedAt != null)
                return BadRequest(new { message = "Este pago ya fue verificado" });

            var userId = GetCurrentUserId();

            qrPayment.VerifiedAt = DateTime.UtcNow;
            qrPayment.VerifiedBy = userId;

            if (qrPayment.Order != null)
            {
                qrPayment.Order.PaymentStatus = "paid";
                qrPayment.Order.OrderStatus = "preparing";
                qrPayment.Order.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Pago verificado exitosamente",
                qrPayment.VerifiedAt,
                qrPayment.Order?.OrderNumber
            });
        }

        [Authorize(Roles = "admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteQRPayment(int id)
        {
            var qrPayment = await _context.QRPayments.FindAsync(id);
            if (qrPayment == null)
                return NotFound(new { message = "Pago QR no encontrado" });

            if (qrPayment.VerifiedAt != null)
                return BadRequest(new { message = "No se puede eliminar un pago ya verificado" });

            _context.QRPayments.Remove(qrPayment);
            await _context.SaveChangesAsync();

            return NoContent();
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