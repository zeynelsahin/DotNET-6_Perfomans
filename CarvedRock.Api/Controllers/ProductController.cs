using CarvedRock.Core;
using CarvedRock.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarvedRock.Api.Controllers;

[ApiController]
[Route("[controller]")]
[AllowAnonymous]
public partial class ProductController : ControllerBase
{
    private readonly ILogger<ProductController> _logger;
    private readonly IProductLogic _productLogic;


    public ProductController(ILogger<ProductController> logger, IProductLogic productLogic)
    {
        _logger = logger;
        _productLogic = productLogic;
    }

    [HttpGet]
    [ResponseCache(Duration = 90, VaryByQueryKeys = new[] { "category" })]
    public async Task<IEnumerable<ProductModel>> Get(string category = "all")
    {
        using (_logger.BeginScope("ScopeCat: {ScopeCat}", category))
        {
            _logger.LogInformation("Getting products in API.");
            return await _productLogic.GetProductsForCategoryAsync(category);
        }
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProductModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(int id)
    {
        _logger.LogDebug("Getting single product in API for {id}", id);
        //var product = _productLogic.GetProductById(id);
        var product = await _productLogic.GetProductByIdAsync(id);
        if (product != null)
        {
            return Ok(product);
        }
        _logger.LogWarning("No product found for ID: {id}", id);
        return NotFound();
    }
    [HttpPost]
    public async Task<IActionResult> Post()
    {
        var newProductModel = new ProductModel()
        {
            Category = "boots",
            Description = "Deneme",
            ImgUrl = "link",
            Name = "name",
            Price = 199
        };

        var addedProduct = await _productLogic.AddNewProductAsync(newProductModel, false);
        return Created($"{Request.Path}/{addedProduct.Id}", addedProduct);
    }
    [HttpPut]
    public async Task<IActionResult> Put()
    {
        var newProductModel = new ProductModel
        {
            Category = "boots",
            Description = "Deneme",
            ImgUrl = "link",
            Name = "name",
            Price = 22.19
        };

        var addedProduct = await _productLogic.AddNewProductAsync(newProductModel, true);
        return Created($"{Request.Path}/{addedProduct.Id}", addedProduct);
    }
}