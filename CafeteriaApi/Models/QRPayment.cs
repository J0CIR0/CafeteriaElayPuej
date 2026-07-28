using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CafeteriaApi.Models
{
    public class QRPayment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int OrderId { get; set; }

        [Required]
        [StringLength(500)]
        public string QrImageUrl { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Amount { get; set; }

        [StringLength(100)]
        public string? PaymentReference { get; set; }

        public DateTime? VerifiedAt { get; set; }

        public int? VerifiedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("OrderId")]
        public Order? Order { get; set; }

        [ForeignKey("VerifiedBy")]
        public User? VerifiedByUser { get; set; }
    }
}