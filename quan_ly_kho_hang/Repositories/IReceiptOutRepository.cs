using quan_ly_kho_hang.Models;

namespace quan_ly_kho_hang.Repositories
{
    public interface IReceiptOutRepository
    {
        Task<List<ReceiptOut>> GetAllAsync();
        Task<ReceiptOut?> GetByIdAsync(string id);
        Task CreateAsync(ReceiptOut receipt);
        Task UpdateAsync(string id, ReceiptOut receipt);
        Task DeleteAsync(string id);
    }
}
