using quan_ly_kho_hang.Models.Reports;

namespace quan_ly_kho_hang.Services.Reports
{
    public interface IReportService
    {
        Task<IEnumerable<SalesSummaryDto>> GetSalesSummaryAsync(DateTime from, DateTime to, string groupBy = "day");
        Task<IEnumerable<TopProductDto>> GetTopSellingProductsAsync(DateTime from, DateTime to, int limit = 10);
        Task<IEnumerable<PurchaseSummaryDto>> GetPurchaseSummaryAsync(DateTime from, DateTime to, string groupBy = "day");
        Task<IEnumerable<InventoryValuationDto>> GetInventoryValuationAsync();
        Task<IEnumerable<LowStockDto>> GetLowStockAsync(int threshold = 10);
        Task<IEnumerable<InventoryDifferenceDto>> GetInventoryDifferencesAsync(string inventoryId);
        Task<IEnumerable<AuditLogDto>> GetAuditLogsAsync(DateTime from, DateTime to, int limit = 100);
    }
}
