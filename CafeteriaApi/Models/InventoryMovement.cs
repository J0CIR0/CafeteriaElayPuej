using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CafeteriaApi.Models
{
    public class InventoryMovement
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        public string MovementType { get; set; } = "entry";

        [Required]
        public int Quantity { get; set; }

        public string? Reason { get; set; }

        [Required]
        public int UserId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("ProductId")]
        public Product? Product { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; }
    }
}