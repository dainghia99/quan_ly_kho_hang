using MongoDB.Driver;
using quan_ly_kho_hang.Data;
using quan_ly_kho_hang.Models;

namespace quan_ly_kho_hang.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly AppDbContext _context;

        public ProductRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _context.Products.Find(_ => true)
                                          .SortByDescending(p => p.CreatedAt)
                                          .ToListAsync();
        }

        public async Task<Product?> GetByIdAsync(string id)
        {
            return await _context.Products.Find(p => p.Id == id).FirstOrDefaultAsync();
        }

        public async Task CreateAsync(Product product)
        {
            await _context.Products.InsertOneAsync(product);
        }

        public async Task UpdateAsync(Product product)
        {
            product.UpdatedAt = DateTime.UtcNow;
            await _context.Products.ReplaceOneAsync(p => p.Id == product.Id, product);
        }

        public async Task DeleteAsync(string id)
        {
            await _context.Products.DeleteOneAsync(p => p.Id == id);
        }
    }
}
