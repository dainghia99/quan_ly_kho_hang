using quan_ly_kho_hang.Models.Reports;
using quan_ly_kho_hang.Repositories.Reports;

namespace quan_ly_kho_hang.Services.Reports
{
    public class ReportService : IReportService
    {
        private readonly IReportRepository _repo;

        public ReportService(IReportRepository repo)
        {
            _repo = repo;
        }

        public Task<IEnumerable<SalesSummaryDto>> GetSalesSummaryAsync(DateTime from, DateTime to, string groupBy = "day")
            => _repo.GetSalesSummaryAsync(from, to, groupBy);

        public Task<IEnumerable<TopProductDto>> GetTopSellingProductsAsync(DateTime from, DateTime to, int limit = 10)
            => _repo.GetTopSellingProductsAsync(from, to, limit);

        public Task<IEnumerable<PurchaseSummaryDto>> GetPurchaseSummaryAsync(DateTime from, DateTime to, string groupBy = "day")
            => _repo.GetPurchaseSummaryAsync(from, to, groupBy);

        public Task<IEnumerable<InventoryValuationDto>> GetInventoryValuationAsync()
            => _repo.GetInventoryValuationAsync();

        public Task<IEnumerable<LowStockDto>> GetLowStockAsync(int threshold = 10)
            => _repo.GetLowStockAsync(threshold);

        public Task<IEnumerable<InventoryDifferenceDto>> GetInventoryDifferencesAsync(string inventoryId)
            => _repo.GetInventoryDifferencesAsync(inventoryId);

        public Task<IEnumerable<AuditLogDto>> GetAuditLogsAsync(DateTime from, DateTime to, int limit = 100)
            => _repo.GetAuditLogsAsync(from, to, limit);
    }
}
