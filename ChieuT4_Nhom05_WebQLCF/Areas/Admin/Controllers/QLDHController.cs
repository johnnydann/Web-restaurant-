using ChieuT4_Nhom05_WebQLCF.Data;
using ChieuT4_Nhom05_WebQLCF.Models;
using ChieuT4_Nhom05_WebQLCF.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using X.PagedList;

namespace ChieuT4_Nhom05_WebQLCF.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class QLDHController : Controller
    {
        private readonly ApplicationDbContext db;

        public QLDHController(ApplicationDbContext dbContext)
        {
            db = dbContext;
        }

        public async Task<IActionResult> QLDH(int? page)
        {
            if (db == null)
            {
                // Xử lý trường hợp db chưa được khởi tạo
                return NotFound();
            }

            if (page == null)
            {
                page = 1;
            }
            int pageSize = 10;
            int pageNum = page ?? 1;

            var orders = await db.Orders
                    .Include(o => o.OrderDetails) // Include OrderDetails
                    .ThenInclude(od => od.Product) // Include Product trong OrderDetails
                    .OrderByDescending(p => p.Id)
                    .ToListAsync();

            int totalOrders = orders.Count();
            ViewBag.TotalOrders = totalOrders;
            var pagedList = orders.ToPagedList(pageNum, pageSize);
            return View(pagedList);
        }

    }
}