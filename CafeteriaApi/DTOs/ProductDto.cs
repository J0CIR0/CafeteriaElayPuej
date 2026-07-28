namespace CafeteriaApi.DTOs
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string? PreparationTime { get; set; }
        public string? Origin { get; set; }
        public string? FlavorNotes { get; set; }
        public string? ImageUrl { get; set; }
        public int Stock { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int CategoryId { get; set; }
    }
}