namespace quan_ly_kho_hang.Models.Reports
{
    public class SalesSummaryDto
    {
        public DateTime Period { get; set; } // day/month depending on group
        public int TotalQuantity { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class TopProductDto
    {
        public string ProductId { get; set; } = null!;
        public string ProductName { get; set; } = null!;
        public int TotalSold { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
