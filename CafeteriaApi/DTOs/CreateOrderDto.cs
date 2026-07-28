using System.ComponentModel.DataAnnotations;

namespace CafeteriaApi.DTOs
{
    public class CreateOrderDto
    {
        [Required]
        public List<OrderItemDto> OrderItems { get; set; } = new List<OrderItemDto>();

        [Required]
        public string PaymentMethod { get; set; } = "qr";

        public DateTime? PickupTime { get; set; }

        public string? Notes { get; set; }
    }

    public class OrderItemDto
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }
    }

    public class UpdatePaymentStatusDto
    {
        [Required]
        public string Status { get; set; } = "pending";
    }

    public class UpdateOrderStatusDto
    {
        [Required]
        public string Status { get; set; } = "pending";
    }
}