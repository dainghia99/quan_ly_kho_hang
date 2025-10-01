namespace quan_ly_kho_hang.Models.Reports
{
    public class LowStockDto
    {
        public string ProductId { get; set; } = null!;
        public string ProductName { get; set; } = null!;
        public int Quantity { get; set; }
        public int Threshold { get; set; }
    }
}
