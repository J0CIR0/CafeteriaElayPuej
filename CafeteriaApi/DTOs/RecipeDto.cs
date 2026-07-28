namespace CafeteriaApi.DTOs
{
    public class RecipeIngredientItemDto
    {
        public int IngredientId { get; set; }
        public string IngredientName { get; set; } = string.Empty;
        public string UnitOfMeasure { get; set; } = string.Empty;
        public decimal QuantityRequired { get; set; }
        public decimal UnitCost { get; set; }
        public decimal SubtotalCost => QuantityRequired * UnitCost;
    }

    public class ProductRecipeDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal SalePrice { get; set; }
        public decimal EstimatedCost { get; set; }
        public decimal NetProfit => SalePrice - EstimatedCost;
        public decimal ProfitMarginPercent => SalePrice > 0 ? ((SalePrice - EstimatedCost) / SalePrice) * 100 : 0;
        public List<RecipeIngredientItemDto> Ingredients { get; set; } = new List<RecipeIngredientItemDto>();
    }

    public class SaveRecipeItemDto
    {
        public int IngredientId { get; set; }
        public decimal QuantityRequired { get; set; }
    }

    public class SaveRecipeDto
    {
        public List<SaveRecipeItemDto> Ingredients { get; set; } = new List<SaveRecipeItemDto>();
    }
}
