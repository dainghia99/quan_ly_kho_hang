using MongoDB.Driver;
using quan_ly_kho_hang.Data;
using quan_ly_kho_hang.Models;

namespace quan_ly_kho_hang.Repositories
{
    public class ReceiptOutRepository : IReceiptOutRepository
    {
        private readonly IMongoCollection<ReceiptOut> _receiptOuts;

        public ReceiptOutRepository(AppDbContext context)
        {
            _receiptOuts = context.ReceiptOuts;
        }

        public async Task<List<ReceiptOut>> GetAllAsync() =>
            await _receiptOuts.Find(_ => true).ToListAsync();

        public async Task<ReceiptOut?> GetByIdAsync(string id) =>
            await _receiptOuts.Find(r => r.Id == id).FirstOrDefaultAsync();

        public async Task CreateAsync(ReceiptOut receipt) =>
            await _receiptOuts.InsertOneAsync(receipt);

        public async Task UpdateAsync(string id, ReceiptOut receipt) =>
            await _receiptOuts.ReplaceOneAsync(r => r.Id == id, receipt);

        public async Task DeleteAsync(string id) =>
            await _receiptOuts.DeleteOneAsync(r => r.Id == id);
    }
}
