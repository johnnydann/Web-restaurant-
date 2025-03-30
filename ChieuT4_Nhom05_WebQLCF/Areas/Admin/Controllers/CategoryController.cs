using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ChieuT4_Nhom05_WebQLCF.Models;
using ChieuT4_Nhom05_WebQLCF.Repositories;
using Microsoft.EntityFrameworkCore;
using ChieuT4_Nhom05_WebQLCF.Data;
using X.PagedList;

namespace ChieuT4_Nhom05_WebQLCF.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
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
        public async Task<IActionResult> IndexCategory()
        {
            var categories = await _categoryRepository.GetAllAsync();
            return View(categories);
        }

        // Hi?n th? form thêm danh m?c m?i
        public IActionResult Add()
        {
            return View();
        }

        // X? lý thêm danh m?c m?i
        [HttpPost]
        public async Task<IActionResult> Add(Category category)
        {
            if (ModelState.IsValid)
            {
                await _categoryRepository.AddAsync(category);
                return RedirectToAction(nameof(IndexCategory));
            }
            return View(category);
        }

        // Hi?n th? thông tin chi ti?t c?a m?t danh m?c
        public async Task<IActionResult> Display(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        // Hi?n th? form c?p nh?t danh m?c
        public async Task<IActionResult> Update(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        // X? lý c?p nh?t danh m?c
        [HttpPost]
        public async Task<IActionResult> Update(int id, Category category)
        {
            if (id != category.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                await _categoryRepository.UpdateAsync(category);
                return RedirectToAction(nameof(IndexCategory));
            }
            return View(category);
        }

        // Hi?n th? form xác nh?n xóa danh m?c
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        // X? lý xóa danh m?c
        [HttpPost, ActionName("Delete")]

        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _categoryRepository.DeleteAsync(id);
            return RedirectToAction(nameof(IndexCategory));
        }
    }
}
