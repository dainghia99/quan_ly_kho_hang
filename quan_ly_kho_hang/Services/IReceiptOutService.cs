using quan_ly_kho_hang.Models;

namespace quan_ly_kho_hang.Services
{
    public interface IReceiptOutService
    {
        Task<List<ReceiptOut>> GetAllAsync();
        Task<ReceiptOut?> GetByIdAsync(string id);
        Task<bool> CreateAsync(ReceiptOut receipt);
        Task UpdateAsync(string id, ReceiptOut receipt);
        Task DeleteAsync(string id);
    }
}
