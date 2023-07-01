using CarvedRock.Data.Entities;

namespace CarvedRock.Data
{
    public interface ICarvedRockRepository
    {
        Task<List<Product>> GetProductsAsync(string category, CancellationToken cancellationToken);
        Task<Product?> GetProductByIdAsync(int id);
        Task<List<Product>> GetProductsListAsync(string category);
        Product? GetProductById(int id);
        Task<Product> AddNewProductAsync(Product product, bool invalidateCache);
    }
}