using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using quan_ly_kho_hang.Models;
using quan_ly_kho_hang.Services;
using System.Security.Claims;

namespace quan_ly_kho_hang.Areas.ReceiptIns.Controllers
{
    [Area("ReceiptIns")]
    [Route("phieu-nhap/[action]/{id?}")]
    [Authorize(Roles = "Administrator,Editor")]
    public class ReceiptInsController : Controller
    {
        private readonly IReceiptInService _receiptService;
        private readonly IProductService _productService;

        public ReceiptInsController(IReceiptInService receiptService, IProductService productService)
        {
            _receiptService = receiptService;
            _productService = productService;
        }

        // GET: Danh sách phiếu nhập
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var receipts = await _receiptService.GetAllAsync();
            return View(receipts);
        }

        // GET: Tạo mới phiếu nhập
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Products = await _productService.GetAllAsync();
            return View(new ReceiptIn
            {
                
                Items = new List<ReceiptInItem>()
            });
        }

        // POST: Lưu phiếu nhập mới
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ReceiptIn receipt)
        {
            if (ModelState.IsValid)
            {
                
                var email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value
                            ?? User.Identity?.Name;

                receipt.CreatedByUserEmail = email;
                

                await _receiptService.AddAsync(receipt);
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Products = await _productService.GetAllAsync();
            return View(receipt);
        }

        // GET: Chi tiết phiếu nhập
        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            var receipt = await _receiptService.GetByIdAsync(id);
            if (receipt == null) return NotFound();
            return View(receipt);
        }

        // GET: Sửa phiếu nhập
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var receipt = await _receiptService.GetByIdAsync(id);
            if (receipt == null) return NotFound();

            ViewBag.Products = await _productService.GetAllAsync();
            return View(receipt);
        }

        // POST: Lưu sửa phiếu nhập
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, ReceiptIn receipt)
        {
            if (id != receipt.Id) return NotFound();

            if (ModelState.IsValid)
            {
                // ✅ Không cho sửa email người nhập
                var existing = await _receiptService.GetByIdAsync(id);
                if (existing != null)
                {
                    receipt.CreatedByUserEmail = existing.CreatedByUserEmail;
                }

                await _receiptService.UpdateAsync(id, receipt);
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Products = await _productService.GetAllAsync();
            return View(receipt);
        }

        // GET: Xóa phiếu nhập
        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            var receipt = await _receiptService.GetByIdAsync(id);
            if (receipt == null) return NotFound();
            return View(receipt);
        }

        // POST: Xác nhận xóa phiếu nhập
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            await _receiptService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
