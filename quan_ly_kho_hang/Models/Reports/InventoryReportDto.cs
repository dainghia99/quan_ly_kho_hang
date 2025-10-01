namespace quan_ly_kho_hang.Models.Reports
{
    public class InventoryValuationDto
    {
        public string ProductId { get; set; } = null!;
        public string ProductName { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalValue => UnitPrice * Quantity;
    }

    public class InventoryDifferenceDto
    {
        public string InventoryId { get; set; } = null!;
        public string ProductId { get; set; } = null!;
        public string ProductName { get; set; } = null!;
        public int ExpectedQuantity { get; set; }
        public int CountedQuantity { get; set; }
        public int Difference => CountedQuantity - ExpectedQuantity;
    }
}
