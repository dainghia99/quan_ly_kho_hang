using quan_ly_kho_hang.Models;
using quan_ly_kho_hang.Repositories;

namespace quan_ly_kho_hang.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repository;

        public ProductService(IProductRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<Product>> GetAllAsync() => await _repository.GetAllAsync();

        public async Task<Product?> GetByIdAsync(string id) => await _repository.GetByIdAsync(id);

        public async Task CreateAsync(Product product) => await _repository.CreateAsync(product);

        public async Task UpdateAsync(Product product) => await _repository.UpdateAsync(product);

        public async Task DeleteAsync(string id) => await _repository.DeleteAsync(id);
    }
}
