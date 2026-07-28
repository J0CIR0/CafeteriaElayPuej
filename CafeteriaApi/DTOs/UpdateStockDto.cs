using System.ComponentModel.DataAnnotations;

namespace CafeteriaApi.DTOs
{
    public class UpdateStockDto
    {
        [Required]
        public int NewStock { get; set; }

        public string? Reason { get; set; }
    }
}