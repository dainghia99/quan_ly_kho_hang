using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using quan_ly_kho_hang.Services.Reports;
using ClosedXML.Excel;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Globalization;

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

        private TimeZoneInfo GetVietnamTimeZone()
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"); // Windows
            }
            catch
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Asia/Bangkok"); // Linux/Mac
            }
        }

        private void ParseDateRange(string fromStr, string toStr, out DateTime fromUtc, out DateTime toUtc)
        {
            var tz = GetVietnamTimeZone();

            DateTime fromLocal, toLocal;
            if (!string.IsNullOrWhiteSpace(fromStr) &&
                DateTime.TryParseExact(fromStr, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var tmpFrom))
            {
                fromLocal = tmpFrom.Date;
            }
            else
            {
                fromLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz).AddMonths(-1).Date;
            }

            if (!string.IsNullOrWhiteSpace(toStr) &&
                DateTime.TryParseExact(toStr, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var tmpTo))
            {
                toLocal = tmpTo.Date.AddDays(1).AddTicks(-1);
            }
            else
            {
                toLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz).Date.AddDays(1).AddTicks(-1);
            }

            fromUtc = TimeZoneInfo.ConvertTimeToUtc(fromLocal, tz);
            toUtc = TimeZoneInfo.ConvertTimeToUtc(toLocal, tz);
        }

        // Dashboard
        public async Task<IActionResult> Index()
        {
            var tz = GetVietnamTimeZone();
            var to = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
            var from = to.AddMonths(-1);
            var sales = await _reportService.GetSalesSummaryAsync(from, to, "month");
            var top = await _reportService.GetTopSellingProductsAsync(from, to, 5);
            var low = await _reportService.GetLowStockAsync(10);

            ViewBag.Sales = sales;
            ViewBag.TopProducts = top;
            ViewBag.LowStock = low;

            return View();
        }

        // Sales page
        [HttpGet]
        public async Task<IActionResult> Sales(string from, string to, string groupBy = "day")
        {
            ParseDateRange(from, to, out var fromUtc, out var toUtc);

            var data = await _reportService.GetSalesSummaryAsync(fromUtc, toUtc, groupBy);

            var tz = GetVietnamTimeZone();
            ViewBag.From = TimeZoneInfo.ConvertTimeFromUtc(fromUtc, tz).Date;
            ViewBag.To = TimeZoneInfo.ConvertTimeFromUtc(toUtc, tz).Date;
            ViewBag.GroupBy = groupBy;

            return View(data);
        }

        [HttpGet]
        public async Task<IActionResult> ExportSalesToExcel(string from, string to, string groupBy = "day")
        {
            ParseDateRange(from, to, out var fromUtc, out var toUtc);
            var data = (await _reportService.GetSalesSummaryAsync(fromUtc, toUtc, groupBy)).ToList();

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("SalesReport");
            ws.Cell(1, 1).Value = "Ngày/Tháng";
            ws.Cell(1, 2).Value = "Số lượng";
            ws.Cell(1, 3).Value = "Doanh thu";

            for (int i = 0; i < data.Count; i++)
            {
                var row = i + 2;
                ws.Cell(row, 1).Value = groupBy == "month"
                    ? data[i].Period.ToString("yyyy-MM")
                    : data[i].Period.ToString("yyyy-MM-dd");
                ws.Cell(row, 2).Value = data[i].TotalQuantity;
                ws.Cell(row, 3).Value = data[i].TotalRevenue;
            }

            using var ms = new MemoryStream();
            workbook.SaveAs(ms);
            ms.Position = 0;
            return File(ms.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "SalesReport.xlsx");
        }

        [HttpGet]
        public async Task<IActionResult> ExportSalesToPdf(string from, string to, string groupBy = "day")
        {
            ParseDateRange(from, to, out var fromUtc, out var toUtc);
            var data = (await _reportService.GetSalesSummaryAsync(fromUtc, toUtc, groupBy)).ToList();

            using var ms = new MemoryStream();
            var doc = new Document(PageSize.A4);
            PdfWriter.GetInstance(doc, ms);
            doc.Open();

            doc.Add(new Paragraph("BÁO CÁO BÁN HÀNG"));
            var tz = GetVietnamTimeZone();
            var fromLocal = TimeZoneInfo.ConvertTimeFromUtc(fromUtc, tz);
            var toLocal = TimeZoneInfo.ConvertTimeFromUtc(toUtc, tz);
            doc.Add(new Paragraph($"Từ: {fromLocal:yyyy-MM-dd} - Đến: {toLocal:yyyy-MM-dd}"));
            doc.Add(new Paragraph("\n"));

            var table = new PdfPTable(3);
            table.AddCell("Ngày/Tháng");
            table.AddCell("Số lượng");
            table.AddCell("Doanh thu");

            foreach (var r in data)
            {
                table.AddCell(groupBy == "month"
                    ? r.Period.ToString("yyyy-MM")
                    : r.Period.ToString("yyyy-MM-dd"));
                table.AddCell(r.TotalQuantity.ToString());
                table.AddCell(r.TotalRevenue.ToString("N0"));
            }

            doc.Add(table);
            doc.Close();

            return File(ms.ToArray(), "application/pdf", "SalesReport.pdf");
        }

        [HttpGet]
        public async Task<IActionResult> InventoryValuation()
        {
            var data = await _reportService.GetInventoryValuationAsync();
            return View(data);
        }
    }
}
