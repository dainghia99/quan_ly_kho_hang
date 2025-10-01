namespace quan_ly_kho_hang.Models.Reports
{
    public class PurchaseSummaryDto
    {
        public DateTime Period { get; set; }
        public int TotalQuantity { get; set; }
        public decimal TotalCost { get; set; }
    }
}
