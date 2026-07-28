using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CafeteriaApi.Models
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = "customer";

        [StringLength(20)]
        public string? Phone { get; set; }

        public string ConcurrencyStamp { get; set; } = Guid.NewGuid().ToString();

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<InventoryMovement> InventoryMovements { get; set; } = new List<InventoryMovement>();
        public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
    }
}