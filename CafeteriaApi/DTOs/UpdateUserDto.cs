using System.ComponentModel.DataAnnotations;

namespace CafeteriaApi.DTOs
{
    public class UpdateUserDto
    {
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string FullName { get; set; } = string.Empty;

        [StringLength(20)]
        public string? Phone { get; set; }

        [Required]
        public string Role { get; set; } = "customer";

        public bool IsActive { get; set; } = true;

        public bool IsEmailVerified { get; set; } = false;

        public string? Password { get; set; }
    }
}
