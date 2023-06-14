using System.Diagnostics;
using CarvedRock.Data.Entities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace CarvedRock.Data
{
    public class CarvedRockRepository : ICarvedRockRepository
    {
        private readonly LocalContext _ctx;
        private readonly ILogger<CarvedRockRepository> _logger;
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger _factoryLogger;

        public CarvedRockRepository(LocalContext ctx, ILogger<CarvedRockRepository> logger,
            ILoggerFactory loggerFactory, IMemoryCache memoryCache)
        {
            _ctx = ctx;
            _logger = logger;
            _memoryCache = memoryCache;
            _factoryLogger = loggerFactory.CreateLogger("DataAccessLayer");
        }
        public async Task<List<Product>> GetProductsAsync(string category)
        {
            _logger.LogInformation("Getting products in repository for {category}", category);
            if (category == "clothing")
            {
                var ex = new ApplicationException("Database error occurred!!");
                ex.Data.Add("Category", category);
                throw ex;
            }
            if (category == "equip")
            {
                throw new SqliteException("Simulated fatal database error occurred!", 551);
            }

            try
            {
                Thread.Sleep(6000);
                var cacheKey = $"products_{category}";
                if (!_memoryCache.TryGetValue(cacheKey, out List<Product> results))
                {
                   results = await _ctx.Products.Where(p => p.Category == category || category == "all")
                    .Include(p => p.Rating).ToListAsync();
                    _memoryCache.Set(cacheKey, results,TimeSpan.FromMinutes(2));
                }
                return results;
            }
            catch (Exception ex)
            {
                var newEx = new ApplicationException("Something bad happened in database", ex);
                newEx.Data.Add("Category", category);
                throw newEx;
            }
        }

        public async Task<Product?> GetProductByIdAsync(int id)
        {
            return await _ctx.Products.FindAsync(id);
        }

        public List<Product> GetProducts(string category)
        {
            return _ctx.Products.Where(p => p.Category == category || category == "all").ToList();
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
    }
}
