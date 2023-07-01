using System.Diagnostics;
using System.Text;
using System.Text.Json;
using CarvedRock.Data.Entities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace CarvedRock.Data
{
    public class CarvedRockRepository : ICarvedRockRepository
    {
        private readonly LocalContext _ctx;
        private readonly ILogger<CarvedRockRepository> _logger;
        private readonly IMemoryCache _memoryCache;
        private readonly IDistributedCache _distributedCache;
        private readonly ILogger _factoryLogger;

        public CarvedRockRepository(LocalContext ctx, ILogger<CarvedRockRepository> logger,
            ILoggerFactory loggerFactory, IMemoryCache memoryCache, IDistributedCache distributedCache)
        {
            _ctx = ctx;
            _logger = logger;
            _memoryCache = memoryCache;
            _distributedCache = distributedCache;
            _factoryLogger = loggerFactory.CreateLogger("DataAccessLayer");
        }
        public async Task<List<Product>> GetProductsAsync(string category, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting products in repository for {category}", category);
            //if (category == "clothing")
            //{
            //    var ex = new ApplicationException("Database error occurred!!");
            //    ex.Data.Add("Category", category);
            //    throw ex;
            //}
            //if (category == "equip")
            //{
            //    throw new SqliteException("Simulated fatal database error occurred!", 551);
            //}
            try
            {
                var cacheKey = $"products_{category}";
                //if (!_memoryCache.TryGetValue(cacheKey, out List<Product> results))
                //{
                //    Thread.Sleep(6000);
                //    results = await _ctx.Products.Where(p => p.Category == category || category == "all")
                //    .Include(p => p.Rating).ToListAsync();
                //    _memoryCache.Set(cacheKey, results,TimeSpan.FromMinutes(2));
                //}
                var distResult = await _distributedCache.GetAsync(cacheKey,cancellationToken);
                if (distResult == null)
                {
                    await Task.Delay(6000, cancellationToken);
                    var productToSerialize = await _ctx.Products.Where(p => p.Category == category || category == "all")
                   .Include(p => p.Rating).ToListAsync(cancellationToken);
                    var serialized = JsonSerializer.Serialize(productToSerialize, CacheSourceGenerationContext.Default.ListProduct);

                    await _distributedCache.SetAsync(cacheKey, Encoding.UTF8.GetBytes(serialized), new DistributedCacheEntryOptions()
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(60)
                    },cancellationToken); ;
                    return productToSerialize;
                }
                var results = JsonSerializer.Deserialize(Encoding.UTF8.GetString(distResult), CacheSourceGenerationContext.Default.ListProduct);
                return results ?? new List<Product>();
            }
            catch (Exception ex)
            {
                Thread.Sleep(6000);
                var newEx = new ApplicationException("Something bad happened in database", ex);
                newEx.Data.Add("Category", category);
                throw newEx;
            }
        }

        public async Task<Product?> GetProductByIdAsync(int id)
        {
            return await _ctx.Products.FindAsync(id);
        }

        public async Task<List<Product>> GetProductsListAsync(string category)
        {
            return await _ctx.Products.Where(p => p.Category == category || category == "all").ToListAsync();
        }

        public Product? GetProductById(int id)
        {
            var timer = new Stopwatch();
            timer.Start();

            var product = _ctx.Products.Find(id);
            timer.Stop();

            _logger.LogDebug("Querying products for {id} finished in {milliseconds} milliseconds",
                id, timer.ElapsedMilliseconds);

            _factoryLogger.LogInformation("(F) Querying products for {id} finished in {ticks} ticks",
                id, timer.ElapsedTicks);

            return product;
        }
        public async Task<Product> AddNewProductAsync(Product product, bool invalidateCache)
        {
            _ctx.Products.Add(product);
            await _ctx.SaveChangesAsync();

            if (invalidateCache)
            {
                var cacheKey = $"products_{product.Category}";
                await _distributedCache.RemoveAsync(cacheKey);
            }
            return product;
        }
    }
}
