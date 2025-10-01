using quan_ly_kho_hang.Models;

namespace quan_ly_kho_hang.Repositories
{
    public interface IInventoryRepository
    {
        Task<List<Inventory>> GetAllAsync(CancellationToken ct = default);
        Task<Inventory?> GetByIdAsync(string id, CancellationToken ct = default);
        Task CreateAsync(Inventory inventory, CancellationToken ct = default);
        Task UpdateAsync(Inventory inventory, CancellationToken ct = default);
        Task DeleteAsync(string id, CancellationToken ct = default);
    }
}
