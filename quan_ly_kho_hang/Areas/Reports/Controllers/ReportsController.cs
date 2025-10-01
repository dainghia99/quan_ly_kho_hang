using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using quan_ly_kho_hang.Services.Reports;
using ClosedXML.Excel;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace quan_ly_kho_hang.Areas.Reports.Controllers
{
    [Area("Reports")]
    [Authorize(Roles = "Administrator,Editor")]
    [Route("bao-cao/[action]/{id?}")]
    public class ReportsController : Controller
    {
        private readonly IReportService _reportService;
        public ReportsController(IReportService reportService)
        {
            _reportService = reportService;
        }

        // Dashboard: summary widgets
        public async Task<IActionResult> Index()
        {
            var to = DateTime.UtcNow;
            var from = to.AddMonths(-1);
            var sales = await _reportService.GetSalesSummaryAsync(from, to, "month");
            var top = await _reportService.GetTopSellingProductsAsync(from, to, 5);
            var low = await _reportService.GetLowStockAsync(10);

            ViewBag.Sales = sales;
            ViewBag.TopProducts = top;
            ViewBag.LowStock = low;

            return View();
        }

        // Sales listing / chart page
        public async Task<IActionResult> Sales(DateTime? from, DateTime? to, string groupBy = "day")
        {
            var f = from ?? DateTime.UtcNow.AddMonths(-1);
            var t = to ?? DateTime.UtcNow;
            var data = await _reportService.GetSalesSummaryAsync(f, t, groupBy);
            return View(data);
        }

        public async Task<IActionResult> TopProducts(DateTime? from, DateTime? to, int limit = 10)
        {
            var f = from ?? DateTime.UtcNow.AddMonths(-1);
            var t = to ?? DateTime.UtcNow;
            var data = await _reportService.GetTopSellingProductsAsync(f, t, limit);
            return View(data);
        }

        public async Task<IActionResult> Purchases(DateTime? from, DateTime? to, string groupBy = "day")
        {
            var f = from ?? DateTime.UtcNow.AddMonths(-1);
            var t = to ?? DateTime.UtcNow;
            var data = await _reportService.GetPurchaseSummaryAsync(f, t, groupBy);
            return View(data);
        }

        public async Task<IActionResult> InventoryValuation()
        {
            var data = await _reportService.GetInventoryValuationAsync();
            return View(data);
        }

        public async Task<IActionResult> LowStock(int threshold = 10)
        {
            var data = await _reportService.GetLowStockAsync(threshold);
            return View(data);
        }

        public async Task<IActionResult> InventoryDifferences(string id)
        {
            var data = await _reportService.GetInventoryDifferencesAsync(id);
            return View(data);
        }

        public async Task<IActionResult> AuditLogs(DateTime? from, DateTime? to, int limit = 200)
        {
            var f = from ?? DateTime.UtcNow.AddMonths(-1);
            var t = to ?? DateTime.UtcNow;
            var data = await _reportService.GetAuditLogsAsync(f, t, limit);
            return View(data);
        }

        // ✅ Export Sales Report to Excel
        public async Task<IActionResult> ExportSalesToExcel(DateTime? from, DateTime? to, string groupBy = "day")
        {
            var f = from ?? DateTime.UtcNow.AddMonths(-1);
            var t = to ?? DateTime.UtcNow;
            var data = (await _reportService.GetSalesSummaryAsync(f, t, groupBy)).ToList();

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("SalesReport");
            ws.Cell(1, 1).Value = "Ngày/Tháng";
            ws.Cell(1, 2).Value = "Số lượng";
            ws.Cell(1, 3).Value = "Doanh thu";

            for (int i = 0; i < data.Count; i++)
            {
                var row = i + 2;
                ws.Cell(row, 1).Value = data[i].Period.ToString("yyyy-MM-dd");
                ws.Cell(row, 2).Value = data[i].TotalQuantity;
                ws.Cell(row, 3).Value = data[i].TotalRevenue;
            }

            using var ms = new MemoryStream();
            workbook.SaveAs(ms);
            ms.Position = 0;
            return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "SalesReport.xlsx");
        }

        // ✅ Export Sales Report to PDF
        public async Task<IActionResult> ExportSalesToPdf(DateTime? from, DateTime? to, string groupBy = "day")
        {
            var f = from ?? DateTime.UtcNow.AddMonths(-1);
            var t = to ?? DateTime.UtcNow;
            var data = (await _reportService.GetSalesSummaryAsync(f, t, groupBy)).ToList();

            using var ms = new MemoryStream();
            var doc = new Document(PageSize.A4);
            PdfWriter.GetInstance(doc, ms);
            doc.Open();

            doc.Add(new Paragraph("BÁO CÁO BÁN HÀNG"));
            doc.Add(new Paragraph($"Từ: {f:yyyy-MM-dd} - Đến: {t:yyyy-MM-dd}"));
            doc.Add(new Paragraph("\n"));

            var table = new PdfPTable(3);
            table.AddCell("Ngày/Tháng");
            table.AddCell("Số lượng");
            table.AddCell("Doanh thu");

            foreach (var r in data)
            {
                table.AddCell(r.Period.ToString("yyyy-MM-dd"));
                table.AddCell(r.TotalQuantity.ToString());
                table.AddCell(r.TotalRevenue.ToString("N0"));
            }

            doc.Add(table);
            doc.Close();

            return File(ms.ToArray(), "application/pdf", "SalesReport.pdf");
        }

        // Ví dụ Export khác: Top products
        public async Task<IActionResult> ExportTopProductsToExcel(DateTime? from, DateTime? to, int limit = 100)
        {
            var f = from ?? DateTime.UtcNow.AddMonths(-1);
            var t = to ?? DateTime.UtcNow;
            var data = (await _reportService.GetTopSellingProductsAsync(f, t, limit)).ToList();

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("TopProducts");
            ws.Cell(1, 1).Value = "ProductId";
            ws.Cell(1, 2).Value = "ProductName";
            ws.Cell(1, 3).Value = "TotalSold";
            ws.Cell(1, 4).Value = "TotalRevenue";
            for (int i = 0; i < data.Count; i++)
            {
                var row = i + 2;
                ws.Cell(row, 1).Value = data[i].ProductId;
                ws.Cell(row, 2).Value = data[i].ProductName;
                ws.Cell(row, 3).Value = data[i].TotalSold;
                ws.Cell(row, 4).Value = data[i].TotalRevenue;
            }
            using var ms = new MemoryStream();
            workbook.SaveAs(ms);
            ms.Position = 0;
            return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "TopProducts.xlsx");
        }
    }
}
