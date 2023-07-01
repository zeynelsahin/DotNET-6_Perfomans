using System.Diagnostics;
using CarvedRock.Core;
using CarvedRock.Data;
using CarvedRock.Data.Entities;
using Microsoft.Extensions.Logging;

namespace CarvedRock.Domain;

public class ProductLogic : IProductLogic
{
    private readonly ILogger<ProductLogic> _logger;
    private readonly ICarvedRockRepository _repo;
    private readonly IExtraLogic _extraLogic;
    public ProductLogic(ILogger<ProductLogic> logger, ICarvedRockRepository repo,IExtraLogic extraLogic)
    {
        _logger = logger;
        _repo = repo;
        _extraLogic = extraLogic;
    }
    public async Task<IEnumerable<ProductModel>> GetProductsForCategoryAsync(string category, CancellationToken cancellationToken)
    {               
        _logger.LogInformation("Getting products in logic for {category}", category);

        Activity.Current?.AddEvent(new ActivityEvent("Getting products from repository"));

        var products = await _repo.GetProductsAsync(category, cancellationToken);

        _logger.LogInformation("About to make extra async calls");
        var invTask = _extraLogic.GetInventoryForProductsAsync(products.Select(p => p.Id).ToList(),cancellationToken);
        var promotionTask= _extraLogic.GetPromotionForProductsAsync(products.Select(p => p.Id).ToList(),cancellationToken);

        await Task.WhenAll(invTask, promotionTask);

        var inventory = await invTask;
        var promotion= await promotionTask;

        var results = new List<ProductModel>();
        foreach (var product in products)
        {
            var productToAdd = ConvertToProductModel(product);
            results.Add(productToAdd);
        }

        Activity.Current?.AddEvent(new ActivityEvent("Retrieved products from repository"));

        return results;
    }

    public async Task<ProductModel?> GetProductByIdAsync(int id)
    {
        var product = await _repo.GetProductByIdAsync(id);
        return product != null ? ConvertToProductModel(product) : null;
    }

    public async Task<IEnumerable<ProductModel>> GetProductsListForCategoryAsync(string category)
    {
        var products =  await _repo.GetProductsListAsync(category);

        var results = new List<ProductModel>();
        foreach (var product in products)
        {
            var productToAdd = ConvertToProductModel(product);
            results.Add(productToAdd);
        }

        return results;
    }

    public ProductModel? GetProductById(int id)
    {
        var product = _repo.GetProductById(id);
        return product != null ? ConvertToProductModel(product) : null;
    }

    private static ProductModel ConvertToProductModel(Product product)
    {
        var productToAdd = new ProductModel
        {
            Id = product.Id,
            Category = product.Category,
            Description = product.Description,
            ImgUrl = product.ImgUrl,
            Name = product.Name,
            Price = product.Price
        };
        var rating = product.Rating;
        if (rating != null)
        {
            productToAdd.Rating = rating.AggregateRating;
            productToAdd.NumberOfRatings = rating.NumberOfRatings;
        }

        return productToAdd;
    }
    public async Task<ProductModel> AddNewProductAsync(ProductModel productToAdd, bool invalidateCache)
    {
        var product = new Product
        {
            Category = productToAdd.Category,
            Description = productToAdd.Description,
            ImgUrl = productToAdd.ImgUrl,
            Name = productToAdd.Name,
            Price = productToAdd.Price
        };
        var addedProduct = await _repo.AddNewProductAsync(product, invalidateCache);
        return ConvertToProductModel(addedProduct);
    }
}