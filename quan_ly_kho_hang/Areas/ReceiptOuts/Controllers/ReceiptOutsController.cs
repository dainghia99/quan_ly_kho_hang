using Microsoft.AspNetCore.Mvc;
using quan_ly_kho_hang.Models;
using quan_ly_kho_hang.Services;

namespace quan_ly_kho_hang.Areas.ReceiptOuts.Controllers
{
    [Area("ReceiptOuts")]
    [Route("phieu-xuat/[action]/{id?}")]
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

            // load danh sách product để map tên sản phẩm
            var products = await _productService.GetAllAsync();
            ViewBag.Products = products;
            return View(receipt);
        }

        public async Task<IActionResult> Create()
        {
            var products = await _productService.GetAllAsync();
            ViewBag.Products = products;
            return View(new ReceiptOut());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ReceiptOut receipt)
        {
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
                await _receiptOutService.UpdateAsync(id, receipt);
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Products = await _productService.GetAllAsync();
            return View(receipt);
        }

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
