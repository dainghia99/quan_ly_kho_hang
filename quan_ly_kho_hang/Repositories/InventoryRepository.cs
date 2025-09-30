using MongoDB.Driver;
using quan_ly_kho_hang.Data;
using quan_ly_kho_hang.Models;

namespace quan_ly_kho_hang.Repositories
{
    public class InventoryRepository : IInventoryRepository
    {
        private readonly IMongoCollection<Inventory> _inventories;

        public InventoryRepository(AppDbContext context)
        {
            _inventories = context.Inventories; 
        }

        public async Task<List<Inventory>> GetAllAsync(CancellationToken ct = default)
        {
            return await _inventories
                .Find(_ => true)
                .SortByDescending(i => i.CreatedAt)
                .ToListAsync(ct);
        }

        public async Task<Inventory?> GetByIdAsync(string id, CancellationToken ct = default)
        {
            return await _inventories.Find(i => i.Id == id).FirstOrDefaultAsync(ct);
        }

        public async Task CreateAsync(Inventory inventory, CancellationToken ct = default)
        {
            await _inventories.InsertOneAsync(inventory, null, ct);
        }

        public async Task UpdateAsync(Inventory inventory, CancellationToken ct = default)
        {
            await _inventories.ReplaceOneAsync(i => i.Id == inventory.Id, inventory, new ReplaceOptions(), ct);
        }

        public async Task DeleteAsync(string id, CancellationToken ct = default)
        {
            await _inventories.DeleteOneAsync(i => i.Id == id, ct);
        }
    }
}
