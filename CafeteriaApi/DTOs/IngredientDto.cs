using System.ComponentModel.DataAnnotations;

namespace CafeteriaApi.DTOs
{
    public class IngredientDto
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string UnitOfMeasure { get; set; } = "g";

        public decimal StockQuantity { get; set; } = 0;

        public decimal MinStockQuantity { get; set; } = 100;

        public decimal UnitCost { get; set; } = 0;

        public DateTime? ExpirationDate { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class RegisterWasteDto
    {
        [Required]
        public decimal Quantity { get; set; }

        public string? Reason { get; set; }
    }
}
