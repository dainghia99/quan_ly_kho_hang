using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using quan_ly_kho_hang.Models;
using quan_ly_kho_hang.Services;

namespace quan_ly_kho_hang.Areas.ReceiptOuts.Controllers
{
    [Area("ReceiptOuts")]
    [Route("phieu-xuat/[action]/{id?}")]
    [Authorize(Roles = "Administrator, Editor")]
    public class ReceiptOutsController : Controller
    {
        private readonly IReceiptOutService _receiptOutService;
        private readonly IProductService _productService;

        public ReceiptOutsController(IReceiptOutService receiptOutService, IProductService productService)
        {
            _receiptOutService = receiptOutService;
            _productService = productService;
        }

        public async Task<IActionResult> Index()
        {
            var receipts = await _receiptOutService.GetAllAsync();
            return View(receipts);
        }

        public async Task<IActionResult> Details(string id)
        {
            var receipt = await _receiptOutService.GetByIdAsync(id);
            if (receipt == null) return NotFound();

            var products = await _productService.GetAllAsync();
            ViewBag.Products = products;
            return View(receipt);
        }

        public async Task<IActionResult> Create()
        {
            var products = await _productService.GetAllAsync();
            ViewBag.Products = products;

            var receipt = new ReceiptOut
            {
                CreatedByUserEmail = User.Identity?.Name // email user đang đăng nhập
            };

            return View(receipt);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ReceiptOut receipt)
        {
            // override email để tránh bị sửa trên client
            receipt.CreatedByUserEmail = User.Identity?.Name;

            if (ModelState.IsValid)
            {
                var success = await _receiptOutService.CreateAsync(receipt);
                if (!success)
                {
                    var products = await _productService.GetAllAsync();
                    ViewBag.Products = products;
                    ModelState.AddModelError("", "Không đủ tồn kho để xuất!");
                    return View(receipt);
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Products = await _productService.GetAllAsync();
            return View(receipt);
        }

        public async Task<IActionResult> Edit(string id)
        {
            var receipt = await _receiptOutService.GetByIdAsync(id);
            if (receipt == null) return NotFound();

            ViewBag.Products = await _productService.GetAllAsync();
            return View(receipt);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, ReceiptOut receipt)
        {
            if (ModelState.IsValid)
            {
                // không cho sửa email người tạo, giữ nguyên
                var oldReceipt = await _receiptOutService.GetByIdAsync(id);
                if (oldReceipt == null) return NotFound();

                receipt.CreatedByUserEmail = oldReceipt.CreatedByUserEmail;

                await _receiptOutService.UpdateAsync(id, receipt);
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Products = await _productService.GetAllAsync();
            return View(receipt);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            var receipt = await _receiptOutService.GetByIdAsync(id);
            if (receipt == null) return NotFound();
            return View(receipt);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            await _receiptOutService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
