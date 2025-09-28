using Microsoft.AspNetCore.Mvc;
using quan_ly_kho_hang.Services;
using quan_ly_kho_hang.Models;
using Microsoft.AspNetCore.Authorization;

namespace quan_ly_kho_hang.Areas.Products.Controllers
{
    [Area("Products")]
    [Route("product/[action]/{id?}")]
    [Authorize(Roles = "Administrator,Editor")]
    public class ProductsController : Controller
    {
        private readonly IProductService _service;
        private readonly IWebHostEnvironment _env;

        public ProductsController(IProductService service, IWebHostEnvironment env)
        {
            _service = service;
            _env = env;
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var products = await _service.GetAllAsync();
            return View(products);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    var fileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
                    var path = Path.Combine(_env.WebRootPath, "images/products", fileName);

                    Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }

                    product.ImagePath = "/images/products/" + fileName;
                }

                await _service.CreateAsync(product);
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        public async Task<IActionResult> Edit(string id)
        {
            var product = await _service.GetByIdAsync(id);
            if (product == null) return NotFound();
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, Product product, IFormFile? imageFile)
        {
            if (id != product.Id) return NotFound();

            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    var fileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
                    var path = Path.Combine(_env.WebRootPath, "images/products", fileName);

                    Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }

                    product.ImagePath = "/images/products/" + fileName;
                }
                else
                {
                    var existing = await _service.GetByIdAsync(id);
                    if (existing != null)
                    {
                        product.ImagePath = existing.ImagePath;
                    }
                }

                await _service.UpdateAsync(product);
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        public async Task<IActionResult> Delete(string id)
        {
            var product = await _service.GetByIdAsync(id);
            if (product == null) return NotFound();
            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            await _service.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
