using ChieuT4_Nhom05_WebQLCF.Data;
using ChieuT4_Nhom05_WebQLCF.Models;
using ChieuT4_Nhom05_WebQLCF.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace ChieuT4_Nhom05_WebQLCF.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly ICategoryRepository _categoryRepository;
        private readonly IProductRepository _productRepository;
        private readonly ApplicationDbContext _dbContext;

        public HomeController(ILogger<HomeController> logger, ICategoryRepository categoryRepository, IProductRepository productRepository, ApplicationDbContext dbContext)
        {
            _logger = logger;
            _categoryRepository = categoryRepository;
            _productRepository = productRepository;
            _dbContext = dbContext;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _productRepository.GetAllAsync();
            foreach (var product in products)
            {
                if (product.CategoryId != null)
                {
                    product.Category = await _categoryRepository.GetByIdAsync(product.CategoryId);
                }
            }
            return View(products);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // Trong HomeController ? khu v?c Customer
        public async Task<IActionResult> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return View("SearchResults", new List<Product>()); // Tr? v? view v?i danh sách tr?ng n?u không có truy v?n
            }

            var products = await _productRepository.SearchByNameAsync(query);
            return View("SearchResults", products); // Tr? v? view v?i model là danh sách s?n ph?m
        }

        [HttpGet]
        public async Task<IActionResult> Suggest(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
            {
                return Json(new List<string>());
            }

            var products = await _productRepository.SearchByNameAsync(term);
            var suggestions = products.Select(p => p.Name).Distinct().ToList();
            return Json(suggestions);
        }
    }
}
