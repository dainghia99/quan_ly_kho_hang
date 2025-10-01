using MongoDB.Driver;
using MongoDB.Driver.Linq;
using quan_ly_kho_hang.Data;
using quan_ly_kho_hang.Models.Reports;
using quan_ly_kho_hang.Repositories.Reports;

namespace quan_ly_kho_hang.Services.Reports
{
    public class ReportService : IReportService
    {
        private readonly IReportRepository _repo;
        private readonly AppDbContext _context;

        public ReportService(IReportRepository repo, AppDbContext context)
        {
            _repo = repo;
            _context = context;
        }

        public async Task<IEnumerable<SalesSummaryDto>> GetSalesSummaryAsync(DateTime from, DateTime to, string groupBy)
        {
            
            var query = _context.ReceiptOuts.AsQueryable()
                .Where(r => r.CreatedAt >= from && r.CreatedAt <= to)
                .SelectMany(r => r.Items, (r, d) => new
                {
                    r.CreatedAt,
                    d.Quantity,
                    d.UnitPrice
                });

            var result = query
                .GroupBy(x => groupBy == "month"
                    ? new DateTime(x.CreatedAt.Year, x.CreatedAt.Month, 1)
                    : x.CreatedAt.Date)
                .Select(g => new SalesSummaryDto
                {
                    Period = g.Key,
                    TotalQuantity = g.Sum(x => x.Quantity ?? 0),
                    TotalRevenue = g.Sum(x => (x.Quantity ?? 0) * x.UnitPrice)
                })
                .OrderBy(x => x.Period)
                .ToList();

            return await Task.FromResult(result);
        }

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
