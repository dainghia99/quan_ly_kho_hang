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
            foreach (var item in receipt.Items)
            {
                var product = await _productRepo.GetByIdAsync(item.ProductId);
                if (product != null)
                {
                    var oldQty = product.Quantity ?? 0;
                    var oldPrice = product.Price ?? 0;

                    var newQty = item.Quantity ?? 0;
                    var newPrice = item.UnitPrice;

                    var totalQty = oldQty + newQty;

                    // Tính giá vốn bình quân
                    decimal avgPrice = oldPrice;
                    if (totalQty > 0)
                    {
                        avgPrice = ((oldQty * oldPrice) + (newQty * newPrice)) / totalQty;
                    }

                    product.Quantity = totalQty;
                    product.Price = avgPrice;
                    product.UpdatedAt = DateTime.UtcNow;

                    await _productRepo.UpdateAsync(product);
                }
            }

            await _receiptRepo.AddAsync(receipt);
        }

        public async Task UpdateAsync(string id, ReceiptIn receipt)
        {
            var oldReceipt = await _receiptRepo.GetByIdAsync(id);
            if (oldReceipt == null) return;

            // Hoàn lại tồn kho cũ (trừ đi trước)
            foreach (var oldItem in oldReceipt.Items)
            {
                var product = await _productRepo.GetByIdAsync(oldItem.ProductId);
                if (product != null)
                {
                    product.Quantity = (product.Quantity ?? 0) - (oldItem.Quantity ?? 0);
                    if (product.Quantity < 0) product.Quantity = 0; // tránh âm

                    await _productRepo.UpdateAsync(product);
                }
            }

            // Thêm tồn kho mới với giá vốn bình quân
            foreach (var newItem in receipt.Items)
            {
                var product = await _productRepo.GetByIdAsync(newItem.ProductId);
                if (product != null)
                {
                    var oldQty = product.Quantity ?? 0;
                    var oldPrice = product.Price ?? 0;

                    var newQty = newItem.Quantity ?? 0;
                    var newPrice = newItem.UnitPrice;

                    var totalQty = oldQty + newQty;

                    decimal avgPrice = oldPrice;
                    if (totalQty > 0)
                    {
                        avgPrice = ((oldQty * oldPrice) + (newQty * newPrice)) / totalQty;
                    }

                    product.Quantity = totalQty;
                    product.Price = avgPrice;
                    product.UpdatedAt = DateTime.UtcNow;

                    await _productRepo.UpdateAsync(product);
                }
            }

            await _receiptRepo.UpdateAsync(id, receipt);
        }

        public async Task DeleteAsync(string id)
        {
            var receipt = await _receiptRepo.GetByIdAsync(id);
            if (receipt == null) return;

            // Khi xóa -> trừ tồn kho
            foreach (var item in receipt.Items)
            {
                var product = await _productRepo.GetByIdAsync(item.ProductId);
                if (product != null)
                {
                    product.Quantity = (product.Quantity ?? 0) - (item.Quantity ?? 0);
                    if (product.Quantity < 0) product.Quantity = 0;

                    await _productRepo.UpdateAsync(product);
                }
            }

            await _receiptRepo.DeleteAsync(id);
        }
    }
}
