using quan_ly_kho_hang.Models;
using quan_ly_kho_hang.Repositories;

namespace quan_ly_kho_hang.Services
{
    public class ReceiptOutService : IReceiptOutService
    {
        private readonly IReceiptOutRepository _receiptOutRepo;
        private readonly IProductRepository _productRepo;

        public ReceiptOutService(IReceiptOutRepository receiptOutRepo, IProductRepository productRepo)
        {
            _receiptOutRepo = receiptOutRepo;
            _productRepo = productRepo;
        }

        public async Task<List<ReceiptOut>> GetAllAsync() =>
            await _receiptOutRepo.GetAllAsync();

        public async Task<ReceiptOut?> GetByIdAsync(string id) =>
            await _receiptOutRepo.GetByIdAsync(id);

        public async Task<bool> CreateAsync(ReceiptOut receipt)
        {
            foreach (var item in receipt.Items)
            {
                var product = await _productRepo.GetByIdAsync(item.ProductId);
                if (product == null || product.Quantity < item.Quantity)
                    return false;

                product.Quantity -= item.Quantity;
                product.UpdatedAt = DateTime.UtcNow;
                await _productRepo.UpdateAsync(product);
            }

            await _receiptOutRepo.CreateAsync(receipt);
            return true;
        }

        public async Task UpdateAsync(string id, ReceiptOut receipt) =>
            await _receiptOutRepo.UpdateAsync(id, receipt);

        public async Task DeleteAsync(string id) =>
            await _receiptOutRepo.DeleteAsync(id);
    }
}
