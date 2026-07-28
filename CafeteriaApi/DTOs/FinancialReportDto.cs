namespace CafeteriaApi.DTOs
{
    public class ProductProfitabilityDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public decimal SalePrice { get; set; }
        public decimal UnitCost { get; set; }
        public int TotalQuantitySold { get; set; }
        public decimal TotalRevenue => SalePrice * TotalQuantitySold;
        public decimal TotalCost => UnitCost * TotalQuantitySold;
        public decimal TotalProfit => TotalRevenue - TotalCost;
        public decimal ProfitMarginPercent => TotalRevenue > 0 ? ((TotalRevenue - TotalCost) / TotalRevenue) * 100 : 0;
    }

    public class ExpiringIngredientDto
    {
        public int IngredientId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal StockQuantity { get; set; }
        public string UnitOfMeasure { get; set; } = string.Empty;
        public DateTime ExpirationDate { get; set; }
        public int DaysUntilExpiration => (ExpirationDate.Date - DateTime.UtcNow.Date).Days;
    }

    public class WasteLossDto
    {
        public int Id { get; set; }
        public string IngredientName { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string UnitOfMeasure { get; set; } = string.Empty;
        public decimal UnitCostAtTime { get; set; }
        public decimal TotalCostLoss { get; set; }
        public string Reason { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class FinancialSummaryDto
    {
        public decimal TotalRevenue { get; set; }
        public decimal TotalProductionCost { get; set; }
        public decimal TotalNetProfit { get; set; }
        public decimal TotalWasteLossCost { get; set; }
        public List<ProductProfitabilityDto> ProductProfitability { get; set; } = new List<ProductProfitabilityDto>();
        public List<ExpiringIngredientDto> ExpiringIngredients { get; set; } = new List<ExpiringIngredientDto>();
        public List<WasteLossDto> WasteLosses { get; set; } = new List<WasteLossDto>();
    }
}
