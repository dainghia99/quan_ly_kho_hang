using quan_ly_kho_hang.Models.Reports;

namespace quan_ly_kho_hang.Repositories.Reports
{
    public interface IReportRepository
    {
        Task<IEnumerable<SalesSummaryDto>> GetSalesSummaryAsync(DateTime from, DateTime to, string groupBy); // groupBy: "day","month"
        Task<IEnumerable<TopProductDto>> GetTopSellingProductsAsync(DateTime from, DateTime to, int limit = 10);
        Task<IEnumerable<PurchaseSummaryDto>> GetPurchaseSummaryAsync(DateTime from, DateTime to, string groupBy);
        Task<IEnumerable<InventoryValuationDto>> GetInventoryValuationAsync();
        Task<IEnumerable<LowStockDto>> GetLowStockAsync(int defaultThreshold = 10);
        Task<IEnumerable<InventoryDifferenceDto>> GetInventoryDifferencesAsync(string inventoryId);
        Task<IEnumerable<AuditLogDto>> GetAuditLogsAsync(DateTime from, DateTime to, int limit = 100);
    }
}
