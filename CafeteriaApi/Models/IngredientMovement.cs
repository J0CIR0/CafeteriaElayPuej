using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CafeteriaApi.Models
{
    public class IngredientMovement
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int IngredientId { get; set; }

        [Required]
        [StringLength(50)]
        public string MovementType { get; set; } = "entry";

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Quantity { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal UnitCostAtTime { get; set; } = 0;

        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalCostLoss { get; set; } = 0;

        public string? Reason { get; set; }

        [Required]
        public int UserId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("IngredientId")]
        public Ingredient? Ingredient { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; }
    }
}
