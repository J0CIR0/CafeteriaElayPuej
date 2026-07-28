using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CafeteriaApi.Models
{
    public class Ingredient
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string UnitOfMeasure { get; set; } = "g";

        [Column(TypeName = "decimal(10,2)")]
        public decimal StockQuantity { get; set; } = 0;

        [Column(TypeName = "decimal(10,2)")]
        public decimal MinStockQuantity { get; set; } = 100;

        [Column(TypeName = "decimal(10,2)")]
        public decimal UnitCost { get; set; } = 0;

        public DateTime? ExpirationDate { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<ProductIngredient> ProductIngredients { get; set; } = new List<ProductIngredient>();
        public ICollection<IngredientMovement> IngredientMovements { get; set; } = new List<IngredientMovement>();
    }
}
