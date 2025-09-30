using quan_ly_kho_hang.Models;

namespace quan_ly_kho_hang.Services
{
    public interface IInventoryService
    {
        Task<List<Inventory>> GetAllAsync();
        Task<Inventory?> GetByIdAsync(string id);
        Task CreateAsync(Inventory inv);
        Task UpdateAsync(Inventory inv);
        Task DeleteAsync(string id);
        Task CompleteInventoryAsync(string id, string userId, string userEmail);
        Task ApplyAdjustmentsAsync(string id, string userId, string userEmail);
        Task CancelAsync(string id, string userId, string userEmail);
    }
}
