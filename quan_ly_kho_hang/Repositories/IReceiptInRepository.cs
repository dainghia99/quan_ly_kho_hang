using quan_ly_kho_hang.Models;

namespace quan_ly_kho_hang.Repositories
{
    public interface IReceiptInRepository
    {
        Task<IEnumerable<ReceiptIn>> GetAllAsync();
        Task<ReceiptIn> GetByIdAsync(string id);
        Task AddAsync(ReceiptIn receipt);
        Task UpdateAsync(string id, ReceiptIn receipt);
        Task DeleteAsync(string id);
    }
}
