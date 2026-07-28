using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CafeteriaApi.Models
{
    public class ProductIngredient
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        public int IngredientId { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal QuantityRequired { get; set; }

        [ForeignKey("ProductId")]
        public Product? Product { get; set; }

        [ForeignKey("IngredientId")]
        public Ingredient? Ingredient { get; set; }
    }
}
