using CarvedRock.Core;
using CarvedRock.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CarvedRock.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class SyncProductController : ControllerBase
    {
        private readonly ILogger<ProductController> _logger;
        private readonly IProductLogic _productLogic;

        public SyncProductController(IProductLogic productLogic, ILogger<ProductController> logger)
        {
            _productLogic = productLogic;
            _logger = logger;
        }
        [HttpGet]
        public async Task<IEnumerable<ProductModel>> Get(string category = "all")
        {
            using (_logger.BeginScope("ScopeCate : {ScopeCat}", category))
            {
                _logger.LogInformation("Getting products in API");
                return await _productLogic.GetProductsListForCategoryAsync(category);
            }
        }
    }
}
