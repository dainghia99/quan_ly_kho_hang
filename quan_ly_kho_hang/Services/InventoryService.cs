using MongoDB.Driver;
using quan_ly_kho_hang.Data;
using quan_ly_kho_hang.Models;
using quan_ly_kho_hang.Repositories;
using System.Text.Json;

namespace quan_ly_kho_hang.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly IInventoryRepository _inventoryRepo;
        private readonly IProductRepository _productRepo;

        public InventoryService(IInventoryRepository inventoryRepo, IProductRepository productRepo)
        {
            _inventoryRepo = inventoryRepo;
            _productRepo = productRepo;
        }

        public Task<List<Inventory>> GetAllAsync() => _inventoryRepo.GetAllAsync();
        public Task<Inventory?> GetByIdAsync(string id) => _inventoryRepo.GetByIdAsync(id);
        public Task CreateAsync(Inventory inv) => _inventoryRepo.CreateAsync(inv);
        public Task UpdateAsync(Inventory inv) => _inventoryRepo.UpdateAsync(inv);
        public Task DeleteAsync(string id) => _inventoryRepo.DeleteAsync(id);

        public async Task CompleteInventoryAsync(string id, string userId, string userEmail)
        {
            var inv = await _inventoryRepo.GetByIdAsync(id);
            if (inv == null) throw new Exception("Inventory not found");

            inv.Status = InventoryStatus.Completed;
            inv.CompletedAt = DateTime.UtcNow;
            inv.UpdatedAt = DateTime.UtcNow;
            await _inventoryRepo.UpdateAsync(inv);
        }

        public async Task ApplyAdjustmentsAsync(string id, string userId, string userEmail)
        {
            var inv = await _inventoryRepo.GetByIdAsync(id);
            if (inv == null) throw new Exception("Inventory not found");

            foreach (var item in inv.Items)
            {
                var product = await _productRepo.GetByIdAsync(item.ProductId);
                if (product != null)
                {
                    // cập nhật tồn kho bằng số lượng kiểm đếm
                    product.Quantity = item.CountedQuantity;
                    product.UpdatedAt = DateTime.UtcNow;
                    await _productRepo.UpdateAsync(product);
                }
            }

            inv.Status = InventoryStatus.Applied;
            inv.AppliedAt = DateTime.UtcNow;
            inv.UpdatedAt = DateTime.UtcNow;
            await _inventoryRepo.UpdateAsync(inv);
        }

        public async Task CancelAsync(string id, string userId, string userEmail)
        {
            var inv = await _inventoryRepo.GetByIdAsync(id);
            if (inv == null) throw new Exception("Inventory not found");

            inv.Status = InventoryStatus.Canceled;
            inv.UpdatedAt = DateTime.UtcNow;
            await _inventoryRepo.UpdateAsync(inv);
        }
    }
}
