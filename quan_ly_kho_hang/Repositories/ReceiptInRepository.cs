using MongoDB.Driver;
using quan_ly_kho_hang.Data;
using quan_ly_kho_hang.Models;

namespace quan_ly_kho_hang.Repositories
{
    public class ReceiptInRepository : IReceiptInRepository
    {
        private readonly IMongoCollection<ReceiptIn> _receiptCollection;

        public ReceiptInRepository(AppDbContext context)
        {
            _receiptCollection = context.ReceiptIns;
        }

        public async Task<IEnumerable<ReceiptIn>> GetAllAsync()
        {
            return await _receiptCollection.Find(_ => true).ToListAsync();
        }

        public async Task<ReceiptIn> GetByIdAsync(string id)
        {
            return await _receiptCollection.Find(r => r.Id == id).FirstOrDefaultAsync();
        }

        public async Task AddAsync(ReceiptIn receipt)
        {
            await _receiptCollection.InsertOneAsync(receipt);
        }

        public async Task UpdateAsync(string id, ReceiptIn receipt)
        {
            await _receiptCollection.ReplaceOneAsync(r => r.Id == id, receipt);
        }

        public async Task DeleteAsync(string id)
        {
            await _receiptCollection.DeleteOneAsync(r => r.Id == id);
        }
    }
}
