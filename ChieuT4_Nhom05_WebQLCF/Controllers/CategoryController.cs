using ChieuT4_Nhom05_WebQLCF.Data;
using ChieuT4_Nhom05_WebQLCF.Models;
using ChieuT4_Nhom05_WebQLCF.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using X.PagedList;

namespace ChieuT4_Nhom05_WebQLCF.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly ApplicationDbContext _dbContext;

        public CategoryController(ICategoryRepository categoryRepository, ApplicationDbContext context)
        {
            _categoryRepository = categoryRepository;
            _dbContext = context;
        }

        // Hi?n th? danh sách các danh m?c
        public async Task<IActionResult> IndexCategory(int? page, string Slug = "")
        {
            if (page == null)
            {
                page = 1;
            }
            int pageSize = 8; // Số lượng sản phẩm trên mỗi trang
            int pageNum = page ?? 1;

            Category category = _dbContext.Categories.FirstOrDefault(c => c.Slug == Slug);
            if (category == null)
            {
                return NotFound(); // Trả về trang 404 nếu không tìm thấy danh mục
            }

            // Lấy danh sách sản phẩm thuộc danh mục và phân trang
            var productsByCategory = _dbContext.Products.Where(p => p.CategoryId == category.Id)
                                                         .OrderByDescending(p => p.Id)
                                                         .ToPagedList(pageNum, pageSize);

            return View(productsByCategory);
        }


    }
}
