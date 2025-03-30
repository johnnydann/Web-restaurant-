using ChieuT4_Nhom05_WebQLCF.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChieuT4_Nhom05_WebQLCF.Repositories.Component
{
    public class CategoriesViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;
        public CategoriesViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var categories = await _context.Categories.ToListAsync();
            return View(categories);
        }
    }
}
