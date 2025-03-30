using ChieuT4_Nhom05_WebQLCF.Data;
using ChieuT4_Nhom05_WebQLCF.Models;
using ChieuT4_Nhom05_WebQLCF.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using X.PagedList;

namespace ChieuT4_Nhom05_WebQLCF.Areas.Customer.Controllers
{
    [Area("Customer")]

    public class HomeController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _db;

        public HomeController(IProductRepository productRepository, ICategoryRepository categoryRepository, ILogger<HomeController> logger, ApplicationDbContext db, ApplicationDbContext context)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;

            _logger = logger;
            _db = db;
            _context = context;
        }

        public async Task<IActionResult> Index(int? page)
        {
            if (page == null)
            {
                page = 1;
            }
            int pageSize = 8;
            int pageNum = page ?? 1;

            var products = await _productRepository.GetAllAsync();
            foreach (var product in products)
            {
                if (product.CategoryId != null)
                {
                    product.Category = await _categoryRepository.GetByIdAsync(product.CategoryId);
                }
            }
            var pagedList = products.ToPagedList(pageNum, pageSize);
            return View(pagedList);
        }

        /*[HttpGet("display/{id}", Name = "diplay")]
        public async Task<IActionResult> Display(int id)
        {
            try
            {
                var product = await _context.Products
                    .Include(p => p.Category) // Thêm Include cho Category
                    .FirstOrDefaultAsync(p => p.Id == id);
                if (product == null)
                {
                    return NotFound();
                }

                var options = new JsonSerializerOptions
                {
                    ReferenceHandler = ReferenceHandler.Preserve
                };

                return Json(product, options);
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal server error");
            }
        }*/

        public async Task<IActionResult> Display(int id)
        {
            var product = await _context.Products
        .Include(p => p.Category) // Thêm Include cho Category
        .FirstOrDefaultAsync(p => p.Id == id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
         

        // Trong HomeController ở khu vực Customer
        public async Task<IActionResult> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return View("SearchResults", new List<Product>()); // Trả về view với danh sách trống nếu không có truy vấn
            }

            var products = await _productRepository.SearchByNameAsync(query);
            return View("SearchResults", products); // Trả về view với model là danh sách sản phẩm
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
