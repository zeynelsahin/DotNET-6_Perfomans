using CarvedRock.Core;

namespace CarvedRock.Domain;

public interface IProductLogic 
{
    Task<IEnumerable<ProductModel>> GetProductsForCategoryAsync(string category,CancellationToken cancellationToken);
    Task<ProductModel?> GetProductByIdAsync(int id);
    Task<IEnumerable<ProductModel>> GetProductsListForCategoryAsync(string category);
    ProductModel? GetProductById(int id);
    Task<ProductModel> AddNewProductAsync(ProductModel productToAdd, bool invalidateCache);
}