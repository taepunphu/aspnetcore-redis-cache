using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using aspnetcore_redis.Data;
using Microsoft.Extensions.Caching.Distributed;
using aspnetcore_redis.helpers;
using System;
using System.Diagnostics;

namespace aspnetcore_redis.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IDistributedCache _cache;
        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, IDistributedCache cache)
        {
            _logger = logger;
            _context = context;
            _cache = cache;
        }

        public async Task<IActionResult> Generate()
        {
            for (int i = 0; i < 100000; i++)
            {
                var model = new Book()
                {
                    Price = 10m,
                    Title = "My book",
                    Publisher = new Publisher()
                    {
                        Name = "kiattisak phanphu"
                    }
                };
                await _context.Books.AddAsync(model);
            }
            await _context.SaveChangesAsync();
            return Ok("generate complete");
        }

        [Obsolete]
        public async Task<IActionResult> Retrive(int id)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            var book = await _cache.GetObjectAsync<Book>("book:" + id);
            if (book == null)
            {
                book = await _context.Books.FindAsync(id);
                await _cache.SetObjectAsync("book:" + book.BookId, book);
            }
            stopWatch.Stop();
            //return Ok(book);
            return Ok(stopWatch.ElapsedMilliseconds);
        }
    }
}
