using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using quan_ly_kho_hang.Models;
using quan_ly_kho_hang.Services;

namespace quan_ly_kho_hang.Areas.Inventories.Controllers
{
    [Area("Inventories")]
    [Route("kiem-ke/[action]/{id?}")]
    public class InventoriesController : Controller
    {
        private readonly IInventoryService _inventoryService;
        private readonly IProductService _productService;
        private readonly UserManager<AppUser> _userManager;

        public InventoriesController(
            IInventoryService inventoryService,
            IProductService productService,
            UserManager<AppUser> userManager)
        {
            _inventoryService = inventoryService;
            _productService = productService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var list = await _inventoryService.GetAllAsync();
            return View(list);
        }

        public async Task<IActionResult> Details(string id)
        {
            var inv = await _inventoryService.GetByIdAsync(id);
            if (inv == null) return NotFound();
            return View(inv);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Products = await _productService.GetAllAsync();
            return View(new Inventory());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Inventory model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Products = await _productService.GetAllAsync();
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            model.CreatedByUserId = user?.Id.ToString();
            model.CreatedByUserEmail = user?.Email;

            await _inventoryService.CreateAsync(model);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(string id)
        {
            var inv = await _inventoryService.GetByIdAsync(id);
            if (inv == null) return NotFound();

            ViewBag.Products = await _productService.GetAllAsync();
            return View(inv);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, Inventory model)
        {
            if (id != model.Id) return BadRequest();
            if (!ModelState.IsValid)
            {
                ViewBag.Products = await _productService.GetAllAsync();
                return View(model);
            }

            await _inventoryService.UpdateAsync(model);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(string id)
        {
            var inv = await _inventoryService.GetByIdAsync(id);
            if (inv == null) return NotFound();
            return View(inv);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            await _inventoryService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(string id)
        {
            var user = await _userManager.GetUserAsync(User);
            string userId = user?.Id.ToString() ?? string.Empty;
            string userEmail = user?.Email ?? string.Empty;

            await _inventoryService.CompleteInventoryAsync(id, userId, userEmail);
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Apply(string id)
        {
            var user = await _userManager.GetUserAsync(User);
            string userId = user?.Id.ToString() ?? string.Empty;
            string userEmail = user?.Email ?? string.Empty;

            await _inventoryService.ApplyAdjustmentsAsync(id, userId, userEmail);
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(string id)
        {
            var user = await _userManager.GetUserAsync(User);
            string userId = user?.Id.ToString() ?? string.Empty;
            string userEmail = user?.Email ?? string.Empty;

            await _inventoryService.CancelAsync(id, userId, userEmail);
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
