using quan_ly_kho_hang.Models;
using quan_ly_kho_hang.Repositories;

namespace quan_ly_kho_hang.Services
{
    public class ReceiptInService : IReceiptInService
    {
        private readonly IReceiptInRepository _receiptRepo;
        private readonly IProductRepository _productRepo;

        public ReceiptInService(IReceiptInRepository receiptRepo, IProductRepository productRepo)
        {
            _receiptRepo = receiptRepo;
            _productRepo = productRepo;
        }

        public async Task<IEnumerable<ReceiptIn>> GetAllAsync()
        {
            return await _receiptRepo.GetAllAsync();
        }

        public async Task<ReceiptIn> GetByIdAsync(string id)
        {
            return await _receiptRepo.GetByIdAsync(id);
        }

        public async Task AddAsync(ReceiptIn receipt)
        {
            // Cập nhật tồn kho của sản phẩm khi nhập kho
            foreach (var item in receipt.Items)
            {
                var product = await _productRepo.GetByIdAsync(item.ProductId);
                if (product != null)
                {
                    product.Quantity += item.Quantity;
                    await _productRepo.UpdateAsync(product);
                }
            }

            await _receiptRepo.AddAsync(receipt);
        }

        public async Task UpdateAsync(string id, ReceiptIn receipt)
        {
            await _receiptRepo.UpdateAsync(id, receipt);
        }

        public async Task DeleteAsync(string id)
        {
            await _receiptRepo.DeleteAsync(id);
        }
    }
}
